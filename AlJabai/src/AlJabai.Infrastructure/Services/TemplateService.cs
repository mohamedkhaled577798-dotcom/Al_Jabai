using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AlJabai.Core.Models;
using PropertyManagement.Application.Helpers;
using AlJabai.Core.Entities;
using AlJabai.Core.Enums;
using PropertyManagement.Domain.Interfaces.Services;
using PropertyManagement.Infrastructure.Data;
using WaqfGIS.Web.Settings;

namespace AlJabai.Infrastructure.Services;

public class TemplateService : ITemplateService
{
    private readonly AlJabaiDbContext _context;
    private readonly ILogger<TemplateService> _logger;
    private readonly AppSettings _appSettings;
    private readonly IWordTemplateParser _wordTemplateParser;
    private readonly IDocxToPdfConverter _pdfConverter;
    private readonly IFileStorageService _storageService;

    public TemplateService(
        AlJabaiDbContext context,
        IOptions<AppSettings> appSettings,
        ILogger<TemplateService> logger,
        IWordTemplateParser wordTemplateParser,
        IDocxToPdfConverter pdfConverter,
        IFileStorageService storageService)
    {
        _context = context;
        _logger = logger;
        _appSettings = appSettings.Value;
        _wordTemplateParser = wordTemplateParser;
        _pdfConverter = pdfConverter;
        _storageService = storageService;
    }

    public async Task<TemplateParseResult> ParseUploadedFileAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file == null)
        {
            throw new ArgumentNullException(nameof(file));
        }

        if (!file.FileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("يجب أن يكون الملف بصيغة .docx");
        }

        const long maxSize = 10 * 1024 * 1024;
        if (file.Length > maxSize)
        {
            throw new InvalidOperationException("حجم الملف يتجاوز 10 ميجابايت");
        }

        await using var stream = file.OpenReadStream();
        var parseResult = await _wordTemplateParser.ParseVariablesAsync(stream);

        var variables = new List<TemplateParseVariable>();

        foreach (var foundKey in parseResult.KnownVariables)
        {
            var info = TemplateVariables.All.First(v => string.Equals(v.Key, foundKey, StringComparison.OrdinalIgnoreCase));
            variables.Add(new TemplateParseVariable
            {
                Key = info.Key,
                Placeholder = info.Placeholder,
                Label = info.Label,
                Group = info.Group,
                SampleValue = info.SampleValue,
                Status = TemplateVariableStatus.Found
            });
        }

        foreach (var missingKey in parseResult.MissingVariables)
        {
            var info = TemplateVariables.All.First(v => string.Equals(v.Key, missingKey, StringComparison.OrdinalIgnoreCase));
            variables.Add(new TemplateParseVariable
            {
                Key = info.Key,
                Placeholder = info.Placeholder,
                Label = info.Label,
                Group = info.Group,
                SampleValue = info.SampleValue,
                Status = TemplateVariableStatus.Missing
            });
        }

        foreach (var customKey in parseResult.CustomVariables)
        {
            variables.Add(new TemplateParseVariable
            {
                Key = customKey,
                Placeholder = "{{" + customKey + "}}",
                Label = "متغير مخصص",
                Group = "مخصص",
                SampleValue = string.Empty,
                Status = TemplateVariableStatus.Custom
            });
        }

        var sorted = variables
            .OrderBy(v => v.Status)
            .ThenBy(v => v.Group)
            .ThenBy(v => v.Key)
            .ToList();

        return new TemplateParseResult
        {
            IsValid = parseResult.IsValid,
            ValidationMessage = parseResult.ValidationMessage,
            Variables = sorted,
            TotalFound = parseResult.FoundVariables.Count,
            TotalKnown = parseResult.KnownVariables.Count,
            TotalMissing = parseResult.MissingVariables.Count,
            TotalCustom = parseResult.CustomVariables.Count,
            TotalOccurrences = parseResult.TotalVariableOccurrences
        };
    }

    public async Task<byte[]> GenerateContractPdfAsync(Guid contractId, CancellationToken cancellationToken = default)
    {
        var contract = await _context.Contracts
            .Include(c => c.Template)
            .Include(c => c.Tenant)
            .Include(c => c.Property)
            .Include(c => c.Unit)
            .FirstOrDefaultAsync(c => c.Id == contractId, cancellationToken)
            ?? throw new InvalidOperationException("Contract not found.");

        if (string.IsNullOrWhiteSpace(contract.Template.DocxFilePath))
        {
            throw new InvalidOperationException("Template file path not found.");
        }

        await using var templateStream = await _storageService.GetTemplateDocxAsync(contract.Template.DocxFilePath);
        var variables = BuildContractVariables(contract);
        var filledDocx = await _wordTemplateParser.FillTemplateAsync(templateStream, variables);
        return await _pdfConverter.ConvertAsync(filledDocx, cancellationToken);
    }

    public async Task<(byte[] bytes, string fileName)> DownloadTemplateDocxAsync(Guid templateId, CancellationToken cancellationToken = default)
    {
        var template = await _context.ContractTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == templateId, cancellationToken)
            ?? throw new InvalidOperationException("Template not found.");

        if (string.IsNullOrWhiteSpace(template.DocxFilePath))
        {
            throw new InvalidOperationException("Template has no uploaded file.");
        }

        await using var stream = await _storageService.GetTemplateDocxAsync(template.DocxFilePath);
        await using var memory = new MemoryStream();
        await stream.CopyToAsync(memory, cancellationToken);

        var fileName = string.IsNullOrWhiteSpace(template.OriginalFileName)
            ? $"template-{template.Id}.docx"
            : template.OriginalFileName;

        return (memory.ToArray(), fileName);
    }

    private Dictionary<string, string> BuildContractVariables(Contract contract)
    {
        var totalValue = CalculateTotalValue(contract);
        var metadata = DeserializeMetadata(contract.MetadataJson);
        var contractDurationMonths = ((contract.EndDate.Year - contract.StartDate.Year) * 12 + contract.EndDate.Month - contract.StartDate.Month).ToString(CultureInfo.InvariantCulture);
        var contractDurationYears = (contract.EndDate.Year - contract.StartDate.Year).ToString(CultureInfo.InvariantCulture);

        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["tenant_name"] = contract.Tenant.FullName,
            ["tenant_id"] = contract.Tenant.NationalId,
            ["tenant_phone"] = contract.Tenant.Phone,
            ["tenant_email"] = contract.Tenant.Email ?? string.Empty,
            ["tenant_type"] = GetTenantTypeName(contract.Tenant.TenantType),
            ["tenant_address"] = contract.Tenant.Address ?? string.Empty,
            ["tenant_company_name"] = contract.Tenant.CompanyName ?? string.Empty,
            ["tenant_authorized_person"] = contract.Tenant.AuthorizedPerson ?? string.Empty,
            ["tenant_tax_number"] = contract.Tenant.TaxNumber ?? string.Empty,
            ["property_name"] = contract.Property.Name,
            ["property_address"] = contract.Property.Address,
            ["property_city"] = contract.Property.City ?? string.Empty,
            ["property_governorate"] = contract.Property.Governorate ?? string.Empty,
            ["property_type"] = contract.Property.PropertyType ?? string.Empty,
            ["unit_number"] = contract.Unit?.UnitNumber ?? "لا يوجد",
            ["unit_floor"] = contract.Unit?.Floor ?? string.Empty,
            ["unit_area"] = contract.Unit?.Area?.ToString("F0", CultureInfo.InvariantCulture) ?? string.Empty,
            ["contract_number"] = contract.ContractNumber,
            ["contract_type_name"] = GetContractTypeName(contract.ContractType),
            ["start_date"] = contract.StartDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
            ["end_date"] = contract.EndDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
            ["contract_duration_months"] = contractDurationMonths,
            ["contract_duration_years"] = contractDurationYears,
            ["total_contract_years"] = contractDurationYears,
            ["notes"] = contract.Notes ?? string.Empty,
            ["rent_amount"] = contract.RentAmount.ToString("N3", CultureInfo.InvariantCulture),
            ["rent_amount_words"] = ArabicNumberHelper.ConvertToArabicCurrencyWords(contract.RentAmount),
            ["payment_cycle"] = GetPaymentCycleName(contract.PaymentCycle),
            ["payment_day"] = GetOrdinalArabic(contract.PaymentDay),
            ["annual_increase"] = contract.AnnualIncreaseRate.ToString(CultureInfo.InvariantCulture) + "%",
            ["security_deposit"] = contract.SecurityDeposit.ToString("N3", CultureInfo.InvariantCulture),
            ["security_deposit_words"] = ArabicNumberHelper.ConvertToArabicCurrencyWords(contract.SecurityDeposit),
            ["total_contract_value"] = totalValue.ToString("N3", CultureInfo.InvariantCulture),
            ["total_contract_value_words"] = ArabicNumberHelper.ConvertToArabicCurrencyWords(totalValue),
            ["landlord_name"] = _appSettings.LandlordName,
            ["landlord_representative"] = _appSettings.LandlordRepresentative,
            ["landlord_phone"] = _appSettings.LandlordPhone ?? string.Empty,
            ["landlord_address"] = _appSettings.LandlordAddress ?? string.Empty,
            ["today_date"] = DateTime.Now.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
            ["today_date_hijri"] = ConvertToHijri(DateTime.Now),
            ["contract_year"] = contract.StartDate.Year.ToString(CultureInfo.InvariantCulture),
            ["sequential_number"] = contract.ContractNumber.Split('-').LastOrDefault() ?? string.Empty,
            ["project_name"] = metadata.GetValueOrDefault("project_name", string.Empty),
            ["operation_period"] = metadata.GetValueOrDefault("operation_period", string.Empty),
            ["transfer_date"] = metadata.GetValueOrDefault("transfer_date", string.Empty),
            ["commercial_use"] = metadata.GetValueOrDefault("commercial_use", string.Empty),
            ["commercial_license_number"] = metadata.GetValueOrDefault("commercial_license_number", string.Empty),
            ["year_by_year_schedule"] = metadata.GetValueOrDefault("year_by_year_schedule", string.Empty)
        };
    }

    private static Dictionary<string, string> DeserializeMetadata(string? metadataJson)
    {
        if (string.IsNullOrWhiteSpace(metadataJson))
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(metadataJson)
                   ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private static decimal CalculateTotalValue(Contract contract)
    {
        var months = Math.Max(1, ((contract.EndDate.Year - contract.StartDate.Year) * 12 + contract.EndDate.Month - contract.StartDate.Month));
        var multiplier = contract.PaymentCycle switch
        {
            PaymentCycle.Monthly => 1m,
            PaymentCycle.Quarterly => 3m,
            PaymentCycle.SemiAnnual => 6m,
            _ => 12m
        };

        var periods = Math.Max(1m, months / multiplier);
        return contract.RentAmount * periods;
    }

    private static string ConvertToHijri(DateTime date)
    {
        var cal = new UmAlQuraCalendar();
        return $"{cal.GetDayOfMonth(date):D2}/{cal.GetMonth(date):D2}/{cal.GetYear(date)}";
    }

    private static string GetContractTypeName(ContractType contractType) => contractType switch
    {
        ContractType.Monthly => "إيجار شهري",
        ContractType.Annual => "إيجار سنوي",
        ContractType.MultiYear => "متعدد السنوات",
        ContractType.Commercial => "تجاري",
        ContractType.BOT => "BOT",
        ContractType.Usufruct => "انتفاع",
        _ => contractType.ToString()
    };

    private static string GetTenantTypeName(TenantType tenantType) => tenantType switch
    {
        TenantType.Individual => "فرد",
        TenantType.Company => "شركة",
        TenantType.Government => "جهة حكومية",
        _ => tenantType.ToString()
    };

    private static string GetPaymentCycleName(PaymentCycle cycle) => cycle switch
    {
        PaymentCycle.Monthly => "شهري",
        PaymentCycle.Quarterly => "ربع سنوي",
        PaymentCycle.SemiAnnual => "نصف سنوي",
        PaymentCycle.Annual => "سنوي",
        _ => cycle.ToString()
    };

    private static string GetOrdinalArabic(int day) => day switch
    {
        1 => "الأول",
        2 => "الثاني",
        3 => "الثالث",
        4 => "الرابع",
        5 => "الخامس",
        6 => "السادس",
        7 => "السابع",
        8 => "الثامن",
        9 => "التاسع",
        10 => "العاشر",
        11 => "الحادي عشر",
        12 => "الثاني عشر",
        13 => "الثالث عشر",
        14 => "الرابع عشر",
        15 => "الخامس عشر",
        16 => "السادس عشر",
        17 => "السابع عشر",
        18 => "الثامن عشر",
        19 => "التاسع عشر",
        20 => "العشرون",
        21 => "الحادي والعشرون",
        22 => "الثاني والعشرون",
        23 => "الثالث والعشرون",
        24 => "الرابع والعشرون",
        25 => "الخامس والعشرون",
        26 => "السادس والعشرون",
        27 => "السابع والعشرون",
        28 => "الثامن والعشرون",
        29 => "التاسع والعشرون",
        30 => "الثلاثون",
        31 => "الحادي والثلاثون",
        _ => day.ToString(CultureInfo.InvariantCulture)
    };
}
