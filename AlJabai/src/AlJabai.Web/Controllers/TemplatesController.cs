using System.Security.Claims;
using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using AlJabai.Core.Models;
using PropertyManagement.Application.DTOs.Templates;
using AlJabai.Core.Entities;
using AlJabai.Core.Enums;
using PropertyManagement.Domain.Interfaces.Services;
using PropertyManagement.Infrastructure.Data;
using PropertyManagement.Infrastructure.Helpers;
using PropertyManagement.Infrastructure.Services;
using PropertyManagement.Web.Models.Templates;

namespace PropertyManagement.Web.Controllers;

public class TemplatesController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly ITemplateService _templateService;
    private readonly IFileStorageService _storageService;

    public TemplatesController(
        IMediator mediator,
        ApplicationDbContext context,
        ITemplateService templateService,
        IFileStorageService storageService) : base(mediator)
    {
        _context = context;
        _templateService = templateService;
        _storageService = storageService;
    }

    [HttpGet]
    public IActionResult Index() => View(_context.ContractTemplates.OrderByDescending(t => t.CreatedAt).ToList());

    [HttpGet]
    public IActionResult Create()
    {
        var viewModel = new CreateTemplateViewModel
        {
            ContractTypes = GetContractTypeSelectList(),
            AllVariables = TemplateVariables.All.ToList()
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ParseFile(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null)
        {
            return BadRequest(new { error = "لم يتم رفع أي ملف" });
        }

        if (!file.FileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = "يجب أن يكون الملف بصيغة .docx" });
        }

        if (file.Length > 10 * 1024 * 1024)
        {
            return BadRequest(new { error = "حجم الملف يتجاوز 10 ميجابايت" });
        }

        var result = await _templateService.ParseUploadedFileAsync(file, cancellationToken);
        return Ok(MapParseResult(result));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTemplateViewModel model, CancellationToken cancellationToken)
    {
        model.ContractTypes = GetContractTypeSelectList();
        model.AllVariables = TemplateVariables.All.ToList();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (model.Template.DocxFile == null || !model.Template.DocxFile.FileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("Template.DocxFile", "يرجى رفع ملف بصيغة .docx");
            return View(model);
        }

        if (model.Template.DocxFile.Length > 10 * 1024 * 1024)
        {
            ModelState.AddModelError("Template.DocxFile", "حجم الملف يتجاوز 10 ميجابايت");
            return View(model);
        }

        var parseResult = await _templateService.ParseUploadedFileAsync(model.Template.DocxFile, cancellationToken);

        var template = new ContractTemplate
        {
            Id = Guid.NewGuid(),
            Name = model.Template.Name,
            ContractType = model.Template.ContractType,
            OriginalFileName = model.Template.DocxFile.FileName,
            FileSizeBytes = model.Template.DocxFile.Length,
            ExtractedVariablesJson = JsonSerializer.Serialize(parseResult.Variables.Where(v => v.Status == TemplateVariableStatus.Found).Select(v => v.Key)),
            MissingVariablesJson = JsonSerializer.Serialize(parseResult.Variables.Where(v => v.Status == TemplateVariableStatus.Missing).Select(v => v.Key)),
            CustomVariablesJson = JsonSerializer.Serialize(parseResult.Variables.Where(v => v.Status == TemplateVariableStatus.Custom).Select(v => v.Key)),
            RequiredFieldsJson = model.Template.RequiredFieldsJson ?? "[]",
            DefaultClausesJson = model.Template.DefaultClausesJson,
            LastParsedAt = DateTime.UtcNow,
            IsActive = true,
            Version = 1,
            CreatedBy = GetCurrentUserId()
        };

        template.DocxFilePath = await _storageService.SaveTemplateDocxAsync(model.Template.DocxFile, template.Id);

        await _context.ContractTemplates.AddAsync(template, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        AddSuccessAlert($"تم حفظ القالب '{template.Name}' بنجاح مع {parseResult.TotalFound} متغير");
        return RedirectToAction(nameof(Details), new { id = template.Id });
    }

    [HttpGet]
    public IActionResult Edit(Guid id)
    {
        var template = _context.ContractTemplates.FirstOrDefault(t => t.Id == id);
        if (template is null)
        {
            return NotFound();
        }

        return View(new UpdateTemplateDto
        {
            Name = template.Name,
            ContractType = template.ContractType,
            RequiredFieldsJson = template.RequiredFieldsJson,
            DefaultClausesJson = template.DefaultClausesJson,
            IsActive = template.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateTemplateDto dto, CancellationToken cancellationToken)
    {
        var template = await _context.ContractTemplates.FindAsync([id], cancellationToken);
        if (template is null)
        {
            return NotFound();
        }

        template.Name = dto.Name;
        template.ContractType = dto.ContractType;
        template.RequiredFieldsJson = dto.RequiredFieldsJson;
        template.DefaultClausesJson = dto.DefaultClausesJson;
        template.IsActive = dto.IsActive;

        if (dto.DocxFile != null && dto.DocxFile.Length > 0)
        {
            if (!dto.DocxFile.FileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(dto.DocxFile), "يرجى رفع ملف بصيغة .docx");
                return View(dto);
            }

            var parseResult = await _templateService.ParseUploadedFileAsync(dto.DocxFile, cancellationToken);
            template.OriginalFileName = dto.DocxFile.FileName;
            template.FileSizeBytes = dto.DocxFile.Length;
            template.ExtractedVariablesJson = JsonSerializer.Serialize(parseResult.Variables.Where(v => v.Status == TemplateVariableStatus.Found).Select(v => v.Key));
            template.MissingVariablesJson = JsonSerializer.Serialize(parseResult.Variables.Where(v => v.Status == TemplateVariableStatus.Missing).Select(v => v.Key));
            template.CustomVariablesJson = JsonSerializer.Serialize(parseResult.Variables.Where(v => v.Status == TemplateVariableStatus.Custom).Select(v => v.Key));
            template.LastParsedAt = DateTime.UtcNow;
            template.DocxFilePath = await _storageService.SaveTemplateDocxAsync(dto.DocxFile, template.Id);
        }

        await _context.SaveChangesAsync(cancellationToken);
        AddSuccessAlert("تم تحديث القالب");
        return RedirectToAction(nameof(Details), new { id = template.Id });
    }

    [HttpGet]
    public IActionResult Details(Guid id)
    {
        var template = _context.ContractTemplates.FirstOrDefault(t => t.Id == id);
        if (template == null)
        {
            return NotFound();
        }

        var dto = new ContractTemplateDto
        {
            Id = template.Id,
            Name = template.Name,
            ContractType = template.ContractType,
            ContractTypeName = GetContractTypeName(template.ContractType),
            DocxFilePath = template.DocxFilePath,
            OriginalFileName = template.OriginalFileName,
            FileSizeBytes = template.FileSizeBytes,
            ExtractedVariables = DeserializeStringList(template.ExtractedVariablesJson),
            MissingVariables = DeserializeStringList(template.MissingVariablesJson),
            CustomVariables = DeserializeStringList(template.CustomVariablesJson),
            Version = template.Version,
            IsActive = template.IsActive,
            LastParsedAt = template.LastParsedAt,
            CreatedAt = template.CreatedAt
        };

        ViewBag.VariableCatalog = TemplateVariables.All.ToList();
        return View(dto);
    }

    [HttpGet]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        var (bytes, fileName) = await _templateService.DownloadTemplateDocxAsync(id, cancellationToken);
        return File(bytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileName);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reparse(Guid id, CancellationToken cancellationToken)
    {
        var template = await _context.ContractTemplates.FindAsync([id], cancellationToken);
        if (template == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(template.DocxFilePath))
        {
            return BadRequest(new { error = "لا يوجد ملف قالب مرتبط" });
        }

        await using var stream = await _storageService.GetTemplateDocxAsync(template.DocxFilePath);
        var fileName = string.IsNullOrWhiteSpace(template.OriginalFileName) ? "template.docx" : template.OriginalFileName;
        var formFile = new FormFileWrapper(stream, fileName);
        var result = await _templateService.ParseUploadedFileAsync(formFile, cancellationToken);

        template.ExtractedVariablesJson = JsonSerializer.Serialize(result.Variables.Where(v => v.Status == TemplateVariableStatus.Found).Select(v => v.Key));
        template.MissingVariablesJson = JsonSerializer.Serialize(result.Variables.Where(v => v.Status == TemplateVariableStatus.Missing).Select(v => v.Key));
        template.CustomVariablesJson = JsonSerializer.Serialize(result.Variables.Where(v => v.Status == TemplateVariableStatus.Custom).Select(v => v.Key));
        template.LastParsedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return Ok(MapParseResult(result));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var template = await _context.ContractTemplates.FindAsync([id], cancellationToken);
        if (template == null)
        {
            return NotFound();
        }

        if (!string.IsNullOrWhiteSpace(template.DocxFilePath))
        {
            await _storageService.DeleteFileAsync(template.DocxFilePath);
        }

        template.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
        AddSuccessAlert("تم حذف القالب");

        return RedirectToAction(nameof(Index));
    }

    private static List<SelectListItem> GetContractTypeSelectList() =>
        Enum.GetValues<ContractType>()
            .Select(type => new SelectListItem
            {
                Value = ((int)type).ToString(),
                Text = GetContractTypeName(type)
            })
            .ToList();

    private static string GetContractTypeName(ContractType type) => type switch
    {
        ContractType.Monthly => "شهري",
        ContractType.Annual => "سنوي",
        ContractType.MultiYear => "متعدد السنوات",
        ContractType.Commercial => "تجاري",
        ContractType.BOT => "BOT",
        ContractType.Usufruct => "انتفاع",
        _ => type.ToString()
    };

    private static List<string> DeserializeStringList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<string>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    private static ParseResultDto MapParseResult(TemplateParseResult result)
    {
        return new ParseResultDto
        {
            IsValid = result.IsValid,
            ValidationMessage = result.ValidationMessage,
            TotalFound = result.TotalFound,
            TotalKnown = result.TotalKnown,
            TotalMissing = result.TotalMissing,
            TotalCustom = result.TotalCustom,
            TotalOccurrences = result.TotalOccurrences,
            Variables = result.Variables.Select(v => new VariableStatusDto
            {
                Key = v.Key,
                Placeholder = v.Placeholder,
                Label = v.Label,
                Group = v.Group,
                SampleValue = v.SampleValue,
                Status = v.Status switch
                {
                    TemplateVariableStatus.Found => VariableStatus.Found,
                    TemplateVariableStatus.Missing => VariableStatus.Missing,
                    _ => VariableStatus.Custom
                }
            }).ToList()
        };
    }

    private Guid? GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userId, out var guid) ? guid : null;
    }
}
