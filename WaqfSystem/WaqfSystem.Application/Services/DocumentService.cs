using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WaqfSystem.Application.DTOs.Common;
using WaqfSystem.Application.DTOs.Document;
using WaqfSystem.Core.Entities;
using WaqfSystem.Core.Enums;
using WaqfSystem.Core.Interfaces;

namespace WaqfSystem.Application.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IAppDbContext _db;
        private readonly IFileStorageService _fileStorage;
        private readonly INotificationService _notifications;
        private readonly IEmailService _emailService;
        private readonly IOcrService _ocr;
        private readonly IDocumentAlertService _documentAlertService;
        private readonly ILogger<DocumentService> _logger;

        public DocumentService(
            IAppDbContext db,
            IFileStorageService fileStorage,
            INotificationService notifications,
            IEmailService emailService,
            IOcrService ocr,
            IDocumentAlertService documentAlertService,
            ILogger<DocumentService> logger)
        {
            _db = db;
            _fileStorage = fileStorage;
            _notifications = notifications;
            _emailService = emailService;
            _ocr = ocr;
            _documentAlertService = documentAlertService;
            _logger = logger;
        }

        public async Task<long> UploadAsync(UploadDocumentDto dto, int userId)
        {
            var propertyId = (int)dto.PropertyId;
            var property = await _db.Properties.AsNoTracking().FirstOrDefaultAsync(x => x.Id == propertyId);
            if (property == null)
            {
                throw new InvalidOperationException("العقار غير موجود");
            }

            var documentType = await _db.DocumentTypes.FirstOrDefaultAsync(x => x.Id == dto.DocumentTypeId && x.IsActive);
            if (documentType == null)
            {
                throw new InvalidOperationException("نوع الوثيقة غير صالح");
            }

            ValidateFile(dto.File, documentType);

            var folder = $"documents/{propertyId}/{documentType.Code}";
            var fileUrl = await _fileStorage.UploadFileAsync(dto.File, folder);
            var fileName = Path.GetFileName(dto.File.FileName);
            var extension = Path.GetExtension(dto.File.FileName).Trim('.').ToLowerInvariant();
            var mimeType = GetMimeType(dto.File, extension);

            var document = new PropertyDocument
            {
                PropertyId = propertyId,
                DocumentTypeId = dto.DocumentTypeId,
                DocumentNumber = dto.DocumentNumber,
                Title = dto.Title.Trim(),
                Description = dto.Description,
                IssuingAuthority = dto.IssuingAuthority,
                IssueDate = dto.IssueDate?.Date,
                ExpiryDate = dto.ExpiryDate?.Date,
                Status = DocumentStatus.PendingVerification,
                VersionCount = 1,
                LinkedUnitId = dto.LinkedUnitId.HasValue ? (int)dto.LinkedUnitId.Value : null,
                LinkedPartnershipId = dto.LinkedPartnershipId,
                PrimaryResponsibleId = dto.PrimaryResponsibleId,
                CreatedAt = DateTime.UtcNow,
                CreatedById = userId,
                Tags = SerializeTags(dto.Tags)
            };

            await _db.PropertyDocuments.AddAsync(document);
            await _db.SaveChangesAsync();

            var version = new DocumentVersion
            {
                DocumentId = document.Id,
                VersionNumber = 1,
                FileUrl = fileUrl,
                FileName = fileName,
                FileExtension = extension,
                FileSizeBytes = dto.File.Length,
                MimeType = mimeType,
                UploadedById = userId,
                UploadedAt = DateTime.UtcNow,
                IsCurrent = true,
                Notes = dto.Notes
            };

            await _db.DocumentVersions.AddAsync(version);
            await _db.SaveChangesAsync();

            document.CurrentVersionId = version.Id;
            document.UpdatedAt = DateTime.UtcNow;

            if (dto.PrimaryResponsibleId.HasValue)
            {
                var existingResponsible = await _db.DocumentResponsibles
                    .FirstOrDefaultAsync(x => x.DocumentId == document.Id && x.UserId == dto.PrimaryResponsibleId.Value);

                if (existingResponsible == null)
                {
                    await _db.DocumentResponsibles.AddAsync(new DocumentResponsible
                    {
                        DocumentId = document.Id,
                        UserId = dto.PrimaryResponsibleId.Value,
                        AssignedById = userId,
                        AssignedAt = DateTime.UtcNow,
                        IsActive = true,
                        Notes = "تعيين تلقائي عند الرفع"
                    });
                }
                else if (!existingResponsible.IsActive)
                {
                    existingResponsible.IsActive = true;
                    existingResponsible.AssignedAt = DateTime.UtcNow;
                    existingResponsible.AssignedById = userId;
                }
            }

            await _db.SaveChangesAsync();

            await LogAuditAsync(document.Id, DocumentActionType.Uploaded, userId,
                $"تم رفع الملف {fileName} بحجم {FormatFileSize(dto.File.Length)}", null, null, version.Id);

            if (documentType.HasExpiry && !document.ExpiryDate.HasValue)
            {
                _logger.LogWarning("تم رفع وثيقة تتطلب تاريخ انتهاء بدون تاريخ انتهاء. DocumentId={DocumentId}", document.Id);
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    await ProcessOcrAsync(document.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "فشل OCR للوثيقة {DocumentId}", document.Id);
                }
            });

            foreach (var roleCode in ParseRoleCodes(documentType.VerifierRoles))
            {
                await _notifications.SendToRoleAsync(roleCode, "وثيقة جديدة", $"وثيقة جديدة تنتظر التحقق: {document.Title}", "PropertyDocuments", (int)document.Id);
            }

            return document.Id;
        }

        public async Task<long> UploadAsync(IFormFile file, UploadDocumentDto dto, int userId)
        {
            dto.File = file;

            if (dto.DocumentTypeId <= 0)
            {
                var requestedCode = dto.DocumentType?.Trim()?.ToUpperInvariant();
                DocumentType? matchedType = null;

                if (!string.IsNullOrWhiteSpace(requestedCode))
                {
                    matchedType = await _db.DocumentTypes.FirstOrDefaultAsync(x => x.IsActive && x.Code == requestedCode);
                }

                matchedType ??= await _db.DocumentTypes.FirstOrDefaultAsync(x => x.IsActive && x.IsRequired);
                matchedType ??= await _db.DocumentTypes.FirstOrDefaultAsync(x => x.IsActive);

                if (matchedType == null)
                {
                    throw new InvalidOperationException("لا توجد أنواع وثائق مفعلة");
                }

                dto.DocumentTypeId = matchedType.Id;
            }

            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                dto.Title = !string.IsNullOrWhiteSpace(dto.DocumentType) ? dto.DocumentType : "وثيقة";
            }

            if (dto.IssueDate == null && dto.DocumentDate.HasValue)
            {
                dto.IssueDate = dto.DocumentDate;
            }

            return await UploadAsync(dto, userId);
        }

        public async Task<long> UploadAsync(IFormFile file, WaqfSystem.Application.DTOs.Property.UploadDocumentDto dto, int userId)
        {
            var mapped = new UploadDocumentDto
            {
                PropertyId = dto.PropertyId,
                Title = !string.IsNullOrWhiteSpace(dto.DocumentType) ? dto.DocumentType : dto.DocumentCategory.ToString(),
                DocumentNumber = dto.DocumentNumber,
                IssuingAuthority = dto.IssuingAuthority,
                IssueDate = dto.DocumentDate,
                ExpiryDate = dto.ExpiryDate,
                Notes = dto.Notes,
                DocumentCategory = dto.DocumentCategory,
                DocumentType = dto.DocumentType,
                File = file
            };

            return await UploadAsync(file, mapped, userId);
        }

        public async Task<DocumentVersionDto> UploadNewVersionAsync(UploadNewVersionDto dto, int userId)
        {
            var document = await _db.PropertyDocuments
                .Include(x => x.DocumentType)
                .Include(x => x.Versions)
                .FirstOrDefaultAsync(x => x.Id == dto.DocumentId);

            if (document == null)
            {
                throw new InvalidOperationException("الوثيقة غير موجودة");
            }

            ValidateFile(dto.File, document.DocumentType);

            foreach (var current in document.Versions.Where(x => x.IsCurrent))
            {
                current.IsCurrent = false;
            }

            var extension = Path.GetExtension(dto.File.FileName).Trim('.').ToLowerInvariant();
            var folder = $"documents/{document.PropertyId}/{document.DocumentType.Code}";
            var fileUrl = await _fileStorage.UploadFileAsync(dto.File, folder);

            var newVersionNumber = document.Versions.Any() ? document.Versions.Max(x => x.VersionNumber) + 1 : 1;
            var newVersion = new DocumentVersion
            {
                DocumentId = document.Id,
                VersionNumber = newVersionNumber,
                FileUrl = fileUrl,
                FileName = Path.GetFileName(dto.File.FileName),
                FileExtension = extension,
                FileSizeBytes = dto.File.Length,
                MimeType = GetMimeType(dto.File, extension),
                UploadedById = userId,
                UploadedAt = DateTime.UtcNow,
                IsCurrent = true,
                Notes = dto.Notes
            };

            await _db.DocumentVersions.AddAsync(newVersion);
            await _db.SaveChangesAsync();

            var oldExpiry = document.ExpiryDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            document.CurrentVersionId = newVersion.Id;
            document.VersionCount += 1;
            document.Status = DocumentStatus.PendingVerification;
            document.VerifiedById = null;
            document.VerifiedAt = null;
            document.VerificationNotes = null;
            document.RejectionReason = null;
            document.UpdatedAt = DateTime.UtcNow;

            if (dto.UpdateExpiryDate.HasValue)
            {
                document.ExpiryDate = dto.UpdateExpiryDate.Value.Date;
                document.Alert1Sent = false;
                document.Alert2Sent = false;
                document.ExpiredAlertSent = false;

                await LogAuditAsync(document.Id, DocumentActionType.ExpiryChanged, userId,
                    "تم تعديل تاريخ انتهاء الوثيقة", oldExpiry,
                    dto.UpdateExpiryDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), newVersion.Id);
            }

            await _db.SaveChangesAsync();

            await LogAuditAsync(document.Id, DocumentActionType.NewVersionUploaded, userId,
                $"تم رفع نسخة جديدة رقم v{newVersionNumber}", null, null, newVersion.Id);

            foreach (var roleCode in ParseRoleCodes(document.DocumentType.VerifierRoles))
            {
                await _notifications.SendToRoleAsync(roleCode, "نسخة جديدة", $"نسخة جديدة من وثيقة {document.Title} تنتظر التحقق", "PropertyDocuments", (int)document.Id);
            }

            return ToVersionDto(newVersion, null);
        }

        public async Task VerifyAsync(VerifyDocumentDto dto, int userId)
        {
            var document = await _db.PropertyDocuments
                .Include(x => x.DocumentType)
                .FirstOrDefaultAsync(x => x.Id == dto.DocumentId);

            if (document == null)
            {
                throw new InvalidOperationException("الوثيقة غير موجودة");
            }

            var verifierRoles = ParseRoleCodes(document.DocumentType.VerifierRoles);
            if (verifierRoles.Count > 0)
            {
                var userRole = await _db.Users.Where(x => x.Id == userId).Select(x => x.Role.Code).FirstOrDefaultAsync();
                if (string.IsNullOrWhiteSpace(userRole) || !verifierRoles.Contains(userRole, StringComparer.OrdinalIgnoreCase))
                {
                    throw new UnauthorizedAccessException("لا تملك صلاحية التحقق من هذا النوع");
                }
            }

            if (dto.IsApproved)
            {
                document.Status = DocumentStatus.Verified;
                document.VerifiedById = userId;
                document.VerifiedAt = DateTime.UtcNow;
                document.VerificationNotes = dto.Notes;
                document.RejectionReason = null;
                document.UpdatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                await LogAuditAsync(document.Id, DocumentActionType.Verified, userId, dto.Notes);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(dto.Notes))
                {
                    throw new InvalidOperationException("سبب الرفض مطلوب");
                }

                document.Status = DocumentStatus.Rejected;
                document.RejectionReason = dto.Notes.Trim();
                document.VerificationNotes = null;
                document.VerifiedById = null;
                document.VerifiedAt = null;
                document.UpdatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                await LogAuditAsync(document.Id, DocumentActionType.Rejected, userId, dto.Notes);
            }
        }

        public async Task VerifyAsync(int id, int userId, VerificationMethod method, string? notes)
        {
            var dto = new VerifyDocumentDto
            {
                DocumentId = id,
                IsApproved = true,
                Notes = notes
            };

            await VerifyAsync(dto, userId);
        }

        public async Task DeleteAsync(int id, int userId)
        {
            await SoftDeleteAsync(id, "حذف عبر الواجهة القديمة", userId);
        }

        public async Task<long> AddPhotoAsync(IFormFile file, int propertyId, PhotoType photoType, int userId)
        {
            if (file == null || file.Length == 0)
            {
                throw new InvalidOperationException("الصورة غير موجودة أو فارغة");
            }

            var fileUrl = await _fileStorage.UploadFileAsync(file, $"photos/{propertyId}");
            var photo = new PropertyPhoto
            {
                PropertyId = propertyId,
                PhotoType = photoType,
                FileUrl = fileUrl,
                FileSizeKB = (int)Math.Ceiling(file.Length / 1024d),
                UploadedById = userId,
                CreatedAt = DateTime.UtcNow,
                CreatedById = userId
            };

            await _db.PropertyPhotos.AddAsync(photo);
            await _db.SaveChangesAsync();
            return photo.Id;
        }

        public async Task SoftDeleteAsync(long id, string reason, int userId)
        {
            var document = await _db.PropertyDocuments.FirstOrDefaultAsync(x => x.Id == id);
            if (document == null)
            {
                throw new InvalidOperationException("الوثيقة غير موجودة");
            }

            document.IsDeleted = true;
            document.DeletedAt = DateTime.UtcNow;
            document.DeletedById = userId;
            document.DeletedReason = reason;
            document.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            await LogAuditAsync(document.Id, DocumentActionType.SoftDeleted, userId, reason);
        }

        public async Task RestoreAsync(long id, int userId)
        {
            var document = await _db.PropertyDocuments.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id);
            if (document == null)
            {
                throw new InvalidOperationException("الوثيقة غير موجودة");
            }

            document.IsDeleted = false;
            document.DeletedAt = null;
            document.DeletedById = null;
            document.DeletedReason = null;
            document.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            await LogAuditAsync(document.Id, DocumentActionType.Restored, userId, "استعادة الوثيقة");
        }

        public async Task AssignResponsibleAsync(long documentId, int userId, string? notes, int assignedById)
        {
            var document = await _db.PropertyDocuments.FirstOrDefaultAsync(x => x.Id == documentId);
            if (document == null)
            {
                throw new InvalidOperationException("الوثيقة غير موجودة");
            }

            var existing = await _db.DocumentResponsibles.FirstOrDefaultAsync(x => x.DocumentId == documentId && x.UserId == userId);
            if (existing == null)
            {
                await _db.DocumentResponsibles.AddAsync(new DocumentResponsible
                {
                    DocumentId = documentId,
                    UserId = userId,
                    AssignedById = assignedById,
                    AssignedAt = DateTime.UtcNow,
                    IsActive = true,
                    Notes = notes
                });
            }
            else
            {
                existing.IsActive = true;
                existing.AssignedById = assignedById;
                existing.AssignedAt = DateTime.UtcNow;
                existing.Notes = notes;
            }

            document.PrimaryResponsibleId ??= userId;
            document.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            await LogAuditAsync(document.Id, DocumentActionType.ResponsibleAssigned, assignedById, $"تعيين مسؤول جديد: {userId}");
        }

        public async Task RemoveResponsibleAsync(long documentId, int userId, int removedById)
        {
            var responsible = await _db.DocumentResponsibles.FirstOrDefaultAsync(x => x.DocumentId == documentId && x.UserId == userId);
            if (responsible == null)
            {
                return;
            }

            responsible.IsActive = false;
            await _db.SaveChangesAsync();

            await LogAuditAsync(documentId, DocumentActionType.ResponsibleAssigned, removedById, "إزالة مسؤول من الوثيقة", userId.ToString(), "غير نشط");
        }

        public async Task RecordDownloadAsync(long documentId, long versionId, int userId)
        {
            var version = await _db.DocumentVersions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == versionId && x.DocumentId == documentId);
            if (version == null)
            {
                return;
            }

            await LogAuditAsync(documentId, DocumentActionType.Downloaded, userId,
                $"تنزيل النسخة v{version.VersionNumber} - {version.FileName}", null, null, versionId);
        }

        public async Task RecordViewAsync(long documentId, int userId)
        {
            var oneHourAgo = DateTime.UtcNow.AddHours(-1);
            var hasRecentView = await _db.DocumentAuditTrail.AnyAsync(x =>
                x.DocumentId == documentId &&
                x.ActionType == DocumentActionType.Viewed &&
                x.ActionByUserId == userId &&
                x.ActionAt >= oneHourAgo);

            if (!hasRecentView)
            {
                await LogAuditAsync(documentId, DocumentActionType.Viewed, userId, "عرض الوثيقة");
            }
        }

        public async Task ProcessOcrAsync(long documentId)
        {
            var document = await _db.PropertyDocuments.Include(x => x.CurrentVersion).FirstOrDefaultAsync(x => x.Id == documentId);
            if (document?.CurrentVersion == null)
            {
                return;
            }

            try
            {
                var result = await _ocr.ExtractTextAsync(document.CurrentVersion.FileUrl, document.CurrentVersion.MimeType);
                document.OcrText = result.Text;
                document.OcrConfidence = result.Confidence;
                document.OcrProcessedAt = DateTime.UtcNow;
                document.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                await LogAuditAsync(document.Id, DocumentActionType.OcrProcessed, null,
                    $"تمت معالجة OCR بنسبة {result.Confidence:F2}%");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "فشل OCR للوثيقة {DocumentId}", documentId);
            }
        }

        public async Task<PagedResult<PropertyDocumentListDto>> GetByPropertyAsync(long propertyId, DocumentFilterRequest filter)
        {
            var propertyIdInt = (int)propertyId;
            var baseQuery = _db.PropertyDocuments
                .Include(x => x.DocumentType)
                .Include(x => x.Property)
                .Include(x => x.PrimaryResponsible).ThenInclude(x => x!.Role)
                .Include(x => x.VerifiedBy)
                .Include(x => x.CreatedBy)
                .Include(x => x.CurrentVersion)
                .Include(x => x.Alerts)
                .Where(x => x.PropertyId == propertyIdInt);

            baseQuery = ApplyFilter(baseQuery, filter);

            var items = await baseQuery
                .OrderByDescending(x => x.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            foreach (var doc in items.Where(x => x.ExpiryDate.HasValue))
            {
                await _documentAlertService.CheckAndCreateAlertsAsync(doc.Id);
            }

            var totalCount = await ApplyFilter(_db.PropertyDocuments.Where(x => x.PropertyId == propertyIdInt), filter).CountAsync();

            var mapped = items.Select(ToListDto).ToList();
            return new PagedResult<PropertyDocumentListDto>
            {
                Items = mapped,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<PropertyDocumentDetailDto?> GetDetailAsync(long id, int userId)
        {
            var document = await _db.PropertyDocuments
                .Include(x => x.Property)
                .Include(x => x.DocumentType)
                .Include(x => x.CurrentVersion)
                .Include(x => x.PrimaryResponsible).ThenInclude(x => x!.Role)
                .Include(x => x.VerifiedBy)
                .Include(x => x.CreatedBy)
                .Include(x => x.LinkedUnit)
                .Include(x => x.Versions).ThenInclude(x => x.UploadedBy)
                .Include(x => x.Alerts)
                .Include(x => x.Responsibles).ThenInclude(x => x.User).ThenInclude(x => x.Role)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (document == null)
            {
                return null;
            }

            await _documentAlertService.CheckAndCreateAlertsAsync(document.Id);
            await EnsureExpiryStatusAsync(document);

            var detail = ToDetailDto(document);
            detail.AuditTrail = await GetAuditTrailAsync(id, 200);
            detail.ActiveAlerts = document.Alerts.OrderByDescending(x => x.CreatedAt).Select(ToAlertDto).ToList();
            detail.Responsibles = document.Responsibles.Where(x => x.IsActive).Select(ToResponsibleDto).ToList();
            detail.AllowedNextActions = await BuildAllowedActionsAsync(document, userId);
            detail.DocumentType = ToDocumentTypeDto(document.DocumentType, 0);

            return detail;
        }

        public async Task<PagedResult<PropertyDocumentListDto>> SearchAsync(DocumentFilterRequest filter, int userId)
        {
            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId);
            var query = _db.PropertyDocuments
                .Include(x => x.Property)
                .Include(x => x.DocumentType)
                .Include(x => x.PrimaryResponsible).ThenInclude(x => x!.Role)
                .Include(x => x.VerifiedBy)
                .Include(x => x.CreatedBy)
                .Include(x => x.CurrentVersion)
                .Include(x => x.Alerts)
                .AsQueryable();

            if (user?.GovernorateId != null)
            {
                query = query.Where(x => x.Property.GovernorateId == user.GovernorateId);
            }

            query = ApplyFilter(query, filter);
            query = ApplySorting(query, filter.SortBy, filter.SortDesc);

            var items = await query.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();
            var totalCount = await ApplyFilter(_db.PropertyDocuments.Include(x => x.Property), filter).CountAsync();

            return new PagedResult<PropertyDocumentListDto>
            {
                Items = items.Select(ToListDto).ToList(),
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<PropertyDocumentSummaryDto> GetPropertySummaryAsync(long propertyId)
        {
            var propertyIdInt = (int)propertyId;
            var docs = await _db.PropertyDocuments
                .Include(x => x.DocumentType)
                .Where(x => x.PropertyId == propertyIdInt)
                .ToListAsync();

            var total = docs.Count;
            var verified = docs.Count(x => x.Status == DocumentStatus.Verified);
            var pending = docs.Count(x => x.Status == DocumentStatus.PendingVerification);
            var expired = docs.Count(x => x.ExpiryDate.HasValue && x.ExpiryDate.Value.Date < DateTime.Today);
            var expiring30 = docs.Count(x => x.ExpiryDate.HasValue && (x.ExpiryDate.Value.Date - DateTime.Today).Days <= 30 && (x.ExpiryDate.Value.Date - DateTime.Today).Days > 0);
            var expiring90 = docs.Count(x => x.ExpiryDate.HasValue && (x.ExpiryDate.Value.Date - DateTime.Today).Days <= 90 && (x.ExpiryDate.Value.Date - DateTime.Today).Days > 0);

            var requiredTypes = await _db.DocumentTypes.Where(x => x.IsRequired && x.IsActive).ToListAsync();
            var metRequiredTypes = requiredTypes.Count(type => docs.Any(d => d.DocumentTypeId == type.Id && d.Status == DocumentStatus.Verified));
            var missingRequiredTypes = requiredTypes
                .Where(type => !docs.Any(d => d.DocumentTypeId == type.Id && d.Status == DocumentStatus.Verified))
                .Select(x => x.NameAr)
                .ToList();

            var compliance = requiredTypes.Count == 0 ? 100m : Math.Round((decimal)metRequiredTypes / requiredTypes.Count * 100m, 2);

            return new PropertyDocumentSummaryDto
            {
                PropertyId = propertyId,
                TotalDocuments = total,
                VerifiedCount = verified,
                PendingCount = pending,
                ExpiredCount = expired,
                ExpiringSoonCount = expiring30,
                ExpiringSoon90Count = expiring90,
                MissingRequiredTypes = missingRequiredTypes,
                CompliancePercent = compliance,
                LastUpdatedAt = docs.OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt).Select(x => x.UpdatedAt ?? x.CreatedAt).FirstOrDefault()
            };
        }

        public async Task<PagedResult<DocumentAlertDto>> GetAlertsAsync(DocumentAlertFilterRequest filter, int userId)
        {
            var query = _db.DocumentAlerts
                .Include(x => x.Document).ThenInclude(x => x.Property)
                .Include(x => x.Document).ThenInclude(x => x.DocumentType)
                .AsQueryable();

            if (filter.PropertyId.HasValue)
            {
                query = query.Where(x => x.PropertyId == (int)filter.PropertyId.Value);
            }

            if (filter.AlertLevel.HasValue)
            {
                query = query.Where(x => (int)x.AlertLevel == filter.AlertLevel.Value);
            }

            if (filter.DocumentTypeId.HasValue)
            {
                query = query.Where(x => x.Document.DocumentTypeId == filter.DocumentTypeId.Value);
            }

            if (filter.IsRead.HasValue)
            {
                query = query.Where(x => x.IsRead == filter.IsRead.Value);
            }

            if (filter.DateFrom.HasValue)
            {
                query = query.Where(x => x.CreatedAt >= filter.DateFrom.Value.Date);
            }

            if (filter.DateTo.HasValue)
            {
                query = query.Where(x => x.CreatedAt <= filter.DateTo.Value.Date.AddDays(1).AddTicks(-1));
            }

            var totalCount = await query.CountAsync();
            var items = await query.OrderByDescending(x => x.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PagedResult<DocumentAlertDto>
            {
                Items = items.Select(ToAlertDto).ToList(),
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task MarkAlertReadAsync(long alertId, int userId)
        {
            var alert = await _db.DocumentAlerts.FirstOrDefaultAsync(x => x.Id == alertId);
            if (alert == null)
            {
                return;
            }

            alert.IsRead = true;
            alert.ReadAt = DateTime.UtcNow;
            alert.ReadByUserId = userId;
            await _db.SaveChangesAsync();

            await LogAuditAsync(alert.DocumentId, DocumentActionType.AlertRead, userId, $"تمت قراءة التنبيه {alert.Id}");
        }

        public async Task MarkAllAlertsReadAsync(long? propertyId, int userId)
        {
            var query = _db.DocumentAlerts.Where(x => !x.IsRead);
            if (propertyId.HasValue)
            {
                query = query.Where(x => x.PropertyId == (int)propertyId.Value);
            }

            var alerts = await query.ToListAsync();
            foreach (var alert in alerts)
            {
                alert.IsRead = true;
                alert.ReadAt = DateTime.UtcNow;
                alert.ReadByUserId = userId;
            }

            await _db.SaveChangesAsync();
        }

        public async Task<List<DocumentTypeDto>> GetDocumentTypesAsync()
        {
            var counts = await _db.PropertyDocuments
                .GroupBy(x => x.DocumentTypeId)
                .Select(g => new { DocumentTypeId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.DocumentTypeId, x => x.Count);

            var types = await _db.DocumentTypes.Where(x => x.IsActive).OrderBy(x => x.SortOrder).ThenBy(x => x.NameAr).ToListAsync();
            return types.Select(x => ToDocumentTypeDto(x, counts.TryGetValue(x.Id, out var c) ? c : 0)).ToList();
        }

        public async Task<List<DocumentAuditTrailDto>> GetAuditTrailAsync(long documentId, int take = 100)
        {
            var trail = await _db.DocumentAuditTrail
                .Include(x => x.ActionByUser)
                .Include(x => x.Version)
                .Where(x => x.DocumentId == documentId)
                .OrderByDescending(x => x.ActionAt)
                .Take(take)
                .ToListAsync();

            return trail.Select(ToAuditDto).ToList();
        }

        private IQueryable<PropertyDocument> ApplyFilter(IQueryable<PropertyDocument> query, DocumentFilterRequest filter)
        {
            if (filter.PropertyId.HasValue)
            {
                query = query.Where(x => x.PropertyId == (int)filter.PropertyId.Value);
            }

            if (filter.DocumentTypeId.HasValue)
            {
                query = query.Where(x => x.DocumentTypeId == filter.DocumentTypeId.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Category))
            {
                query = query.Where(x => x.DocumentType.Category == filter.Category);
            }

            if (filter.Status.HasValue)
            {
                query = query.Where(x => x.Status == filter.Status.Value);
            }

            if (filter.ExpiryDateFrom.HasValue)
            {
                query = query.Where(x => x.ExpiryDate >= filter.ExpiryDateFrom.Value.Date);
            }

            if (filter.ExpiryDateTo.HasValue)
            {
                query = query.Where(x => x.ExpiryDate <= filter.ExpiryDateTo.Value.Date);
            }

            if (filter.PrimaryResponsibleId.HasValue)
            {
                query = query.Where(x => x.PrimaryResponsibleId == filter.PrimaryResponsibleId.Value);
            }

            if (filter.IsExpired == true)
            {
                query = query.Where(x => x.ExpiryDate.HasValue && x.ExpiryDate.Value < DateTime.Today);
            }

            if (filter.IsExpiringSoon == true)
            {
                var today = DateTime.Today;
                var day90 = today.AddDays(90);
                query = query.Where(x => x.ExpiryDate.HasValue && x.ExpiryDate.Value > today && x.ExpiryDate.Value <= day90);
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim();
                query = query.Where(x =>
                    x.Title.Contains(term) ||
                    (x.DocumentNumber != null && x.DocumentNumber.Contains(term)) ||
                    (x.OcrText != null && x.OcrText.Contains(term)) ||
                    (x.Tags != null && x.Tags.Contains($"\"{term}\"")));
            }

            return query;
        }

        private IQueryable<PropertyDocument> ApplySorting(IQueryable<PropertyDocument> query, string sortBy, bool sortDesc)
        {
            return (sortBy ?? string.Empty).ToLowerInvariant() switch
            {
                "expirydate" => sortDesc ? query.OrderByDescending(x => x.ExpiryDate) : query.OrderBy(x => x.ExpiryDate),
                "status" => sortDesc ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
                "title" => sortDesc ? query.OrderByDescending(x => x.Title) : query.OrderBy(x => x.Title),
                _ => sortDesc ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt)
            };
        }

        private static string SerializeTags(List<string>? tags)
        {
            return tags == null || tags.Count == 0 ? "[]" : JsonSerializer.Serialize(tags.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct(StringComparer.OrdinalIgnoreCase));
        }

        private static List<string> ParseTags(string? tags)
        {
            if (string.IsNullOrWhiteSpace(tags))
            {
                return new List<string>();
            }

            try
            {
                var list = JsonSerializer.Deserialize<List<string>>(tags);
                return list ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        private static List<string> ParseRoleCodes(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<string>();
            }

            try
            {
                var list = JsonSerializer.Deserialize<List<string>>(json);
                return list?.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList() ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        private static string GetMimeType(Microsoft.AspNetCore.Http.IFormFile file, string extension)
        {
            if (!string.IsNullOrWhiteSpace(file.ContentType))
            {
                return file.ContentType;
            }

            return extension.ToLowerInvariant() switch
            {
                "pdf" => "application/pdf",
                "jpg" => "image/jpeg",
                "jpeg" => "image/jpeg",
                "png" => "image/png",
                "tif" => "image/tiff",
                "tiff" => "image/tiff",
                _ => "application/octet-stream"
            };
        }

        private static string FormatFileSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{Math.Round(bytes / 1024d, 1)} KB";
            return $"{Math.Round(bytes / (1024d * 1024d), 1)} MB";
        }

        private void ValidateFile(Microsoft.AspNetCore.Http.IFormFile file, DocumentType type)
        {
            if (file == null || file.Length == 0)
            {
                throw new InvalidOperationException("الملف غير موجود أو فارغ");
            }

            var ext = Path.GetExtension(file.FileName).Trim('.').ToLowerInvariant();
            var allowed = (type.AllowedExtensions ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(x => x.ToLowerInvariant())
                .ToHashSet();

            if (!allowed.Contains(ext))
            {
                throw new InvalidOperationException("امتداد الملف غير مسموح لهذا النوع");
            }

            var maxSize = type.MaxFileSizeMB * 1024L * 1024L;
            if (file.Length > maxSize)
            {
                throw new InvalidOperationException($"حجم الملف يتجاوز الحد الأقصى ({type.MaxFileSizeMB} MB)");
            }

            if (!IsMagicBytesValid(file, ext))
            {
                throw new InvalidOperationException("نوع الملف الفعلي لا يطابق الامتداد");
            }
        }

        private static bool IsMagicBytesValid(Microsoft.AspNetCore.Http.IFormFile file, string extension)
        {
            using var stream = file.OpenReadStream();
            var header = new byte[8];
            var read = stream.Read(header, 0, header.Length);
            if (read < 3)
            {
                return false;
            }

            if (extension == "pdf")
            {
                return read >= 4 && header[0] == 0x25 && header[1] == 0x50 && header[2] == 0x44 && header[3] == 0x46;
            }

            if (extension == "jpg" || extension == "jpeg")
            {
                return header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;
            }

            if (extension == "png")
            {
                return read >= 4 && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47;
            }

            return true;
        }

        private async Task EnsureExpiryStatusAsync(PropertyDocument document)
        {
            if (document.ExpiryDate.HasValue && document.ExpiryDate.Value.Date < DateTime.Today && document.Status != DocumentStatus.Expired)
            {
                document.Status = DocumentStatus.Expired;
                document.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
        }

        private async Task<List<string>> BuildAllowedActionsAsync(PropertyDocument document, int userId)
        {
            var actions = new List<string> { "عرض", "تحميل" };
            var userRoleCode = await _db.Users.Where(x => x.Id == userId).Select(x => x.Role.Code).FirstOrDefaultAsync();
            var verifierRoles = ParseRoleCodes(document.DocumentType.VerifierRoles);

            if (document.Status == DocumentStatus.PendingVerification && !string.IsNullOrWhiteSpace(userRoleCode) && verifierRoles.Contains(userRoleCode, StringComparer.OrdinalIgnoreCase))
            {
                actions.Add("تحقق");
                actions.Add("رفض");
            }

            if (document.Status == DocumentStatus.Verified || document.Status == DocumentStatus.Expired)
            {
                actions.Add("رفع نسخة جديدة");
            }

            actions.Add(document.IsDeleted ? "استعادة" : "حذف");
            return actions;
        }

        private PropertyDocumentListDto ToListDto(PropertyDocument document)
        {
            var days = document.ExpiryDate.HasValue ? (document.ExpiryDate.Value.Date - DateTime.Today).Days : (int?)null;
            var isExpired = days.HasValue && days.Value < 0;
            var isExpiringSoon = days.HasValue && days.Value <= 90 && days.Value > 0;

            return new PropertyDocumentListDto
            {
                Id = document.Id,
                PropertyId = document.PropertyId,
                PropertyNameAr = document.Property.PropertyName ?? string.Empty,
                PropertyWqfNumber = document.Property.WqfNumber,
                DocumentTypeId = document.DocumentTypeId,
                DocumentTypeNameAr = document.DocumentType.NameAr,
                DocumentTypeCode = document.DocumentType.Code,
                Category = document.DocumentType.Category,
                Title = document.Title,
                DocumentNumber = document.DocumentNumber,
                IssuingAuthority = document.IssuingAuthority,
                IssueDate = document.IssueDate,
                ExpiryDate = document.ExpiryDate,
                Status = document.Status,
                StatusDisplayAr = ToStatusAr(document.Status),
                StatusColor = ToStatusColor(document.Status),
                DaysUntilExpiry = days,
                IsExpired = isExpired,
                IsExpiringSoon = isExpiringSoon,
                VersionCount = document.VersionCount,
                CurrentVersionId = document.CurrentVersionId,
                CurrentFileName = document.CurrentVersion?.FileName,
                CurrentFileSizeBytes = document.CurrentVersion?.FileSizeBytes,
                PrimaryResponsibleName = document.PrimaryResponsible?.FullNameAr,
                VerifiedByName = document.VerifiedBy?.FullNameAr,
                VerifiedAt = document.VerifiedAt,
                Tags = ParseTags(document.Tags),
                HasOcr = !string.IsNullOrWhiteSpace(document.OcrText),
                CreatedAt = document.CreatedAt,
                CreatedByName = document.CreatedBy.FullNameAr,
                UnreadAlertCount = document.Alerts?.Count(a => !a.IsRead) ?? 0
            };
        }

        private PropertyDocumentDetailDto ToDetailDto(PropertyDocument document)
        {
            var dto = new PropertyDocumentDetailDto();
            var baseDto = ToListDto(document);

            dto.Id = baseDto.Id;
            dto.PropertyId = baseDto.PropertyId;
            dto.PropertyNameAr = baseDto.PropertyNameAr;
            dto.PropertyWqfNumber = baseDto.PropertyWqfNumber;
            dto.DocumentTypeId = baseDto.DocumentTypeId;
            dto.DocumentTypeNameAr = baseDto.DocumentTypeNameAr;
            dto.DocumentTypeCode = baseDto.DocumentTypeCode;
            dto.Category = baseDto.Category;
            dto.Title = baseDto.Title;
            dto.DocumentNumber = baseDto.DocumentNumber;
            dto.IssuingAuthority = baseDto.IssuingAuthority;
            dto.IssueDate = baseDto.IssueDate;
            dto.ExpiryDate = baseDto.ExpiryDate;
            dto.Status = baseDto.Status;
            dto.StatusDisplayAr = baseDto.StatusDisplayAr;
            dto.StatusColor = baseDto.StatusColor;
            dto.DaysUntilExpiry = baseDto.DaysUntilExpiry;
            dto.IsExpired = baseDto.IsExpired;
            dto.IsExpiringSoon = baseDto.IsExpiringSoon;
            dto.VersionCount = baseDto.VersionCount;
            dto.CurrentVersionId = baseDto.CurrentVersionId;
            dto.CurrentFileName = baseDto.CurrentFileName;
            dto.CurrentFileSizeBytes = baseDto.CurrentFileSizeBytes;
            dto.PrimaryResponsibleName = baseDto.PrimaryResponsibleName;
            dto.VerifiedByName = baseDto.VerifiedByName;
            dto.VerifiedAt = baseDto.VerifiedAt;
            dto.Tags = baseDto.Tags;
            dto.HasOcr = baseDto.HasOcr;
            dto.CreatedAt = baseDto.CreatedAt;
            dto.CreatedByName = baseDto.CreatedByName;
            dto.UnreadAlertCount = baseDto.UnreadAlertCount;

            dto.Description = document.Description;
            dto.OcrText = document.OcrText;
            dto.OcrConfidence = document.OcrConfidence;
            dto.LinkedUnitId = document.LinkedUnitId;
            dto.LinkedUnitNumber = document.LinkedUnit?.UnitNumber;
            dto.LinkedPartnershipId = document.LinkedPartnershipId;
            dto.VerificationNotes = document.VerificationNotes;
            dto.RejectionReason = document.RejectionReason;
            dto.Versions = document.Versions.OrderByDescending(x => x.VersionNumber).Select(v => ToVersionDto(v, v.UploadedBy?.FullNameAr)).ToList();

            return dto;
        }

        private static DocumentVersionDto ToVersionDto(DocumentVersion version, string? uploadedByName)
        {
            return new DocumentVersionDto
            {
                Id = version.Id,
                DocumentId = version.DocumentId,
                VersionNumber = version.VersionNumber,
                FileUrl = version.FileUrl,
                FileName = version.FileName,
                FileExtension = version.FileExtension,
                FileSizeBytes = version.FileSizeBytes,
                FileSizeDisplay = FormatFileSize(version.FileSizeBytes),
                MimeType = version.MimeType,
                ThumbnailUrl = version.ThumbnailUrl,
                PageCount = version.PageCount,
                UploadedByName = uploadedByName ?? string.Empty,
                UploadedAt = version.UploadedAt,
                IsCurrent = version.IsCurrent,
                Notes = version.Notes
            };
        }

        private DocumentAuditTrailDto ToAuditDto(DocumentAuditTrail audit)
        {
            return new DocumentAuditTrailDto
            {
                Id = audit.Id,
                ActionType = audit.ActionType,
                ActionTypeDisplayAr = ToActionAr(audit.ActionType),
                ActionByName = audit.ActionByUser?.FullNameAr,
                ActionAt = audit.ActionAt,
                Details = audit.Details,
                OldValue = audit.OldValue,
                NewValue = audit.NewValue,
                VersionNumber = audit.Version?.VersionNumber
            };
        }

        private DocumentAlertDto ToAlertDto(DocumentAlert alert)
        {
            var dto = new DocumentAlertDto
            {
                Id = alert.Id,
                DocumentId = alert.DocumentId,
                DocumentTitle = alert.Document?.Title ?? string.Empty,
                PropertyNameAr = alert.Document?.Property?.PropertyName ?? string.Empty,
                PropertyWqfNumber = alert.Document?.Property?.WqfNumber ?? string.Empty,
                DocumentTypeNameAr = alert.Document?.DocumentType?.NameAr ?? string.Empty,
                ExpiryDate = alert.Document?.ExpiryDate,
                DaysRemaining = alert.DaysRemaining,
                AlertLevel = alert.AlertLevel,
                AlertType = alert.AlertType,
                IsRead = alert.IsRead,
                ReadAt = alert.ReadAt,
                CreatedAt = alert.CreatedAt
            };

            dto.UrgencyColor = alert.AlertLevel switch
            {
                DocumentAlertLevel.Expired => "#dc2626",
                DocumentAlertLevel.Day30 => "#ea580c",
                _ => "#d97706"
            };

            dto.UrgencyLabel = alert.AlertLevel switch
            {
                DocumentAlertLevel.Expired => "منتهية",
                DocumentAlertLevel.Day30 => "حرج",
                _ => "تنبيه"
            };

            return dto;
        }

        private static DocumentResponsibleDto ToResponsibleDto(DocumentResponsible responsible)
        {
            return new DocumentResponsibleDto
            {
                Id = responsible.Id,
                UserId = responsible.UserId,
                FullNameAr = responsible.User.FullNameAr,
                Role = responsible.User.Role?.DisplayNameAr ?? responsible.User.Role?.Code ?? string.Empty,
                Phone = responsible.User.Phone,
                Email = responsible.User.Email,
                AssignedAt = responsible.AssignedAt,
                IsActive = responsible.IsActive,
                Notes = responsible.Notes
            };
        }

        private static DocumentTypeDto ToDocumentTypeDto(DocumentType type, int docCount)
        {
            return new DocumentTypeDto
            {
                Id = type.Id,
                Code = type.Code,
                NameAr = type.NameAr,
                NameEn = type.NameEn,
                Category = type.Category,
                IsRequired = type.IsRequired,
                HasExpiry = type.HasExpiry,
                AlertDays1 = type.AlertDays1,
                AlertDays2 = type.AlertDays2,
                AllowedExtensions = type.AllowedExtensions,
                VerifierRoles = ParseRoleCodes(type.VerifierRoles),
                IsActive = type.IsActive,
                DocumentCount = docCount
            };
        }

        private static string ToStatusAr(DocumentStatus status)
        {
            return status switch
            {
                DocumentStatus.PendingVerification => "تنتظر تحقق",
                DocumentStatus.Verified => "معتمدة",
                DocumentStatus.Rejected => "مرفوضة",
                DocumentStatus.ExpiringSoon => "تنتهي قريباً",
                DocumentStatus.Expired => "منتهية",
                DocumentStatus.Archived => "مؤرشفة",
                _ => "غير معروف"
            };
        }

        private static string ToStatusColor(DocumentStatus status)
        {
            return status switch
            {
                DocumentStatus.PendingVerification => "#378ADD",
                DocumentStatus.Verified => "#3B6D11",
                DocumentStatus.Rejected => "#A32D2D",
                DocumentStatus.ExpiringSoon => "#BA7517",
                DocumentStatus.Expired => "#DC2626",
                DocumentStatus.Archived => "#888780",
                _ => "#6b7280"
            };
        }

        private static string ToActionAr(DocumentActionType action)
        {
            return action switch
            {
                DocumentActionType.Uploaded => "رفع",
                DocumentActionType.Downloaded => "تحميل",
                DocumentActionType.Verified => "اعتماد",
                DocumentActionType.Rejected => "رفض",
                DocumentActionType.NewVersionUploaded => "رفع نسخة جديدة",
                DocumentActionType.ResponsibleAssigned => "تعيين مسؤول",
                DocumentActionType.ExpiryChanged => "تغيير الانتهاء",
                DocumentActionType.AlertCreated => "إنشاء تنبيه",
                DocumentActionType.AlertRead => "قراءة تنبيه",
                DocumentActionType.SoftDeleted => "حذف ناعم",
                DocumentActionType.Restored => "استعادة",
                DocumentActionType.OcrProcessed => "معالجة OCR",
                DocumentActionType.LinkedToUnit => "ربط بوحدة",
                DocumentActionType.LinkedToPartnership => "ربط بشراكة",
                DocumentActionType.Viewed => "عرض",
                _ => action.ToString()
            };
        }

        private async Task LogAuditAsync(long docId, DocumentActionType action, int? userId, string? details = null, string? oldVal = null, string? newVal = null, long? versionId = null)
        {
            var propertyId = await _db.PropertyDocuments.IgnoreQueryFilters().Where(x => x.Id == docId).Select(x => x.PropertyId).FirstOrDefaultAsync();
            var audit = new DocumentAuditTrail
            {
                DocumentId = docId,
                PropertyId = propertyId,
                ActionType = action,
                ActionByUserId = userId,
                ActionAt = DateTime.UtcNow,
                VersionId = versionId,
                Details = details,
                OldValue = oldVal,
                NewValue = newVal
            };

            _db.DocumentAuditTrail.Add(audit);
            await _db.SaveChangesAsync();
        }
    }
}
