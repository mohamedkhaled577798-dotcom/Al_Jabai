using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WaqfSystem.Application.DTOs.Common;
using WaqfSystem.Application.DTOs.Partnership;
using WaqfSystem.Core.Entities;
using WaqfSystem.Core.Enums;
using WaqfSystem.Core.Interfaces;

namespace WaqfSystem.Application.Services
{
    public class PartnershipService : IPartnershipService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileStorageService _fileStorageService;
        private readonly IPartnerCommunicationService _partnerCommunicationService;
        private readonly INotificationService _notificationService;
        private readonly IReportService _reportService;
        private readonly ILogger<PartnershipService> _logger;

        public PartnershipService(
            IUnitOfWork unitOfWork,
            IFileStorageService fileStorageService,
            IPartnerCommunicationService partnerCommunicationService,
            INotificationService notificationService,
            IReportService reportService,
            ILogger<PartnershipService> logger)
        {
            _unitOfWork = unitOfWork;
            _fileStorageService = fileStorageService;
            _partnerCommunicationService = partnerCommunicationService;
            _notificationService = notificationService;
            _reportService = reportService;
            _logger = logger;
        }

        public async Task<int> CreateAsync(CreatePartnershipDto dto, IFormFile? agreementFile, int userId)
        {
            try
            {
                var property = await _unitOfWork.GetQueryable<Property>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == dto.PropertyId && !x.IsDeleted);

                if (property == null)
                {
                    throw new ValidationException("العقار غير موجود");
                }

                await ValidateCreateOrUpdateAsync(dto, property, null);

                var agreementUrl = agreementFile != null
                    ? await _fileStorageService.UploadFileAsync(agreementFile, "partnership-agreements")
                    : null;

                var entity = new PropertyPartnership
                {
                    PropertyId = dto.PropertyId,
                    PartnershipType = dto.PartnershipType,
                    WaqfSharePercent = NormalizePercent(dto.WaqfSharePercent),
                    PartnerSharePercent = 100m - NormalizePercent(dto.WaqfSharePercent),
                    OwnedFloorNumbers = SerializeOrNull(dto.OwnedFloorNumbers),
                    OwnedUnitIds = SerializeOrNull(dto.OwnedUnitIds),
                    UsufructStartDate = dto.UsufructStartDate?.Date,
                    UsufructEndDate = dto.UsufructEndDate?.Date,
                    UsufructTermYears = CalculateTermYears(dto.UsufructStartDate, dto.UsufructEndDate),
                    UsufructAnnualFeePerYear = dto.UsufructAnnualFeePerYear,
                    PartnershipStartDate = dto.PartnershipStartDate?.Date,
                    PartnershipEndDate = dto.PartnershipEndDate?.Date,
                    LandSharePercentWaqf = dto.LandSharePercentWaqf,
                    LandTotalDunum = dto.LandTotalDunum,
                    WaqfLandDunum = CalculateWaqfLand(dto.LandSharePercentWaqf, dto.LandTotalDunum),
                    WaqfHarvestPercent = dto.WaqfHarvestPercent,
                    FarmerName = dto.FarmerName,
                    FarmerNationalId = dto.FarmerNationalId,
                    HarvestContractType = dto.HarvestContractType,
                    CustomPartnershipName = dto.CustomPartnershipName,
                    CustomCalculationFormula = dto.CustomCalculationFormula,
                    PartnerName = dto.PartnerName,
                    PartnerNameEn = dto.PartnerNameEn,
                    PartnerType = dto.PartnerType,
                    PartnerNationalId = dto.PartnerNationalId,
                    PartnerRegistrationNo = dto.PartnerRegistrationNo,
                    PartnerPhone = dto.PartnerPhone,
                    PartnerPhone2 = dto.PartnerPhone2,
                    PartnerEmail = dto.PartnerEmail,
                    PartnerWhatsApp = dto.PartnerWhatsApp,
                    PartnerAddress = dto.PartnerAddress,
                    PartnerBankName = dto.PartnerBankName,
                    PartnerBankIBAN = dto.PartnerBankIBAN,
                    PartnerBankAccountNo = dto.PartnerBankAccountNo,
                    PartnerBankBranch = dto.PartnerBankBranch,
                    AgreementDate = dto.AgreementDate?.Date,
                    AgreementNotaryName = dto.AgreementNotaryName,
                    AgreementCourt = dto.AgreementCourt,
                    AgreementReferenceNo = dto.AgreementReferenceNo,
                    AgreementDocUrl = agreementUrl,
                    RevenueDistribMethod = dto.RevenueDistribMethod,
                    ExpenseBearingMethod = dto.ExpenseBearingMethod,
                    RevenueDistribDay = dto.RevenueDistribDay,
                    NextDistribDate = ComputeNextDistribDate(dto.RevenueDistribMethod, dto.RevenueDistribDay, DateTime.Today),
                    IsActive = true,
                    CreatedById = userId,
                    CreatedAt = DateTime.UtcNow,
                    Notes = dto.Notes,
                    ContactPhone = dto.PartnerPhone,
                    ContactEmail = dto.PartnerEmail
                };

                await _unitOfWork.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                await SyncConditionRulesAsync(entity.Id, dto.ConditionRules, userId);

                await CreateExpirySchedulesAsync(entity);
                await WriteAuditAsync("PropertyPartnerships", entity.Id, "INSERT", null, JsonSerializer.Serialize(entity), userId);

                await _notificationService.SendToRoleAsync("REGIONAL_MGR", "شراكة جديدة", $"تم إنشاء شراكة جديدة للعقار رقم {property.WqfNumber}", "PropertyPartnerships", entity.Id);
                await _notificationService.SendToRoleAsync("CONTRACTS_MGR", "شراكة جديدة", $"تم إنشاء شراكة جديدة مع {entity.PartnerName}", "PropertyPartnerships", entity.Id);

                return entity.Id;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create partnership failed");
                throw;
            }
        }

        public async Task UpdateAsync(UpdatePartnershipDto dto, IFormFile? agreementFile, int userId)
        {
            try
            {
                var entity = await _unitOfWork.GetQueryable<PropertyPartnership>()
                    .FirstOrDefaultAsync(x => x.Id == dto.Id && !x.IsDeleted);

                if (entity == null)
                {
                    throw new ValidationException("الشراكة غير موجودة");
                }

                var propertyId = dto.PropertyId.HasValue ? (int)dto.PropertyId.Value : entity.PropertyId;
                var property = await _unitOfWork.GetQueryable<Property>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == propertyId && !x.IsDeleted);

                if (property == null)
                {
                    throw new ValidationException("العقار غير موجود");
                }

                var merged = MergeForValidation(entity, dto);
                await ValidateCreateOrUpdateAsync(merged, property, entity.Id);

                var oldValues = JsonSerializer.Serialize(entity);

                if (agreementFile != null)
                {
                    entity.AgreementDocUrl = await _fileStorageService.UploadFileAsync(agreementFile, "partnership-agreements");
                }

                entity.PropertyId = propertyId;
                entity.PartnershipType = merged.PartnershipType;
                entity.WaqfSharePercent = NormalizePercent(merged.WaqfSharePercent);
                entity.PartnerSharePercent = 100m - NormalizePercent(merged.WaqfSharePercent);
                entity.OwnedFloorNumbers = SerializeOrNull(merged.OwnedFloorNumbers);
                entity.OwnedUnitIds = SerializeOrNull(merged.OwnedUnitIds);
                entity.UsufructStartDate = merged.UsufructStartDate?.Date;
                entity.UsufructEndDate = merged.UsufructEndDate?.Date;
                entity.UsufructTermYears = CalculateTermYears(merged.UsufructStartDate, merged.UsufructEndDate);
                entity.UsufructAnnualFeePerYear = merged.UsufructAnnualFeePerYear;
                entity.PartnershipStartDate = merged.PartnershipStartDate?.Date;
                entity.PartnershipEndDate = merged.PartnershipEndDate?.Date;
                entity.LandSharePercentWaqf = merged.LandSharePercentWaqf;
                entity.LandTotalDunum = merged.LandTotalDunum;
                entity.WaqfLandDunum = CalculateWaqfLand(merged.LandSharePercentWaqf, merged.LandTotalDunum);
                entity.WaqfHarvestPercent = merged.WaqfHarvestPercent;
                entity.FarmerName = merged.FarmerName;
                entity.FarmerNationalId = merged.FarmerNationalId;
                entity.HarvestContractType = merged.HarvestContractType;
                entity.CustomPartnershipName = merged.CustomPartnershipName;
                entity.CustomCalculationFormula = merged.CustomCalculationFormula;
                entity.PartnerName = merged.PartnerName;
                entity.PartnerNameEn = merged.PartnerNameEn;
                entity.PartnerType = merged.PartnerType;
                entity.PartnerNationalId = merged.PartnerNationalId;
                entity.PartnerRegistrationNo = merged.PartnerRegistrationNo;
                entity.PartnerPhone = merged.PartnerPhone;
                entity.PartnerPhone2 = merged.PartnerPhone2;
                entity.PartnerEmail = merged.PartnerEmail;
                entity.PartnerWhatsApp = merged.PartnerWhatsApp;
                entity.PartnerAddress = merged.PartnerAddress;
                entity.PartnerBankName = merged.PartnerBankName;
                entity.PartnerBankIBAN = merged.PartnerBankIBAN;
                entity.PartnerBankAccountNo = merged.PartnerBankAccountNo;
                entity.PartnerBankBranch = merged.PartnerBankBranch;
                entity.AgreementDate = merged.AgreementDate?.Date;
                entity.AgreementNotaryName = merged.AgreementNotaryName;
                entity.AgreementCourt = merged.AgreementCourt;
                entity.AgreementReferenceNo = merged.AgreementReferenceNo;
                entity.RevenueDistribMethod = merged.RevenueDistribMethod;
                entity.ExpenseBearingMethod = merged.ExpenseBearingMethod;
                entity.RevenueDistribDay = merged.RevenueDistribDay;
                entity.NextDistribDate = ComputeNextDistribDate(merged.RevenueDistribMethod, merged.RevenueDistribDay, entity.LastDistribDate ?? DateTime.Today);
                entity.Notes = merged.Notes;
                entity.ContactPhone = merged.PartnerPhone;
                entity.ContactEmail = merged.PartnerEmail;
                entity.UpdatedById = userId;
                entity.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                if (dto.ConditionRules != null)
                {
                    await SyncConditionRulesAsync(entity.Id, dto.ConditionRules, userId);
                }

                await CreateExpirySchedulesAsync(entity);
                await WriteAuditAsync("PropertyPartnerships", entity.Id, "UPDATE", oldValues, JsonSerializer.Serialize(entity), userId);
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update partnership failed");
                throw;
            }
        }

        public async Task DeactivateAsync(int id, string reason, int userId)
        {
            try
            {
                var entity = await _unitOfWork.GetQueryable<PropertyPartnership>()
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (entity == null)
                {
                    throw new ValidationException("الشراكة غير موجودة");
                }

                var oldValues = JsonSerializer.Serialize(entity);
                entity.IsActive = false;
                entity.DeactivationReason = reason;
                entity.DeactivatedAt = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedById = userId;

                await _unitOfWork.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                await WriteAuditAsync("PropertyPartnerships", entity.Id, "DEACTIVATE", oldValues, JsonSerializer.Serialize(entity), userId);
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Deactivate partnership failed");
                throw;
            }
        }

        public async Task<RevenueDistributionDto> RecordDistributionAsync(RevenueDistributionCreateDto dto, int userId)
        {
            try
            {
                var partnership = await _unitOfWork.GetQueryable<PropertyPartnership>()
                    .Include(x => x.Property)
                    .FirstOrDefaultAsync(x => x.Id == dto.PartnershipId && !x.IsDeleted);

                if (partnership == null)
                {
                    throw new ValidationException("الشراكة غير موجودة");
                }

                if (!partnership.IsActive)
                {
                    throw new ValidationException("لا يمكن تسجيل توزيع لشراكة غير نشطة");
                }

                var calc = await CalculateRevenueAsync(partnership, dto.TotalRevenue, dto.TotalExpenses, dto.DistributionType, DateTime.Today, dto.SeasonLabel);

                if (Math.Abs((calc.WaqfAmount + calc.PartnerAmount) - calc.NetRevenue) >= 0.01m)
                {
                    throw new InvalidOperationException("Revenue calculation mismatch");
                }

                var distribution = new PartnerRevenueDistribution
                {
                    PartnershipId = partnership.Id,
                    PropertyId = partnership.PropertyId,
                    PeriodLabel = dto.PeriodLabel,
                    PeriodStartDate = dto.PeriodStartDate.Date,
                    PeriodEndDate = dto.PeriodEndDate.Date,
                    DistributionType = dto.DistributionType,
                    TotalRevenue = dto.TotalRevenue,
                    TotalExpenses = dto.TotalExpenses,
                    NetRevenue = calc.NetRevenue,
                    WaqfAmount = calc.WaqfAmount,
                    PartnerAmount = calc.PartnerAmount,
                    WaqfPercentSnapshot = calc.WaqfPercent,
                    TransferStatus = TransferStatus.Pending,
                    TransferMethod = dto.TransferMethod,
                    Notes = dto.Notes,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = userId
                };

                await _unitOfWork.AddAsync(distribution);

                partnership.LastDistribDate = DateTime.Today;
                partnership.NextDistribDate = ComputeNextDistribDate(partnership.RevenueDistribMethod, partnership.RevenueDistribDay, DateTime.Today);
                partnership.UpdatedAt = DateTime.UtcNow;
                partnership.UpdatedById = userId;

                await _unitOfWork.UpdateAsync(partnership);
                await _unitOfWork.SaveChangesAsync();

                await _partnerCommunicationService.SendDistributionNotificationAsync(distribution, partnership, userId);
                await _notificationService.SendToRoleAsync("CONTRACTS_MGR", "توزيع جديد", $"تم تسجيل توزيع جديد للشراكة {partnership.Id}", "PartnerRevenueDistributions", distribution.Id);
                await _notificationService.SendToRoleAsync("REGIONAL_MGR", "توزيع جديد", $"تم تسجيل توزيع جديد للعقار {partnership.Property.WqfNumber}", "PartnerRevenueDistributions", distribution.Id);

                return MapDistribution(distribution, partnership);
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Record distribution failed");
                throw;
            }
        }

        public async Task MarkTransferredAsync(int distributionId, string method, string reference, int userId)
        {
            try
            {
                var dist = await _unitOfWork.GetQueryable<PartnerRevenueDistribution>()
                    .Include(x => x.Partnership)
                    .FirstOrDefaultAsync(x => x.Id == distributionId);

                if (dist == null)
                {
                    throw new ValidationException("سجل التوزيع غير موجود");
                }

                dist.TransferStatus = TransferStatus.Transferred;
                dist.TransferDate = DateTime.Today;
                dist.TransferMethod = method;
                dist.TransferReference = reference;

                await _unitOfWork.UpdateAsync(dist);
                await _unitOfWork.SaveChangesAsync();

                await _partnerCommunicationService.SendTransferConfirmationAsync(dist, dist.Partnership, userId);

                var log = new PartnerContactLog
                {
                    PartnershipId = dist.PartnershipId,
                    ContactType = ContactType.Email,
                    ContactDirection = ContactDirection.Outgoing,
                    Subject = "تأكيد تحويل",
                    MessageBody = $"تم تحويل مستحقاتكم برقم مرجع {reference}",
                    RecipientAddress = dist.Partnership.PartnerEmail,
                    SentAt = DateTime.UtcNow,
                    SentById = userId,
                    IsAutomatic = true,
                    DeliveryStatus = "Sent",
                    LinkedDistributionId = dist.Id
                };

                await _unitOfWork.AddAsync(log);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mark transferred failed");
                throw;
            }
        }

        public async Task<RevenueCalculationResultDto> PreviewRevenueCalculationAsync(int partnershipId, decimal totalRevenue, decimal totalExpenses = 0m, string? distributionType = null, string? seasonLabel = null)
        {
            try
            {
                var partnership = await _unitOfWork.GetQueryable<PropertyPartnership>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == partnershipId && !x.IsDeleted);

                if (partnership == null)
                {
                    throw new ValidationException("الشراكة غير موجودة");
                }

                var calc = await CalculateRevenueAsync(partnership, totalRevenue, totalExpenses, distributionType ?? "Revenue", DateTime.Today, seasonLabel);
                return calc;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Preview revenue failed");
                throw;
            }
        }

        public async Task<string> SendCommunicationAsync(SendCommunicationDto dto, int userId)
        {
            try
            {
                var partnership = await _unitOfWork.GetQueryable<PropertyPartnership>()
                    .Include(x => x.Property)
                    .FirstOrDefaultAsync(x => x.Id == dto.PartnershipId && !x.IsDeleted);

                if (partnership == null)
                {
                    throw new ValidationException("الشراكة غير موجودة");
                }

                string deliveryRef;
                switch (dto.ContactType)
                {
                    case ContactType.SMS:
                        deliveryRef = await _partnerCommunicationService.SendManualMessageAsync(dto, partnership, userId) ? "SENT" : "FAILED";
                        break;
                    case ContactType.WhatsApp:
                        deliveryRef = await _partnerCommunicationService.SendManualMessageAsync(dto, partnership, userId) ? "SENT" : "FAILED";
                        break;
                    case ContactType.Email:
                        deliveryRef = await _partnerCommunicationService.SendManualMessageAsync(dto, partnership, userId) ? "SENT" : "FAILED";
                        break;
                    case ContactType.PDF:
                        deliveryRef = await _partnerCommunicationService.SendManualMessageAsync(dto, partnership, userId) ? "SENT" : "FAILED";
                        break;
                    default:
                        deliveryRef = "LOGGED";
                        break;
                }

                var log = new PartnerContactLog
                {
                    PartnershipId = partnership.Id,
                    ContactType = dto.ContactType,
                    ContactDirection = ContactDirection.Outgoing,
                    Subject = dto.Subject,
                    MessageBody = dto.MessageBody,
                    RecipientAddress = ResolveRecipient(dto.ContactType, partnership),
                    SentAt = DateTime.UtcNow,
                    SentById = userId,
                    IsAutomatic = false,
                    DeliveryStatus = deliveryRef == "FAILED" ? "Failed" : "Sent",
                    ExternalMessageId = deliveryRef,
                    LinkedDistributionId = dto.LinkedDistributionId
                };

                await _unitOfWork.AddAsync(log);
                await _unitOfWork.SaveChangesAsync();

                return deliveryRef;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Send communication failed");
                throw;
            }
        }

        public async Task ProcessExpiryNotificationsAsync()
        {
            try
            {
                var today = DateTime.Today;
                var pending = await _unitOfWork.GetQueryable<PartnerNotificationSchedule>()
                    .Include(x => x.Partnership)
                    .Where(x => !x.IsSent && x.TriggerDate <= today)
                    .ToListAsync();

                foreach (var schedule in pending)
                {
                    var channels = ParseChannels(schedule.Channels);
                    var daysRemaining = (schedule.Partnership.PartnershipEndDate ?? schedule.Partnership.UsufructEndDate)?.Date.Subtract(today).Days ?? 0;

                    if (channels.Contains("Email") || channels.Contains("SMS") || channels.Contains("WhatsApp") || channels.Contains("Internal"))
                    {
                        await _partnerCommunicationService.SendExpiryWarningAsync(schedule.Partnership, daysRemaining, 1);
                    }

                    schedule.IsSent = true;
                    schedule.SentAt = DateTime.UtcNow;
                    await _unitOfWork.UpdateAsync(schedule);
                }

                var activeWithEndDate = await _unitOfWork.GetQueryable<PropertyPartnership>()
                    .Where(x => x.IsActive && !x.IsDeleted && (x.PartnershipEndDate != null || x.UsufructEndDate != null))
                    .ToListAsync();

                foreach (var p in activeWithEndDate)
                {
                    await CreateExpirySchedulesAsync(p);
                }

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Process expiry notifications failed");
                throw;
            }
        }

        public async Task<PartnerStatementDto> GetStatementAsync(int partnershipId, DateTime from, DateTime to)
        {
            try
            {
                var partnership = await _unitOfWork.GetQueryable<PropertyPartnership>()
                    .Include(x => x.Property)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == partnershipId && !x.IsDeleted);

                if (partnership == null)
                {
                    throw new ValidationException("الشراكة غير موجودة");
                }

                var distributions = await _unitOfWork.GetQueryable<PartnerRevenueDistribution>()
                    .AsNoTracking()
                    .Where(x => x.PartnershipId == partnership.Id && x.PeriodStartDate >= from.Date && x.PeriodEndDate <= to.Date)
                    .OrderByDescending(x => x.PeriodStartDate)
                    .ToListAsync();

                var mapped = distributions.Select(x => MapDistribution(x, partnership)).ToList();

                return new PartnerStatementDto
                {
                    PartnerName = partnership.PartnerName,
                    PropertyNameAr = partnership.Property.PropertyName ?? partnership.Property.WqfNumber,
                    WqfNumber = partnership.Property.WqfNumber,
                    PartnershipType = partnership.PartnershipType,
                    PeriodFrom = from,
                    PeriodTo = to,
                    TotalRevenue = distributions.Sum(x => x.TotalRevenue),
                    TotalWaqfAmount = distributions.Sum(x => x.WaqfAmount),
                    TotalPartnerAmount = distributions.Sum(x => x.PartnerAmount),
                    Distributions = mapped,
                    TransferHistory = mapped.Where(x => x.TransferStatus == TransferStatus.Transferred).ToList(),
                    PendingAmount = distributions.Where(x => x.TransferStatus == TransferStatus.Pending).Sum(x => x.PartnerAmount)
                };
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get statement failed");
                throw;
            }
        }

        public async Task<List<PartnershipDetailDto>> GetByPropertyAsync(int propertyId)
        {
            try
            {
                var items = await _unitOfWork.GetQueryable<PropertyPartnership>()
                    .Include(x => x.Property)
                    .Where(x => x.PropertyId == propertyId && !x.IsDeleted)
                    .AsNoTracking()
                    .ToListAsync();

                var result = new List<PartnershipDetailDto>();
                foreach (var item in items)
                {
                    var detail = await BuildDetailDtoAsync(item);
                    result.Add(detail);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByProperty failed");
                throw;
            }
        }

        public async Task<PartnershipDetailDto?> GetByIdAsync(int id)
        {
            try
            {
                var item = await _unitOfWork.GetQueryable<PropertyPartnership>()
                    .Include(x => x.Property)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                return item == null ? null : await BuildDetailDtoAsync(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetById failed");
                throw;
            }
        }

        public async Task<PagedResult<PartnershipListItemDto>> GetPagedAsync(PartnershipFilterRequest filter)
        {
            try
            {
                var query = _unitOfWork.GetQueryable<PropertyPartnership>()
                    .Include(x => x.Property)
                    .Where(x => !x.IsDeleted)
                    .AsNoTracking();

                if (filter.GovernorateId.HasValue)
                {
                    query = query.Where(x => x.Property.GovernorateId == filter.GovernorateId.Value);
                }

                if (filter.PartnershipType.HasValue)
                {
                    query = query.Where(x => x.PartnershipType == filter.PartnershipType.Value);
                }

                if (filter.PartnerType.HasValue)
                {
                    query = query.Where(x => x.PartnerType == filter.PartnerType.Value);
                }

                if (filter.IsActive.HasValue)
                {
                    query = query.Where(x => x.IsActive == filter.IsActive.Value);
                }

                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    query = query.Where(x =>
                        x.PartnerName.Contains(filter.SearchTerm) ||
                        x.Property.WqfNumber.Contains(filter.SearchTerm) ||
                        (x.Property.PropertyName != null && x.Property.PropertyName.Contains(filter.SearchTerm)));
                }

                var total = await query.CountAsync();
                var pageItems = await query
                    .OrderByDescending(x => x.CreatedAt)
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                var list = new List<PartnershipListItemDto>();
                foreach (var p in pageItems)
                {
                    var endDate = p.PartnershipEndDate ?? p.UsufructEndDate;
                    var days = endDate.HasValue ? (int?)(endDate.Value.Date - DateTime.Today).TotalDays : null;
                    var distributed = await _unitOfWork.GetQueryable<PartnerRevenueDistribution>()
                        .Where(x => x.PartnershipId == p.Id)
                        .SumAsync(x => (decimal?)x.TotalRevenue) ?? 0m;
                    var pending = await _unitOfWork.GetQueryable<PartnerRevenueDistribution>()
                        .Where(x => x.PartnershipId == p.Id && x.TransferStatus == TransferStatus.Pending)
                        .SumAsync(x => (decimal?)x.PartnerAmount) ?? 0m;

                    list.Add(new PartnershipListItemDto
                    {
                        Id = p.Id,
                        PropertyId = p.PropertyId,
                        PropertyNameAr = p.Property.PropertyName ?? p.Property.WqfNumber,
                        PropertyWqfNumber = p.Property.WqfNumber,
                        PartnerName = p.PartnerName,
                        PartnerType = p.PartnerType,
                        PartnerPhone = p.PartnerPhone,
                        PartnerEmail = p.PartnerEmail,
                        PartnerNationalId = p.PartnerNationalId,
                        PartnerAddress = p.PartnerAddress,
                        PartnershipType = p.PartnershipType,
                        WaqfSharePercent = p.WaqfSharePercent,
                        PartnerSharePercent = p.PartnerSharePercent,
                        PartnershipEndDate = p.PartnershipEndDate,
                        DaysUntilExpiry = days,
                        IsActive = p.IsActive,
                        IsExpiringSoon = days.HasValue && days.Value < 90 && days.Value >= 0,
                        IsExpired = days.HasValue && days.Value < 0,
                        TotalDistributed = distributed,
                        PendingTransferAmount = pending,
                        RevenueDistribMethod = p.RevenueDistribMethod,
                        ExpenseBearingMethod = p.ExpenseBearingMethod,
                        NextDistribDate = p.NextDistribDate
                    });
                }

                return new PagedResult<PartnershipListItemDto>
                {
                    Items = list,
                    TotalCount = total,
                    Page = filter.Page,
                    PageSize = filter.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetPaged failed");
                throw;
            }
        }

        public async Task<List<PartnerContactDto>> GetContactHistoryAsync(int partnershipId, int page, int pageSize)
        {
            try
            {
                var partnership = await _unitOfWork.GetQueryable<PropertyPartnership>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == partnershipId && !x.IsDeleted);

                if (partnership == null)
                {
                    throw new ValidationException("الشراكة غير موجودة");
                }

                var logs = await _unitOfWork.GetQueryable<PartnerContactLog>()
                    .AsNoTracking()
                    .Where(x => x.PartnershipId == partnershipId)
                    .OrderByDescending(x => x.SentAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return logs.Select(x => new PartnerContactDto
                {
                    Id = x.Id,
                    PartnershipId = x.PartnershipId,
                    ContactType = x.ContactType,
                    ContactDirection = x.ContactDirection,
                    Subject = x.Subject,
                    MessageBody = x.MessageBody,
                    RecipientAddress = x.RecipientAddress,
                    SentAt = x.SentAt,
                    SentById = x.SentById,
                    PartnerName = partnership.PartnerName,
                    IsAutomatic = x.IsAutomatic,
                    DeliveryStatus = x.DeliveryStatus,
                    ExternalMessageId = x.ExternalMessageId,
                    LinkedDistributionId = x.LinkedDistributionId,
                    Notes = x.Notes
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get contact history failed");
                throw;
            }
        }

        public async Task<List<RevenueDistributionDto>> GetDistributionHistoryAsync(int partnershipId)
        {
            try
            {
                var partnership = await _unitOfWork.GetQueryable<PropertyPartnership>()
                    .Include(x => x.Property)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == partnershipId && !x.IsDeleted);

                if (partnership == null)
                {
                    throw new ValidationException("الشراكة غير موجودة");
                }

                var items = await _unitOfWork.GetQueryable<PartnerRevenueDistribution>()
                    .AsNoTracking()
                    .Where(x => x.PartnershipId == partnershipId)
                    .OrderByDescending(x => x.PeriodStartDate)
                    .ToListAsync();

                return items.Select(x => MapDistribution(x, partnership)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get distribution history failed");
                throw;
            }
        }

        public async Task<PartnershipExpenseEntryDto> AddExpenseAsync(CreatePartnershipExpenseDto dto, int userId)
        {
            var partnership = await _unitOfWork.GetQueryable<PropertyPartnership>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == dto.PartnershipId && !x.IsDeleted);

            if (partnership == null)
            {
                throw new ValidationException("الشراكة غير موجودة");
            }

            var expense = new PartnershipExpenseEntry
            {
                PartnershipId = partnership.Id,
                PropertyId = partnership.PropertyId,
                PeriodLabel = dto.PeriodLabel,
                PeriodStartDate = dto.PeriodStartDate.Date,
                PeriodEndDate = dto.PeriodEndDate.Date,
                ExpenseType = dto.ExpenseType,
                Amount = decimal.Round(dto.Amount, 2),
                ReferenceNo = dto.ReferenceNo,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow,
                CreatedById = userId
            };

            await _unitOfWork.AddAsync(expense);
            await _unitOfWork.SaveChangesAsync();

            return new PartnershipExpenseEntryDto
            {
                Id = expense.Id,
                PartnershipId = expense.PartnershipId,
                PropertyId = expense.PropertyId,
                PeriodLabel = expense.PeriodLabel,
                PeriodStartDate = expense.PeriodStartDate,
                PeriodEndDate = expense.PeriodEndDate,
                ExpenseType = expense.ExpenseType,
                Amount = expense.Amount,
                ReferenceNo = expense.ReferenceNo,
                Notes = expense.Notes,
                CreatedAt = expense.CreatedAt
            };
        }

        public async Task<List<PartnershipExpenseEntryDto>> GetExpensesAsync(int partnershipId, DateTime? from = null, DateTime? to = null)
        {
            var query = _unitOfWork.GetQueryable<PartnershipExpenseEntry>()
                .AsNoTracking()
                .Where(x => x.PartnershipId == partnershipId);

            if (from.HasValue)
            {
                query = query.Where(x => x.PeriodEndDate >= from.Value.Date);
            }

            if (to.HasValue)
            {
                query = query.Where(x => x.PeriodStartDate <= to.Value.Date);
            }

            return await query
                .OrderByDescending(x => x.PeriodStartDate)
                .Select(x => new PartnershipExpenseEntryDto
                {
                    Id = x.Id,
                    PartnershipId = x.PartnershipId,
                    PropertyId = x.PropertyId,
                    PeriodLabel = x.PeriodLabel,
                    PeriodStartDate = x.PeriodStartDate,
                    PeriodEndDate = x.PeriodEndDate,
                    ExpenseType = x.ExpenseType,
                    Amount = x.Amount,
                    ReferenceNo = x.ReferenceNo,
                    Notes = x.Notes,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<List<PartnershipListItemDto>> GetExpiringAsync(int daysAhead)
        {
            try
            {
                var to = DateTime.Today.AddDays(daysAhead);
                var items = await _unitOfWork.GetQueryable<PropertyPartnership>()
                    .Include(x => x.Property)
                    .AsNoTracking()
                    .Where(x => x.IsActive && !x.IsDeleted &&
                                ((x.PartnershipEndDate != null && x.PartnershipEndDate <= to) ||
                                 (x.UsufructEndDate != null && x.UsufructEndDate <= to)))
                    .ToListAsync();

                return items.Select(p =>
                {
                    var endDate = p.PartnershipEndDate ?? p.UsufructEndDate;
                    var days = endDate.HasValue ? (int?)(endDate.Value.Date - DateTime.Today).TotalDays : null;
                    return new PartnershipListItemDto
                    {
                        Id = p.Id,
                        PropertyId = p.PropertyId,
                        PropertyNameAr = p.Property.PropertyName ?? p.Property.WqfNumber,
                        PropertyWqfNumber = p.Property.WqfNumber,
                        PartnerName = p.PartnerName,
                        PartnerType = p.PartnerType,
                        PartnerPhone = p.PartnerPhone,
                        PartnerEmail = p.PartnerEmail,
                        PartnershipType = p.PartnershipType,
                        WaqfSharePercent = p.WaqfSharePercent,
                        PartnerSharePercent = p.PartnerSharePercent,
                        PartnershipEndDate = p.PartnershipEndDate,
                        DaysUntilExpiry = days,
                        IsActive = p.IsActive,
                        IsExpiringSoon = days.HasValue && days.Value < 90 && days.Value >= 0,
                        IsExpired = days.HasValue && days.Value < 0,
                        RevenueDistribMethod = p.RevenueDistribMethod,
                        ExpenseBearingMethod = p.ExpenseBearingMethod,
                        NextDistribDate = p.NextDistribDate
                    };
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get expiring failed");
                throw;
            }
        }

        private async Task ValidateCreateOrUpdateAsync(CreatePartnershipDto dto, Property property, int? excludeId)
        {
            if (dto.WaqfSharePercent <= 1 || dto.WaqfSharePercent >= 99)
            {
                throw new ValidationException("نسبة الوقف يجب أن تكون بين 1 و 99");
            }

            var waqf = NormalizePercent(dto.WaqfSharePercent);
            var partner = NormalizePercent(100m - waqf);
            if (decimal.Round(waqf + partner, 2) != 100m)
            {
                throw new ValidationException("مجموع نسبة الوقف والشريك يجب أن يساوي 100.00");
            }

            if (dto.RevenueDistribMethod == RevenueDistribMethod.Monthly && (!dto.RevenueDistribDay.HasValue || dto.RevenueDistribDay < 1 || dto.RevenueDistribDay > 28))
            {
                throw new ValidationException("يوم التوزيع الشهري يجب أن يكون بين 1 و 28");
            }

            if ((dto.PartnershipType == PartnershipType.LandPercent || dto.PartnershipType == PartnershipType.HarvestShare) && property.PropertyCategory != PropertyCategory.Agricultural)
            {
                throw new ValidationException("هذا النوع من الشراكات متاح فقط للعقارات الزراعية");
            }

            switch (dto.PartnershipType)
            {
                case PartnershipType.FloorOwnership:
                    if (dto.OwnedFloorNumbers == null || dto.OwnedFloorNumbers.Count == 0)
                    {
                        throw new ValidationException("يجب اختيار طوابق الوقف");
                    }

                    var floorNumbers = await _unitOfWork.GetQueryable<PropertyFloor>()
                        .AsNoTracking()
                        .Where(x => x.PropertyId == (int)dto.PropertyId && !x.IsDeleted)
                        .Select(x => (int)x.FloorNumber)
                        .ToListAsync();

                    if (dto.OwnedFloorNumbers.Except(floorNumbers).Any())
                    {
                        throw new ValidationException("بعض أرقام الطوابق غير موجودة ضمن العقار");
                    }
                    break;

                case PartnershipType.UnitOwnership:
                    if (dto.OwnedUnitIds == null || dto.OwnedUnitIds.Count == 0)
                    {
                        throw new ValidationException("يجب اختيار وحدات الوقف");
                    }

                    var unitIds = await _unitOfWork.GetQueryable<PropertyUnit>()
                        .AsNoTracking()
                        .Where(x => x.PropertyId == (int)dto.PropertyId && !x.IsDeleted)
                        .Select(x => x.Id)
                        .ToListAsync();

                    if (dto.OwnedUnitIds.Any(x => !unitIds.Contains((int)x)))
                    {
                        throw new ValidationException("بعض الوحدات المختارة لا تتبع هذا العقار");
                    }
                    break;

                case PartnershipType.UsufructRight:
                    if (!dto.UsufructStartDate.HasValue)
                    {
                        throw new ValidationException("تاريخ بدء حق الانتفاع مطلوب");
                    }
                    if (!dto.UsufructEndDate.HasValue || dto.UsufructEndDate <= dto.UsufructStartDate)
                    {
                        throw new ValidationException("تاريخ انتهاء حق الانتفاع يجب أن يكون بعد تاريخ البدء");
                    }
                    if (!dto.UsufructAnnualFeePerYear.HasValue || dto.UsufructAnnualFeePerYear <= 0)
                    {
                        throw new ValidationException("الرسوم السنوية لحق الانتفاع يجب أن تكون أكبر من صفر");
                    }
                    if (dto.UsufructEndDate.Value.Date <= DateTime.Today)
                    {
                        throw new ValidationException("لا يمكن إنشاء حق انتفاع منتهي الصلاحية");
                    }
                    break;

                case PartnershipType.TimedPartnership:
                    if (!dto.PartnershipStartDate.HasValue)
                    {
                        throw new ValidationException("تاريخ بدء الشراكة المؤقتة مطلوب");
                    }
                    if (!dto.PartnershipEndDate.HasValue || dto.PartnershipEndDate <= dto.PartnershipStartDate)
                    {
                        throw new ValidationException("تاريخ انتهاء الشراكة المؤقتة يجب أن يكون بعد تاريخ البدء");
                    }
                    break;

                case PartnershipType.LandPercent:
                    if (!dto.LandSharePercentWaqf.HasValue || dto.LandSharePercentWaqf <= 1 || dto.LandSharePercentWaqf >= 99)
                    {
                        throw new ValidationException("نسبة الوقف من الأرض يجب أن تكون بين 1 و99");
                    }
                    if (!dto.LandTotalDunum.HasValue || dto.LandTotalDunum <= 0)
                    {
                        throw new ValidationException("إجمالي مساحة الأرض يجب أن يكون أكبر من صفر");
                    }
                    break;

                case PartnershipType.HarvestShare:
                    if (!dto.WaqfHarvestPercent.HasValue || dto.WaqfHarvestPercent <= 1 || dto.WaqfHarvestPercent >= 99)
                    {
                        throw new ValidationException("نسبة الوقف من المحصول يجب أن تكون بين 1 و99");
                    }
                    if (string.IsNullOrWhiteSpace(dto.FarmerName))
                    {
                        throw new ValidationException("اسم المزارع مطلوب");
                    }
                    break;

                case PartnershipType.Custom:
                    if (string.IsNullOrWhiteSpace(dto.CustomPartnershipName))
                    {
                        throw new ValidationException("اسم نوع الشراكة المخصصة مطلوب");
                    }
                    break;
            }
        }

        private async Task<RevenueCalculationResultDto> CalculateRevenueAsync(PropertyPartnership partnership, decimal totalRevenue, decimal totalExpenses, string distributionType, DateTime currentDate, string? seasonLabel)
        {
            decimal waqfAmount;
            decimal waqfPercent = partnership.WaqfSharePercent;
            string method;
            var distributableRevenue = GetDistributableRevenue(partnership.ExpenseBearingMethod, totalRevenue, totalExpenses);

            switch (partnership.PartnershipType)
            {
                case PartnershipType.RevenuePercent:
                    waqfAmount = distributableRevenue * (partnership.WaqfSharePercent / 100m);
                    method = "نسبة من الإيراد الكلي";
                    break;

                case PartnershipType.FloorOwnership:
                    var allFloors = await _unitOfWork.GetQueryable<PropertyFloor>()
                        .AsNoTracking()
                        .Where(x => x.PropertyId == partnership.PropertyId && !x.IsDeleted)
                        .ToListAsync();
                    var waqfFloors = ParseIntArray(partnership.OwnedFloorNumbers);
                    var waqfFloorCount = allFloors.Count(x => waqfFloors.Contains(x.FloorNumber));
                    var totalFloorCount = allFloors.Count;
                    waqfAmount = totalFloorCount == 0 ? 0 : distributableRevenue * (waqfFloorCount / (decimal)totalFloorCount);
                    waqfPercent = totalFloorCount == 0 ? 0 : decimal.Round((waqfFloorCount / (decimal)totalFloorCount) * 100m, 2);
                    method = "ملكية طوابق حسب عدد الطوابق";
                    break;

                case PartnershipType.UnitOwnership:
                    var units = await _unitOfWork.GetQueryable<PropertyUnit>()
                        .AsNoTracking()
                        .Where(x => x.PropertyId == partnership.PropertyId && !x.IsDeleted)
                        .ToListAsync();
                    var waqfUnits = ParseLongArray(partnership.OwnedUnitIds);
                    var selectedUnits = units.Where(x => waqfUnits.Contains(x.Id)).ToList();
                    var waqfUnitRent = selectedUnits.Sum(x => x.MarketRentMonthly ?? 0m);
                    var totalRent = units.Sum(x => x.MarketRentMonthly ?? 0m);
                    if (totalRent > 0)
                    {
                        waqfAmount = distributableRevenue * (waqfUnitRent / totalRent);
                        waqfPercent = decimal.Round((waqfUnitRent / totalRent) * 100m, 2);
                    }
                    else
                    {
                        var totalUnits = units.Count;
                        waqfAmount = totalUnits == 0 ? 0 : distributableRevenue * (selectedUnits.Count / (decimal)totalUnits);
                        waqfPercent = totalUnits == 0 ? 0 : decimal.Round((selectedUnits.Count / (decimal)totalUnits) * 100m, 2);
                    }
                    method = "ملكية عينية حسب الوحدات";
                    break;

                case PartnershipType.UsufructRight:
                    if (partnership.UsufructEndDate.HasValue && currentDate.Date > partnership.UsufructEndDate.Value.Date)
                    {
                        waqfAmount = totalRevenue;
                        waqfPercent = 100m;
                        method = "انتهى حق الانتفاع - 100% للوقف";
                    }
                    else
                    {
                        var monthlyFee = (partnership.UsufructAnnualFeePerYear ?? 0m) / 12m;
                        waqfAmount = monthlyFee > distributableRevenue ? distributableRevenue : monthlyFee;
                        waqfPercent = distributableRevenue == 0 ? 0 : decimal.Round((waqfAmount / distributableRevenue) * 100m, 2);
                        method = "حق انتفاع برسوم شهرية ثابتة";
                    }
                    break;

                case PartnershipType.LandPercent:
                    waqfPercent = partnership.LandSharePercentWaqf ?? 0m;
                    waqfAmount = distributableRevenue * (waqfPercent / 100m);
                    method = "نسبة الوقف من الأرض الزراعية";
                    break;

                case PartnershipType.TimedPartnership:
                    if (partnership.PartnershipEndDate.HasValue && currentDate.Date > partnership.PartnershipEndDate.Value.Date)
                    {
                        waqfAmount = totalRevenue;
                        waqfPercent = 100m;
                        method = "انتهت الشراكة المؤقتة - كامل الإيراد للوقف";
                    }
                    else
                    {
                        waqfAmount = distributableRevenue * (partnership.WaqfSharePercent / 100m);
                        method = "شراكة مؤقتة بالنسبة المتفق عليها";
                    }
                    break;

                case PartnershipType.HarvestShare:
                    waqfPercent = partnership.WaqfHarvestPercent ?? 0m;
                    waqfAmount = distributableRevenue * (waqfPercent / 100m);
                    method = distributionType == "Harvest" ? "حصة الوقف من المحصول" : "مزارعة/مساقاة";
                    break;

                case PartnershipType.Custom:
                    waqfAmount = distributableRevenue * (partnership.WaqfSharePercent / 100m);
                    method = string.IsNullOrWhiteSpace(partnership.CustomPartnershipName)
                        ? "شراكة مخصصة"
                        : $"شراكة مخصصة ({partnership.CustomPartnershipName})";
                    break;

                default:
                    waqfAmount = distributableRevenue * (partnership.WaqfSharePercent / 100m);
                    method = "افتراضي";
                    break;
            }

            var activeRules = await _unitOfWork.GetQueryable<PartnershipConditionRule>()
                .AsNoTracking()
                .Where(x => x.PartnershipId == partnership.Id && x.IsActive)
                .OrderBy(x => x.PriorityOrder)
                .ToListAsync();

            PartnershipConditionRule? appliedRule = null;
            foreach (var rule in activeRules)
            {
                if (!IsRuleApplicable(rule, totalRevenue, distributionType, seasonLabel, currentDate))
                {
                    continue;
                }

                appliedRule = rule;
                switch (rule.RuleType)
                {
                    case ConditionRuleType.FixedAmount:
                        waqfAmount = rule.FixedAmount ?? waqfAmount;
                        break;
                    case ConditionRuleType.PercentOfRevenue:
                    case ConditionRuleType.SeasonalOverride:
                    case ConditionRuleType.HarvestOverride:
                        if (rule.PercentValue.HasValue)
                        {
                            waqfAmount = distributableRevenue * (rule.PercentValue.Value / 100m);
                            waqfPercent = rule.PercentValue.Value;
                        }
                        break;
                    case ConditionRuleType.MinGuaranteedAmount:
                        waqfAmount = Math.Max(waqfAmount, rule.FixedAmount ?? 0m);
                        break;
                    case ConditionRuleType.OneTimeAdjustment:
                        waqfAmount += rule.FixedAmount ?? 0m;
                        break;
                }

                break;
            }

            var (waqfAfterExpense, partnerAfterExpense, totalForSplit) = ApplyExpensePolicy(
                partnership.ExpenseBearingMethod,
                distributableRevenue,
                totalRevenue,
                totalExpenses,
                waqfAmount);

            waqfAmount = decimal.Round(waqfAfterExpense, 2);
            var partnerAmount = decimal.Round(partnerAfterExpense, 2);
            var finalWaqfPercent = totalForSplit == 0 ? 0m : decimal.Round((waqfAmount / totalForSplit) * 100m, 2);

            return new RevenueCalculationResultDto
            {
                TotalRevenue = totalRevenue,
                TotalExpenses = totalExpenses,
                NetRevenue = decimal.Round(Math.Max(0m, totalRevenue - totalExpenses), 2),
                WaqfAmount = waqfAmount,
                PartnerAmount = partnerAmount,
                WaqfPercent = finalWaqfPercent,
                CalculationMethod = method,
                AppliedRuleName = appliedRule?.RuleName,
                CalculationDetail = $"إجمالي الإيراد {totalRevenue.ToString("N0", CultureInfo.InvariantCulture)} د.ع، المصروفات {totalExpenses.ToString("N0", CultureInfo.InvariantCulture)} د.ع، صافي التوزيع {(Math.Max(0m, totalRevenue - totalExpenses)).ToString("N0", CultureInfo.InvariantCulture)} د.ع. حصة الوقف {waqfAmount.ToString("N0", CultureInfo.InvariantCulture)} د.ع، حصة الشريك {partnerAmount.ToString("N0", CultureInfo.InvariantCulture)} د.ع"
            };
        }

        private async Task<PartnershipDetailDto> BuildDetailDtoAsync(PropertyPartnership p)
        {
            var distributions = await _unitOfWork.GetQueryable<PartnerRevenueDistribution>()
                .AsNoTracking()
                .Where(x => x.PartnershipId == p.Id)
                .ToListAsync();

            var lastContact = await _unitOfWork.GetQueryable<PartnerContactLog>()
                .AsNoTracking()
                .Where(x => x.PartnershipId == p.Id)
                .OrderByDescending(x => x.SentAt)
                .FirstOrDefaultAsync();

            var endDate = p.PartnershipEndDate ?? p.UsufructEndDate;
            var days = endDate.HasValue ? (int?)(endDate.Value.Date - DateTime.Today).TotalDays : null;

            return new PartnershipDetailDto
            {
                Id = p.Id,
                PropertyId = p.PropertyId,
                PropertyNameAr = p.Property.PropertyName ?? p.Property.WqfNumber,
                PropertyWqfNumber = p.Property.WqfNumber,
                PartnershipType = p.PartnershipType,
                WaqfSharePercent = p.WaqfSharePercent,
                PartnerSharePercent = p.PartnerSharePercent,
                PartnerName = p.PartnerName,
                PartnerNameEn = p.PartnerNameEn,
                PartnerType = p.PartnerType,
                PartnerNationalId = p.PartnerNationalId,
                PartnerRegistrationNo = p.PartnerRegistrationNo,
                PartnerPhone = p.PartnerPhone,
                PartnerPhone2 = p.PartnerPhone2,
                PartnerEmail = p.PartnerEmail,
                PartnerWhatsApp = p.PartnerWhatsApp,
                PartnerAddress = p.PartnerAddress,
                PartnerBankName = p.PartnerBankName,
                PartnerBankIBAN = p.PartnerBankIBAN,
                PartnerBankAccountNo = p.PartnerBankAccountNo,
                PartnerBankBranch = p.PartnerBankBranch,
                AgreementDate = p.AgreementDate,
                AgreementNotaryName = p.AgreementNotaryName,
                AgreementCourt = p.AgreementCourt,
                AgreementReferenceNo = p.AgreementReferenceNo,
                AgreementDocUrl = p.AgreementDocUrl,
                UsufructStartDate = p.UsufructStartDate,
                UsufructEndDate = p.UsufructEndDate,
                UsufructAnnualFeePerYear = p.UsufructAnnualFeePerYear,
                PartnershipStartDate = p.PartnershipStartDate,
                PartnershipEndDate = p.PartnershipEndDate,
                LandSharePercentWaqf = p.LandSharePercentWaqf,
                LandTotalDunum = p.LandTotalDunum,
                WaqfLandDunum = p.WaqfLandDunum,
                WaqfHarvestPercent = p.WaqfHarvestPercent,
                FarmerName = p.FarmerName,
                FarmerNationalId = p.FarmerNationalId,
                HarvestContractType = p.HarvestContractType,
                CustomPartnershipName = p.CustomPartnershipName,
                CustomCalculationFormula = p.CustomCalculationFormula,
                RevenueDistribMethod = p.RevenueDistribMethod,
                ExpenseBearingMethod = p.ExpenseBearingMethod,
                RevenueDistribDay = p.RevenueDistribDay,
                LastDistribDate = p.LastDistribDate,
                NextDistribDate = p.NextDistribDate,
                OwnedFloorNumbersList = ParseIntArray(p.OwnedFloorNumbers).Select(x => (int)x).ToList(),
                OwnedUnitsList = ParseLongArray(p.OwnedUnitIds).Select(x => (long)x).ToList(),
                DaysUntilExpiry = days,
                IsExpiringSoon = days.HasValue && days.Value < 90 && days.Value >= 0,
                IsExpired = days.HasValue && days.Value < 0,
                TotalDistributed = distributions.Sum(x => x.TotalRevenue),
                TotalWaqfReceived = distributions.Sum(x => x.WaqfAmount),
                TotalPartnerReceived = distributions.Sum(x => x.PartnerAmount),
                PendingTransferAmount = distributions.Where(x => x.TransferStatus == TransferStatus.Pending).Sum(x => x.PartnerAmount),
                LastContactDate = lastContact?.SentAt,
                LastContactType = lastContact?.ContactType.ToString(),
                ConditionRulesCount = await _unitOfWork.GetQueryable<PartnershipConditionRule>().CountAsync(x => x.PartnershipId == p.Id && x.IsActive),
                IsActive = p.IsActive,
                Notes = p.Notes
            };
        }

        private async Task WriteAuditAsync(string tableName, int recordId, string action, string? oldValues, string? newValues, int userId)
        {
            await _unitOfWork.AddAsync(new AuditLog
            {
                TableName = tableName,
                RecordId = recordId,
                Action = action,
                OldValues = oldValues,
                NewValues = newValues,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            });
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task CreateExpirySchedulesAsync(PropertyPartnership partnership)
        {
            var endDate = partnership.PartnershipEndDate ?? partnership.UsufructEndDate;
            if (!endDate.HasValue)
            {
                return;
            }

            var trigger90 = endDate.Value.Date.AddDays(-90);
            var trigger30 = endDate.Value.Date.AddDays(-30);

            var has90 = await _unitOfWork.GetQueryable<PartnerNotificationSchedule>()
                .AnyAsync(x => x.PartnershipId == partnership.Id && x.TriggerType == PartnershipNotificationTrigger.ExpiryWarning90 && x.TriggerDate == trigger90);
            if (!has90)
            {
                await _unitOfWork.AddAsync(new PartnerNotificationSchedule
                {
                    PartnershipId = partnership.Id,
                    TriggerType = PartnershipNotificationTrigger.ExpiryWarning90,
                    TriggerDate = trigger90,
                    Channels = "[\"Internal\"]",
                    TemplateKey = "ExpiryWarning90",
                    CreatedAt = DateTime.UtcNow
                });
            }

            var has30 = await _unitOfWork.GetQueryable<PartnerNotificationSchedule>()
                .AnyAsync(x => x.PartnershipId == partnership.Id && x.TriggerType == PartnershipNotificationTrigger.ExpiryWarning30 && x.TriggerDate == trigger30);
            if (!has30)
            {
                await _unitOfWork.AddAsync(new PartnerNotificationSchedule
                {
                    PartnershipId = partnership.Id,
                    TriggerType = PartnershipNotificationTrigger.ExpiryWarning30,
                    TriggerDate = trigger30,
                    Channels = "[\"SMS\",\"WhatsApp\",\"Internal\"]",
                    TemplateKey = "ExpiryWarning30",
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _unitOfWork.SaveChangesAsync();
        }

        private static DateTime? ComputeNextDistribDate(RevenueDistribMethod method, int? day, DateTime fromDate)
        {
            return method switch
            {
                RevenueDistribMethod.Monthly => new DateTime(fromDate.Year, fromDate.Month, Math.Min(day ?? 1, 28)).AddMonths(1),
                RevenueDistribMethod.Quarterly => fromDate.AddMonths(3),
                RevenueDistribMethod.Annual => fromDate.AddYears(1),
                RevenueDistribMethod.PerCollection => null,
                _ => null
            };
        }

        private static decimal NormalizePercent(decimal value)
        {
            return decimal.Round(value, 2);
        }

        private static int? CalculateTermYears(DateTime? start, DateTime? end)
        {
            if (!start.HasValue || !end.HasValue)
            {
                return null;
            }

            var years = (end.Value.Date - start.Value.Date).TotalDays / 365d;
            return (int)Math.Round(years, MidpointRounding.AwayFromZero);
        }

        private static decimal? CalculateWaqfLand(decimal? share, decimal? total)
        {
            if (!share.HasValue || !total.HasValue)
            {
                return null;
            }

            return decimal.Round(total.Value * (share.Value / 100m), 2);
        }

        private static string? SerializeOrNull<T>(List<T>? values)
        {
            if (values == null || values.Count == 0)
            {
                return null;
            }
            return JsonSerializer.Serialize(values);
        }

        private static HashSet<short> ParseIntArray(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new HashSet<short>();
            }

            try
            {
                var values = JsonSerializer.Deserialize<List<int>>(json) ?? new List<int>();
                return values.Select(x => (short)x).ToHashSet();
            }
            catch
            {
                return new HashSet<short>();
            }
        }

        private static HashSet<int> ParseLongArray(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new HashSet<int>();
            }

            try
            {
                var values = JsonSerializer.Deserialize<List<long>>(json) ?? new List<long>();
                return values.Select(x => (int)x).ToHashSet();
            }
            catch
            {
                return new HashSet<int>();
            }
        }

        private async Task SyncConditionRulesAsync(int partnershipId, List<PartnershipConditionRuleDto> rules, int userId)
        {
            var existing = await _unitOfWork.GetQueryable<PartnershipConditionRule>()
                .Where(x => x.PartnershipId == partnershipId)
                .ToListAsync();

            foreach (var old in existing)
            {
                old.IsActive = false;
                await _unitOfWork.UpdateAsync(old);
            }

            foreach (var rule in rules.Where(x => !string.IsNullOrWhiteSpace(x.RuleName)))
            {
                var entity = new PartnershipConditionRule
                {
                    PartnershipId = partnershipId,
                    RuleType = rule.RuleType,
                    Scope = rule.Scope,
                    RuleName = rule.RuleName,
                    FixedAmount = rule.FixedAmount,
                    PercentValue = rule.PercentValue,
                    MinRevenueThreshold = rule.MinRevenueThreshold,
                    MaxRevenueThreshold = rule.MaxRevenueThreshold,
                    StartDate = rule.StartDate,
                    EndDate = rule.EndDate,
                    DistributionType = rule.DistributionType,
                    SeasonLabel = rule.SeasonLabel,
                    PriorityOrder = rule.PriorityOrder,
                    IsActive = rule.IsActive,
                    Notes = rule.Notes,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = userId
                };
                await _unitOfWork.AddAsync(entity);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        private static decimal GetDistributableRevenue(ExpenseBearingMethod expenseMethod, decimal totalRevenue, decimal totalExpenses)
        {
            var expenses = Math.Max(0m, totalExpenses);
            var revenue = Math.Max(0m, totalRevenue);

            if (expenseMethod == ExpenseBearingMethod.BeforeDistribution || expenseMethod == ExpenseBearingMethod.SharedByPercent)
            {
                return Math.Max(0m, revenue - expenses);
            }

            return revenue;
        }

        private static (decimal waqfAmount, decimal partnerAmount, decimal totalForSplit) ApplyExpensePolicy(
            ExpenseBearingMethod expenseMethod,
            decimal distributableRevenue,
            decimal totalRevenue,
            decimal totalExpenses,
            decimal waqfAmount)
        {
            var revenue = Math.Max(0m, totalRevenue);
            var expenses = Math.Max(0m, totalExpenses);
            var waqfBase = Math.Min(Math.Max(0m, waqfAmount), distributableRevenue);
            var partnerBase = Math.Max(0m, distributableRevenue - waqfBase);

            if (expenseMethod == ExpenseBearingMethod.WaqfOnly)
            {
                waqfBase = Math.Max(0m, waqfBase - expenses);
                return (waqfBase, Math.Max(0m, revenue - waqfBase - expenses), Math.Max(0m, revenue - expenses));
            }

            if (expenseMethod == ExpenseBearingMethod.PartnerOnly)
            {
                partnerBase = Math.Max(0m, partnerBase - expenses);
                return (waqfBase, partnerBase, Math.Max(0m, revenue - expenses));
            }

            return (waqfBase, partnerBase, distributableRevenue);
        }

        private static bool IsRuleApplicable(PartnershipConditionRule rule, decimal totalRevenue, string distributionType, string? seasonLabel, DateTime currentDate)
        {
            if (rule.MinRevenueThreshold.HasValue && totalRevenue < rule.MinRevenueThreshold.Value)
            {
                return false;
            }

            if (rule.MaxRevenueThreshold.HasValue && totalRevenue > rule.MaxRevenueThreshold.Value)
            {
                return false;
            }

            if (rule.StartDate.HasValue && currentDate.Date < rule.StartDate.Value.Date)
            {
                return false;
            }

            if (rule.EndDate.HasValue && currentDate.Date > rule.EndDate.Value.Date)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(rule.DistributionType) && !string.Equals(rule.DistributionType, distributionType, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(rule.SeasonLabel) && !string.Equals(rule.SeasonLabel, seasonLabel, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }

        private static string ResolveRecipient(ContactType type, PropertyPartnership partnership)
        {
            return type switch
            {
                ContactType.SMS => partnership.PartnerPhone,
                ContactType.WhatsApp => partnership.PartnerWhatsApp ?? partnership.PartnerPhone,
                ContactType.Email => partnership.PartnerEmail ?? string.Empty,
                _ => partnership.PartnerName
            };
        }

        private static CreatePartnershipDto MergeForValidation(PropertyPartnership entity, UpdatePartnershipDto dto)
        {
            return new CreatePartnershipDto
            {
                PropertyId = dto.PropertyId ?? entity.PropertyId,
                PartnershipType = dto.PartnershipType ?? entity.PartnershipType,
                WaqfSharePercent = dto.WaqfSharePercent ?? entity.WaqfSharePercent,
                PartnerName = dto.PartnerName ?? entity.PartnerName,
                PartnerNameEn = dto.PartnerNameEn ?? entity.PartnerNameEn,
                PartnerType = dto.PartnerType ?? entity.PartnerType,
                PartnerNationalId = dto.PartnerNationalId ?? entity.PartnerNationalId,
                PartnerRegistrationNo = dto.PartnerRegistrationNo ?? entity.PartnerRegistrationNo,
                PartnerPhone = dto.PartnerPhone ?? entity.PartnerPhone,
                PartnerPhone2 = dto.PartnerPhone2 ?? entity.PartnerPhone2,
                PartnerEmail = dto.PartnerEmail ?? entity.PartnerEmail,
                PartnerWhatsApp = dto.PartnerWhatsApp ?? entity.PartnerWhatsApp,
                PartnerAddress = dto.PartnerAddress ?? entity.PartnerAddress,
                PartnerBankName = dto.PartnerBankName ?? entity.PartnerBankName,
                PartnerBankIBAN = dto.PartnerBankIBAN ?? entity.PartnerBankIBAN,
                PartnerBankAccountNo = dto.PartnerBankAccountNo ?? entity.PartnerBankAccountNo,
                PartnerBankBranch = dto.PartnerBankBranch ?? entity.PartnerBankBranch,
                OwnedFloorNumbers = dto.OwnedFloorNumbers ?? JsonSerializer.Deserialize<List<int>>(entity.OwnedFloorNumbers ?? "[]"),
                OwnedUnitIds = dto.OwnedUnitIds ?? JsonSerializer.Deserialize<List<long>>(entity.OwnedUnitIds ?? "[]"),
                UsufructStartDate = dto.UsufructStartDate ?? entity.UsufructStartDate,
                UsufructEndDate = dto.UsufructEndDate ?? entity.UsufructEndDate,
                UsufructAnnualFeePerYear = dto.UsufructAnnualFeePerYear ?? entity.UsufructAnnualFeePerYear,
                PartnershipStartDate = dto.PartnershipStartDate ?? entity.PartnershipStartDate,
                PartnershipEndDate = dto.PartnershipEndDate ?? entity.PartnershipEndDate,
                LandSharePercentWaqf = dto.LandSharePercentWaqf ?? entity.LandSharePercentWaqf,
                LandTotalDunum = dto.LandTotalDunum ?? entity.LandTotalDunum,
                WaqfHarvestPercent = dto.WaqfHarvestPercent ?? entity.WaqfHarvestPercent,
                FarmerName = dto.FarmerName ?? entity.FarmerName,
                FarmerNationalId = dto.FarmerNationalId ?? entity.FarmerNationalId,
                HarvestContractType = dto.HarvestContractType ?? entity.HarvestContractType,
                CustomPartnershipName = dto.CustomPartnershipName ?? entity.CustomPartnershipName,
                CustomCalculationFormula = dto.CustomCalculationFormula ?? entity.CustomCalculationFormula,
                AgreementDate = dto.AgreementDate ?? entity.AgreementDate,
                AgreementNotaryName = dto.AgreementNotaryName ?? entity.AgreementNotaryName,
                AgreementCourt = dto.AgreementCourt ?? entity.AgreementCourt,
                AgreementReferenceNo = dto.AgreementReferenceNo ?? entity.AgreementReferenceNo,
                RevenueDistribMethod = dto.RevenueDistribMethod ?? entity.RevenueDistribMethod,
                ExpenseBearingMethod = dto.ExpenseBearingMethod ?? entity.ExpenseBearingMethod,
                RevenueDistribDay = dto.RevenueDistribDay ?? entity.RevenueDistribDay,
                ConditionRules = dto.ConditionRules ?? new List<PartnershipConditionRuleDto>(),
                Notes = dto.Notes ?? entity.Notes
            };
        }

        private static List<string> ParseChannels(string json)
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

        private static RevenueDistributionDto MapDistribution(PartnerRevenueDistribution distribution, PropertyPartnership partnership)
        {
            return new RevenueDistributionDto
            {
                Id = distribution.Id,
                PartnershipId = distribution.PartnershipId,
                PropertyId = distribution.PropertyId,
                PartnerName = partnership.PartnerName,
                PropertyNameAr = partnership.Property?.PropertyName ?? string.Empty,
                PeriodLabel = distribution.PeriodLabel,
                PeriodStartDate = distribution.PeriodStartDate,
                PeriodEndDate = distribution.PeriodEndDate,
                DistributionType = distribution.DistributionType,
                TotalRevenue = distribution.TotalRevenue,
                TotalExpenses = distribution.TotalExpenses,
                NetRevenue = distribution.NetRevenue,
                WaqfAmount = distribution.WaqfAmount,
                PartnerAmount = distribution.PartnerAmount,
                WaqfPercentSnapshot = distribution.WaqfPercentSnapshot,
                TransferStatus = distribution.TransferStatus,
                TransferDate = distribution.TransferDate,
                TransferMethod = distribution.TransferMethod,
                TransferReference = distribution.TransferReference,
                TransferBankName = distribution.TransferBankName,
                Notes = distribution.Notes,
                CreatedAt = distribution.CreatedAt
            };
        }

        public async Task<int> CreatePartnerAsync(CreatePartnerDto dto, int userId)
        {
            var partner = new Partner
            {
                Name = dto.PartnerName,
                NameEn = dto.PartnerNameEn,
                Type = dto.PartnerType,
                NationalId = dto.PartnerNationalId,
                RegistrationNo = dto.PartnerRegistrationNo,
                Phone = dto.PartnerPhone,
                Email = dto.PartnerEmail,
                WhatsApp = dto.PartnerWhatsApp,
                Address = dto.PartnerAddress,
                CreatedById = userId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _unitOfWork.AddAsync(partner);
            await _unitOfWork.SaveChangesAsync();
            return partner.Id;
        }

        public async Task<List<PartnerSummaryDto>> SearchPartnersAsync(string term)
        {
            var registeredPartners = await _unitOfWork.GetQueryable<Partner>()
                .Where(x => !x.IsDeleted && (string.IsNullOrEmpty(term) || x.Name.Contains(term) || x.Phone.Contains(term)))
                .OrderBy(x => x.Name)
                .ToListAsync();

            var historicalPartnersRaw = await _unitOfWork.GetQueryable<PropertyPartnership>()
                .Where(x => !x.IsDeleted && (string.IsNullOrEmpty(term) || x.PartnerName.Contains(term) || x.PartnerPhone.Contains(term)))
                .GroupBy(x => x.PartnerName)
                .Select(g => new { 
                    Name = g.Key, 
                    Type = g.First().PartnerType,
                    Phone = g.First().PartnerPhone,
                    Email = g.First().PartnerEmail,
                    NationalId = g.First().PartnerNationalId,
                    Address = g.First().PartnerAddress
                })
                .ToListAsync();

            var result = registeredPartners.Select(p => new PartnerSummaryDto
            {
                Name = p.Name,
                Type = p.Type,
                Phone = p.Phone,
                Email = p.Email,
                NationalId = p.NationalId,
                Address = p.Address
            }).ToList();

            foreach (var h in historicalPartnersRaw)
            {
                if (!result.Any(r => r.Name == h.Name))
                {
                    result.Add(new PartnerSummaryDto
                    {
                        Name = h.Name,
                        Type = h.Type,
                        Phone = h.Phone,
                        Email = h.Email,
                        NationalId = h.NationalId,
                        Address = h.Address
                    });
                }
            }

            var allPartnerships = await _unitOfWork.GetQueryable<PropertyPartnership>()
                .Include(x => x.RevenueDistributions)
                .Where(x => !x.IsDeleted)
                .ToListAsync();

            foreach (var r in result)
            {
                var related = allPartnerships.Where(x => x.PartnerName == r.Name).ToList();
                r.PartnershipCount = related.Count;
                r.TotalWaqfRevenue = related.SelectMany(x => x.RevenueDistributions).Sum(d => d.WaqfAmount);
                r.TotalPartnerShare = related.SelectMany(x => x.RevenueDistributions).Sum(d => d.PartnerAmount);
                r.LastPartnershipDate = related.OrderByDescending(x => x.CreatedAt).FirstOrDefault()?.CreatedAt;
            }

            return result;
        }
    }
}
