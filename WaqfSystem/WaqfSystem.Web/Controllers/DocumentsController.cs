using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using WaqfSystem.Application.DTOs.Document;
using WaqfSystem.Application.Services;
using WaqfSystem.Infrastructure.Authorization;
using WaqfSystem.Web.ViewModels.Document;

namespace WaqfSystem.Web.Controllers
{
    [Authorize]
    public class DocumentsController : BaseController
    {
        private readonly IDocumentService _documentService;
        private readonly IDocumentAlertService _documentAlertService;
        private readonly IAdminService _adminService;
        private readonly Microsoft.Extensions.Logging.ILogger<DocumentsController> _logger;

        public DocumentsController(
            IDocumentService documentService,
            IDocumentAlertService documentAlertService,
            IAdminService adminService,
            Microsoft.Extensions.Logging.ILogger<DocumentsController> logger)
        {
            _documentService = documentService;
            _documentAlertService = documentAlertService;
            _adminService = adminService;
            _logger = logger;
        }

        [HttpGet]
        [RequirePermission(PermissionKeys.Documents_View)]
        public async Task<IActionResult> Index([FromQuery] DocumentFilterRequest filter)
        {
            var docs = await _documentService.SearchAsync(filter, CurrentUserId);
            var types = await _documentService.GetDocumentTypesAsync();

            var vm = new DocumentIndexViewModel
            {
                Documents = docs,
                Filter = filter,
                DocumentTypes = types.Select(x => new SelectListItem(x.NameAr, x.Id.ToString())).ToList(),
                Categories = types.Select(x => x.Category).Distinct().OrderBy(x => x).Select(x => new SelectListItem(x, x)).ToList(),
                AlertSummary = new DocumentAlertSummaryVm
                {
                    ExpiredCount = docs.Items.Count(x => x.IsExpired),
                    Expiring30 = docs.Items.Count(x => x.DaysUntilExpiry.HasValue && x.DaysUntilExpiry.Value <= 30 && x.DaysUntilExpiry.Value > 0),
                    Expiring90 = docs.Items.Count(x => x.IsExpiringSoon)
                },
                CurrentUserRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty
            };

            return View("Index", vm);
        }

        [HttpGet]
        [RequirePermission(PermissionKeys.Documents_View)]
        public async Task<IActionResult> ForProperty(long propertyId, [FromQuery] DocumentFilterRequest filter)
        {
            filter.PropertyId = propertyId;
            var docs = await _documentService.GetByPropertyAsync(propertyId, filter);
            var summary = await _documentService.GetPropertySummaryAsync(propertyId);
            var types = await _documentService.GetDocumentTypesAsync();

            var first = docs.Items.FirstOrDefault();
            var vm = new ForPropertyViewModel
            {
                PropertyId = propertyId,
                PropertyNameAr = first?.PropertyNameAr ?? "العقار",
                PropertyWqfNumber = first?.PropertyWqfNumber ?? string.Empty,
                Summary = summary,
                Documents = docs,
                Filter = filter,
                DocumentTypes = types.Select(x => new SelectListItem(x.NameAr, x.Id.ToString())).ToList(),
                CanUpload = true,
                CanVerify = true
            };

            vm.DocumentTypeGroups = types.Select(type =>
            {
                var groupDocs = docs.Items.Where(d => d.DocumentTypeId == type.Id).ToList();
                return new DocumentTypeGroupVm
                {
                    DocumentType = type,
                    Documents = groupDocs,
                    HasRequired = type.IsRequired,
                    RequirementMet = !type.IsRequired || groupDocs.Any(g => g.Status == Core.Entities.DocumentStatus.Verified)
                };
            }).ToList();

            return View("ForProperty", vm);
        }

        [HttpGet]
        [RequirePermission(PermissionKeys.Documents_View)]
        public async Task<IActionResult> Detail(long id)
        {
            await _documentService.RecordViewAsync(id, CurrentUserId);
            var detail = await _documentService.GetDetailAsync(id, CurrentUserId);
            if (detail == null)
            {
                return NotFound();
            }

            var users = await _adminService.GetUsersAsync(new WaqfSystem.Application.DTOs.Admin.UserFilterRequest { Page = 1, PageSize = 200 });
            var vm = new DocumentDetailViewModel
            {
                Document = detail,
                CanUpload = true,
                CanVerify = true,
                CanDelete = true,
                CanAssignResponsible = true,
                AvailableResponsibles = users.Items.Select(u => new SelectListItem($"{u.FullNameAr} - {u.RoleDisplayNameAr}", u.Id.ToString())).ToList()
            };

            return View("Detail", vm);
        }

        [HttpGet]
        [RequirePermission(PermissionKeys.Documents_Upload)]
        public async Task<IActionResult> Upload(long propertyId, int? documentTypeId)
        {
            var types = await _documentService.GetDocumentTypesAsync();
            var users = await _adminService.GetUsersAsync(new WaqfSystem.Application.DTOs.Admin.UserFilterRequest { Page = 1, PageSize = 200 });

            var vm = new UploadDocumentViewModel
            {
                PropertyId = propertyId,
                PropertyNameAr = "العقار",
                Dto = new UploadDocumentDto { PropertyId = propertyId, DocumentTypeId = documentTypeId ?? 0 },
                PreselectedTypeId = documentTypeId,
                DocumentTypes = types.Select(t => new SelectListItem(t.NameAr, t.Id.ToString())).ToList(),
                Responsibles = users.Items.Select(u => new SelectListItem(u.FullNameAr, u.Id.ToString())).ToList()
            };

            return View("Upload", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(50_000_000)]
        [RequirePermission(PermissionKeys.Documents_Upload)]
        public async Task<IActionResult> Upload(UploadDocumentDto dto)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Upload), new { propertyId = dto.PropertyId, documentTypeId = dto.DocumentTypeId });
            }

            var id = await _documentService.UploadAsync(dto, CurrentUserId);
            TempData["SuccessMessage"] = "تم رفع الوثيقة بنجاح";
            return RedirectToAction(nameof(ForProperty), new { propertyId = dto.PropertyId, id });
        }

        [HttpGet]
        [RequirePermission(PermissionKeys.Documents_Upload)]
        public IActionResult UploadVersion(long documentId)
        {
            return View("UploadVersion", new UploadNewVersionDto { DocumentId = documentId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(50_000_000)]
        [RequirePermission(PermissionKeys.Documents_Upload)]
        public async Task<IActionResult> UploadVersion(UploadNewVersionDto dto)
        {
            await _documentService.UploadNewVersionAsync(dto, CurrentUserId);
            TempData["SuccessMessage"] = "تم رفع النسخة الجديدة";
            return RedirectToAction(nameof(Detail), new { id = dto.DocumentId });
        }

        [HttpPost]
        [RequirePermission(PermissionKeys.Documents_Verify)]
        public async Task<IActionResult> Verify([FromBody] VerifyDocumentDto dto)
        {
            try
            {
                await _documentService.VerifyAsync(dto, CurrentUserId);
                var detail = await _documentService.GetDetailAsync(dto.DocumentId, CurrentUserId);
                return Json(new { success = true, data = new { newStatus = detail?.Status.ToString(), statusDisplayAr = detail?.StatusDisplayAr }, message = "تم تحديث حالة الوثيقة", errors = Array.Empty<string>() });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, data = (object?)null, message = ex.Message, errors = new[] { ex.Message } });
            }
        }

        [HttpGet]
        [RequirePermission(PermissionKeys.Documents_View)]
        public async Task<IActionResult> Download(long versionId)
        {
            var details = await _documentService.SearchAsync(new DocumentFilterRequest { Page = 1, PageSize = 1 }, CurrentUserId);
            var doc = await _documentService.GetDetailAsync(details.Items.FirstOrDefault(x => x.CurrentVersionId == versionId)?.Id ?? 0, CurrentUserId);
            if (doc == null)
            {
                return NotFound();
            }

            await _documentService.RecordDownloadAsync(doc.Id, versionId, CurrentUserId);
            var version = doc.Versions.FirstOrDefault(v => v.Id == versionId) ?? doc.Versions.FirstOrDefault(v => v.IsCurrent);
            if (version == null)
            {
                return NotFound();
            }

            return Redirect(version.FileUrl);
        }

        [HttpPost]
        [RequirePermission(PermissionKeys.Documents_Upload)]
        public async Task<IActionResult> AssignResponsible(long documentId, int userId, string? notes)
        {
            try
            {
                await _documentService.AssignResponsibleAsync(documentId, userId, notes, CurrentUserId);
                return Json(new { success = true, data = new { userName = userId.ToString() }, message = "تم تعيين المسؤول", errors = Array.Empty<string>() });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, data = (object?)null, message = ex.Message, errors = new[] { ex.Message } });
            }
        }

        [HttpPost]
        [RequirePermission(PermissionKeys.Documents_Upload)]
        public async Task<IActionResult> RemoveResponsible(long documentId, int userId)
        {
            await _documentService.RemoveResponsibleAsync(documentId, userId, CurrentUserId);
            return Json(new { success = true, data = (object?)null, message = "تمت إزالة المسؤول", errors = Array.Empty<string>() });
        }

        [HttpPost]
        [RequirePermission(PermissionKeys.Documents_Upload)]
        public async Task<IActionResult> SoftDelete(long id, string reason)
        {
            await _documentService.SoftDeleteAsync(id, reason, CurrentUserId);
            return Json(new { success = true, data = (object?)null, message = "تم حذف الوثيقة", errors = Array.Empty<string>() });
        }

        [HttpPost]
        [RequirePermission(PermissionKeys.Documents_Upload)]
        public async Task<IActionResult> Restore(long id)
        {
            await _documentService.RestoreAsync(id, CurrentUserId);
            return Json(new { success = true, data = (object?)null, message = "تمت الاستعادة", errors = Array.Empty<string>() });
        }

        [HttpGet]
        [RequirePermission(PermissionKeys.Documents_View)]
        public async Task<IActionResult> Alerts([FromQuery] DocumentAlertFilterRequest filter)
        {
            var alerts = await _documentService.GetAlertsAsync(filter, CurrentUserId);
            var unreadCount = await _documentAlertService.GetUnreadAlertCountAsync(CurrentUserId);
            var vm = new AlertsViewModel
            {
                Alerts = alerts,
                Filter = filter,
                UnreadCount = unreadCount,
                ExpiredCount = alerts.Items.Count(x => x.AlertLevel == Core.Entities.DocumentAlertLevel.Expired),
                Expiring30Count = alerts.Items.Count(x => x.AlertLevel == Core.Entities.DocumentAlertLevel.Day30),
                Expiring90Count = alerts.Items.Count(x => x.AlertLevel == Core.Entities.DocumentAlertLevel.Day90)
            };
            return View("Alerts", vm);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAlertRead(long alertId)
        {
            await _documentService.MarkAlertReadAsync(alertId, CurrentUserId);
            return Json(new { success = true, data = (object?)null, message = "تم تعليم التنبيه كمقروء", errors = Array.Empty<string>() });
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllRead(long? propertyId)
        {
            await _documentService.MarkAllAlertsReadAsync(propertyId, CurrentUserId);
            return Json(new { success = true, data = new { count = 0 }, message = "تم تعليم الكل كمقروء", errors = Array.Empty<string>() });
        }

        [HttpPost]
        public async Task<IActionResult> RunExpiryCheck()
        {
            var count = await _documentAlertService.ProcessAllExpiryChecksAsync();
            return Json(new { success = true, data = new { newAlertsCount = count }, message = "تم الفحص", errors = Array.Empty<string>() });
        }

        [HttpGet]
        [RequirePermission(PermissionKeys.Documents_View)]
        public async Task<IActionResult> AuditTrail(long documentId)
        {
            var trail = await _documentService.GetAuditTrailAsync(documentId);
            return PartialView("_AuditTrail", trail);
        }

        [HttpGet]
        public async Task<IActionResult> GetDocumentTypes()
        {
            var types = await _documentService.GetDocumentTypesAsync();
            return Json(new { success = true, data = types, message = "", errors = Array.Empty<string>() });
        }

        [HttpGet]
        public async Task<IActionResult> AlertBadgeCount()
        {
            var count = await _documentAlertService.GetUnreadAlertCountAsync(CurrentUserId);
            return Json(new { count });
        }
    }
}
