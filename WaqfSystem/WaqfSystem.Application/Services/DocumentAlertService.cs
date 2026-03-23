using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WaqfSystem.Application.DTOs.Document;
using WaqfSystem.Core.Entities;
using WaqfSystem.Core.Interfaces;

namespace WaqfSystem.Application.Services
{
    public class DocumentAlertService : IDocumentAlertService
    {
        private readonly IAppDbContext _db;
        private readonly INotificationService _notifications;
        private readonly ILogger<DocumentAlertService> _logger;

        public DocumentAlertService(IAppDbContext db, INotificationService notifications, ILogger<DocumentAlertService> logger)
        {
            _db = db;
            _notifications = notifications;
            _logger = logger;
        }

        public async Task CheckAndCreateAlertsAsync(long documentId)
        {
            var document = await _db.PropertyDocuments
                .Include(x => x.DocumentType)
                .Include(x => x.Property)
                .Include(x => x.PrimaryResponsible)
                .FirstOrDefaultAsync(x => x.Id == documentId);

            if (document == null || !document.ExpiryDate.HasValue)
            {
                return;
            }

            var today = DateTime.Today;
            var daysRemaining = (document.ExpiryDate.Value.Date - today).Days;
            var alertDays1 = document.DocumentType.AlertDays1 ?? 90;
            var alertDays2 = document.DocumentType.AlertDays2 ?? 30;

            if (daysRemaining <= 0 && !document.ExpiredAlertSent)
            {
                var created = await TryCreateAlertAsync(document, DocumentAlertLevel.Expired, DocumentAlertType.Expired, daysRemaining, "انتهت صلاحية الوثيقة");
                if (created)
                {
                    document.Status = DocumentStatus.Expired;
                    document.ExpiredAlertSent = true;
                    await _db.SaveChangesAsync();
                }

                return;
            }

            if (daysRemaining <= alertDays2 && !document.Alert2Sent)
            {
                var created = await TryCreateAlertAsync(document, DocumentAlertLevel.Day30, DocumentAlertType.ExpiringSoon, daysRemaining, "تنبيه حرج قبل الانتهاء");
                if (created)
                {
                    if (document.Status == DocumentStatus.Verified)
                    {
                        document.Status = DocumentStatus.ExpiringSoon;
                    }

                    document.Alert2Sent = true;
                    await _db.SaveChangesAsync();
                }

                return;
            }

            if (daysRemaining <= alertDays1 && !document.Alert1Sent)
            {
                var created = await TryCreateAlertAsync(document, DocumentAlertLevel.Day90, DocumentAlertType.ExpiringSoon, daysRemaining, "تنبيه مبكر قبل الانتهاء");
                if (created)
                {
                    if (document.Status == DocumentStatus.Verified)
                    {
                        document.Status = DocumentStatus.ExpiringSoon;
                    }

                    document.Alert1Sent = true;
                    await _db.SaveChangesAsync();
                }
            }
        }

        public async Task<int> ProcessAllExpiryChecksAsync()
        {
            var docs = await _db.PropertyDocuments
                .Include(x => x.DocumentType)
                .Where(x => x.ExpiryDate.HasValue && !x.IsDeleted)
                .Select(x => x.Id)
                .ToListAsync();

            var before = await _db.DocumentAlerts.CountAsync();
            foreach (var id in docs)
            {
                await CheckAndCreateAlertsAsync(id);
            }

            var after = await _db.DocumentAlerts.CountAsync();
            return Math.Max(after - before, 0);
        }

        public async Task<List<DocumentAlertDto>> GetUnreadAlertsForUserAsync(int userId)
        {
            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId);
            var query = _db.DocumentAlerts
                .Include(x => x.Document).ThenInclude(x => x.Property)
                .Include(x => x.Document).ThenInclude(x => x.DocumentType)
                .Where(x => !x.IsRead)
                .AsQueryable();

            if (user?.GovernorateId != null)
            {
                query = query.Where(x => x.Document.Property.GovernorateId == user.GovernorateId);
            }

            var items = await query.OrderByDescending(x => x.CreatedAt).Take(50).ToListAsync();
            return items.Select(ToAlertDto).ToList();
        }

        public async Task<int> GetUnreadAlertCountAsync(int userId)
        {
            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId);
            var query = _db.DocumentAlerts.Where(x => !x.IsRead).AsQueryable();

            if (user?.GovernorateId != null)
            {
                query = query.Where(x => x.Document.Property.GovernorateId == user.GovernorateId);
            }

            return await query.CountAsync();
        }

        private async Task<bool> TryCreateAlertAsync(PropertyDocument document, DocumentAlertLevel level, DocumentAlertType type, int daysRemaining, string details)
        {
            var since = DateTime.UtcNow.AddDays(-7);
            var exists = await _db.DocumentAlerts.AnyAsync(x =>
                x.DocumentId == document.Id &&
                x.AlertLevel == level &&
                x.CreatedAt >= since);

            if (exists)
            {
                return false;
            }

            var notifiedUsers = new List<int>();
            if (document.PrimaryResponsibleId.HasValue)
            {
                notifiedUsers.Add(document.PrimaryResponsibleId.Value);
            }

            var alert = new DocumentAlert
            {
                DocumentId = document.Id,
                PropertyId = document.PropertyId,
                AlertLevel = level,
                AlertType = type,
                DaysRemaining = daysRemaining,
                IsRead = false,
                NotifiedUserIds = JsonSerializer.Serialize(notifiedUsers),
                CreatedAt = DateTime.UtcNow
            };

            await _db.DocumentAlerts.AddAsync(alert);
            await _db.DocumentAuditTrail.AddAsync(new DocumentAuditTrail
            {
                DocumentId = document.Id,
                PropertyId = document.PropertyId,
                ActionType = DocumentActionType.AlertCreated,
                ActionByUserId = null,
                ActionAt = DateTime.UtcNow,
                Details = details,
                NewValue = $"Level={(int)level}, Days={daysRemaining}"
            });

            await _db.SaveChangesAsync();

            var message = level == DocumentAlertLevel.Expired
                ? $"الوثيقة {document.Title} منتهية الصلاحية"
                : $"الوثيقة {document.Title} ستنتهي خلال {daysRemaining} يوم";

            await _notifications.SendToRoleAsync("SYS_ADMIN", "تنبيه وثيقة", message, "PropertyDocuments", (int)document.Id);

            _logger.LogInformation("Document alert created. Document={DocumentId}, Level={Level}, Days={Days}", document.Id, level, daysRemaining);
            return true;
        }

        private static DocumentAlertDto ToAlertDto(DocumentAlert alert)
        {
            var urgencyColor = alert.AlertLevel switch
            {
                DocumentAlertLevel.Expired => "#dc2626",
                DocumentAlertLevel.Day30 => "#ea580c",
                _ => "#d97706"
            };

            var urgencyLabel = alert.AlertLevel switch
            {
                DocumentAlertLevel.Expired => "منتهية الصلاحية",
                DocumentAlertLevel.Day30 => "تنتهي خلال 30 يوم",
                _ => "تنتهي خلال 90 يوم"
            };

            return new DocumentAlertDto
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
                CreatedAt = alert.CreatedAt,
                UrgencyColor = urgencyColor,
                UrgencyLabel = urgencyLabel
            };
        }
    }
}
