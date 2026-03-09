using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using WaqfGIS.Core.Entities;
using WaqfGIS.Core.Interfaces;
using WaqfGIS.Services;
using WaqfGIS.Web.Models;

namespace WaqfGIS.Web.Controllers;

[Authorize]
public class EncroachementsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly PermissionService _permissionService;
    private readonly AuditLogService _auditLogService;
    private readonly ILogger<EncroachementsController> _logger;

    public EncroachementsController(
        IUnitOfWork unitOfWork,
        PermissionService permissionService,
        AuditLogService auditLogService,
        ILogger<EncroachementsController> logger)
    {
        _unitOfWork = unitOfWork;
        _permissionService = permissionService;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    // =================== INDEX ===================
    public async Task<IActionResult> Index(string? status, string? entityType, int? provinceId, string? search)
    {
        var query = _unitOfWork.Repository<EncroachmentRecord>().Query()
            .Include(e => e.Province)
            .AsQueryable();

        // تصفية حسب صلاحية المستخدم
        var user = await _permissionService.GetCurrentUserAsync(User);
        if (user?.ProvinceId != null)
            query = query.Where(e => e.ProvinceId == user.ProvinceId);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(e => e.Status == status);
        if (!string.IsNullOrEmpty(entityType))
            query = query.Where(e => e.EntityType == entityType);
        if (provinceId.HasValue)
            query = query.Where(e => e.ProvinceId == provinceId.Value);
        if (!string.IsNullOrEmpty(search))
            query = query.Where(e => e.EntityName!.Contains(search) || e.Description.Contains(search) || e.EncroachwrName!.Contains(search));

        var records = await query.OrderByDescending(e => e.DiscoveryDate).ToListAsync();

        // إحصاءات
        ViewBag.TotalCount = records.Count;
        ViewBag.ActiveCount = records.Count(r => r.Status == "قائم");
        ViewBag.InProcessCount = records.Count(r => r.Status == "قيد المعالجة");
        ViewBag.RemovedCount = records.Count(r => r.Status == "أُزيل");
        ViewBag.LegalCount = records.Count(r => r.Status == "مرفوع للقضاء");

        // GeoJSON للخريطة
        var geoFeatures = records
            .Where(r => r.Location != null)
            .Select(r => new {
                type = "Feature",
                geometry = new { type = "Point", coordinates = new[] { r.Location!.X, r.Location.Y } },
                properties = new {
                    id = r.Id,
                    entityName = r.EntityName,
                    encroachmentType = r.EncroachmentType,
                    status = r.Status,
                    severity = r.Severity,
                    discoveryDate = r.DiscoveryDate.ToString("dd/MM/yyyy"),
                    description = r.Description
                }
            }).ToList();
        ViewBag.GeoJson = System.Text.Json.JsonSerializer.Serialize(new { type = "FeatureCollection", features = geoFeatures });

        await LoadViewDataAsync();
        ViewBag.CurrentStatus = status;
        ViewBag.CurrentEntityType = entityType;
        ViewBag.CurrentSearch = search;
        ViewBag.CanEdit = await _permissionService.CanEditAsync(User);

        return View(records);
    }

    // =================== DETAILS ===================
    public async Task<IActionResult> Details(int id)
    {
        var record = await _unitOfWork.Repository<EncroachmentRecord>().Query()
            .Include(e => e.Province)
            .Include(e => e.Photos)
            .Include(e => e.LegalDispute)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (record == null) return NotFound();

        ViewBag.CanEdit = await _permissionService.CanEditAsync(User);
        return View(record);
    }

    // =================== CREATE GET ===================
    public async Task<IActionResult> Create(string? entityType, int? entityId)
    {
        if (!await _permissionService.CanCreateAsync(User))
            return Forbid();

        var model = new EncroachmentViewModel
        {
            EntityType = entityType ?? "WaqfProperty",
            EntityId = entityId ?? 0,
            DiscoveryDate = DateTime.Today
        };

        // جلب اسم العنصر إذا تم تحديده
        if (entityId.HasValue && entityId.Value > 0)
        {
            model.EntityName = await GetEntityNameAsync(entityType ?? "WaqfProperty", entityId.Value);
            // جلب إحداثيات العنصر كنقطة بداية للخريطة
            var coords = await GetEntityCoordsAsync(entityType ?? "WaqfProperty", entityId.Value);
            if (coords.HasValue)
            {
                model.Latitude = coords.Value.lat;
                model.Longitude = coords.Value.lng;
            }
        }

        await LoadViewDataAsync();
        return View(model);
    }

    // =================== CREATE POST ===================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EncroachmentViewModel model, IFormFileCollection photos)
    {
        if (!await _permissionService.CanCreateAsync(User))
            return Forbid();

        if (!ModelState.IsValid)
        {
            await LoadViewDataAsync();
            return View(model);
        }

        var record = new EncroachmentRecord
        {
            EntityType = model.EntityType,
            EntityId = model.EntityId,
            EntityName = model.EntityName,
            ProvinceId = model.ProvinceId,
            EncroachmentType = model.EncroachmentType,
            Severity = model.Severity,
            Description = model.Description,
            EncroachmentAreaSqm = model.EncroachmentAreaSqm,
            EncroachwrName = model.EncroachwrName,
            EncroachwrPhone = model.EncroachwrPhone,
            EncroachwrNationalId = model.EncroachwrNationalId,
            LocationDescription = model.LocationDescription,
            DiscoveryDate = model.DiscoveryDate,
            ReportDate = model.ReportDate,
            RemovalDate = model.RemovalDate,
            Status = model.Status,
            ActionTaken = model.ActionTaken,
            IsReportedToAuthorities = model.IsReportedToAuthorities,
            ReportReferenceNumber = model.ReportReferenceNumber,
            AuthorityReportDate = model.AuthorityReportDate,
            HasLegalCase = model.HasLegalCase,
            LegalDisputeId = model.HasLegalCase ? model.LegalDisputeId : null,
            Notes = model.Notes,
            CreatedBy = User.Identity?.Name
        };

        // حفظ الإحداثيات إذا وجدت
        if (model.Latitude.HasValue && model.Longitude.HasValue &&
            model.Latitude.Value != 0 && model.Longitude.Value != 0)
        {
            record.Location = new Point(model.Longitude.Value, model.Latitude.Value) { SRID = 4326 };
        }

        await _unitOfWork.Repository<EncroachmentRecord>().AddAsync(record);
        await _unitOfWork.SaveChangesAsync();

        // رفع الصور
        if (photos != null && photos.Count > 0)
            await UploadPhotosAsync(record.Id, photos);

        await _auditLogService.LogCreateAsync("EncroachmentRecord", record.Id,
            $"تجاوز على {record.EntityName}",
            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
            User.Identity?.Name, HttpContext.Connection.RemoteIpAddress?.ToString());

        TempData["Success"] = "تم تسجيل التجاوز بنجاح";

        // العودة لصفحة العنصر الأصلي إذا جاء منه
        if (!string.IsNullOrEmpty(model.EntityType) && model.EntityId > 0)
        {
            var controller = model.EntityType switch {
                "WaqfProperty" => "Properties",
                "WaqfLand" => "WaqfLands",
                "Mosque" => "Mosques",
                _ => null
            };
            if (controller != null)
                return RedirectToAction("Details", controller, new { id = model.EntityId });
        }

        return RedirectToAction(nameof(Index));
    }

    // =================== EDIT GET ===================
    public async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.CanEditAsync(User))
            return Forbid();

        var record = await _unitOfWork.Repository<EncroachmentRecord>().Query()
            .Include(e => e.Province)
            .Include(e => e.Photos)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (record == null) return NotFound();

        var model = MapToViewModel(record);
        await LoadViewDataAsync();
        ViewBag.Photos = record.Photos.ToList();
        return View(model);
    }

    // =================== EDIT POST ===================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EncroachmentViewModel model, IFormFileCollection photos)
    {
        if (id != model.Id) return NotFound();
        if (!await _permissionService.CanEditAsync(User))
            return Forbid();

        var record = await _unitOfWork.Repository<EncroachmentRecord>().Query()
            .Include(e => e.Photos)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (record == null) return NotFound();

        if (!ModelState.IsValid)
        {
            await LoadViewDataAsync();
            ViewBag.Photos = record.Photos.ToList();
            return View(model);
        }

        record.EntityType = model.EntityType;
        record.EntityId = model.EntityId;
        record.EntityName = model.EntityName;
        record.ProvinceId = model.ProvinceId;
        record.EncroachmentType = model.EncroachmentType;
        record.Severity = model.Severity;
        record.Description = model.Description;
        record.EncroachmentAreaSqm = model.EncroachmentAreaSqm;
        record.EncroachwrName = model.EncroachwrName;
        record.EncroachwrPhone = model.EncroachwrPhone;
        record.EncroachwrNationalId = model.EncroachwrNationalId;
        record.LocationDescription = model.LocationDescription;
        record.DiscoveryDate = model.DiscoveryDate;
        record.ReportDate = model.ReportDate;
        record.RemovalDate = model.RemovalDate;
        record.Status = model.Status;
        record.ActionTaken = model.ActionTaken;
        record.IsReportedToAuthorities = model.IsReportedToAuthorities;
        record.ReportReferenceNumber = model.ReportReferenceNumber;
        record.AuthorityReportDate = model.AuthorityReportDate;
        record.HasLegalCase = model.HasLegalCase;
        record.LegalDisputeId = model.HasLegalCase ? model.LegalDisputeId : null;
        record.Notes = model.Notes;
        record.UpdatedBy = User.Identity?.Name;

        if (model.Latitude.HasValue && model.Longitude.HasValue &&
            model.Latitude.Value != 0 && model.Longitude.Value != 0)
        {
            record.Location = new Point(model.Longitude.Value, model.Latitude.Value) { SRID = 4326 };
        }

        await _unitOfWork.Repository<EncroachmentRecord>().UpdateAsync(record);
        await _unitOfWork.SaveChangesAsync();

        if (photos != null && photos.Count > 0)
            await UploadPhotosAsync(record.Id, photos);

        TempData["Success"] = "تم تحديث سجل التجاوز بنجاح";
        return RedirectToAction(nameof(Details), new { id });
    }

    // =================== DELETE ===================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.CanDeleteAsync(User))
        {
            TempData["Error"] = "ليس لديك صلاحية الحذف";
            return RedirectToAction(nameof(Index));
        }

        var record = await _unitOfWork.Repository<EncroachmentRecord>().GetByIdAsync(id);
        if (record != null)
        {
            await _unitOfWork.Repository<EncroachmentRecord>().DeleteAsync(record);
            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = "تم حذف سجل التجاوز";
        }
        return RedirectToAction(nameof(Index));
    }

    // =================== DELETE PHOTO ===================
    [HttpPost]
    public async Task<IActionResult> DeletePhoto(int photoId, int encroachmentId)
    {
        var photo = await _unitOfWork.Repository<EncroachmentPhoto>().GetByIdAsync(photoId);
        if (photo != null && photo.EncroachmentId == encroachmentId)
        {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", photo.FilePath.TrimStart('/'));
            if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
            await _unitOfWork.Repository<EncroachmentPhoto>().DeleteAsync(photo);
            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = "تم حذف الصورة";
        }
        return RedirectToAction(nameof(Edit), new { id = encroachmentId });
    }

    // =================== API - للخريطة الرئيسية ===================
    [HttpGet]
    public async Task<IActionResult> GetGeoJson(int? provinceId)
    {
        var query = _unitOfWork.Repository<EncroachmentRecord>().Query()
            .Where(e => e.Location != null);

        var user = await _permissionService.GetCurrentUserAsync(User);
        if (user?.ProvinceId != null)
            query = query.Where(e => e.ProvinceId == user.ProvinceId);
        if (provinceId.HasValue)
            query = query.Where(e => e.ProvinceId == provinceId.Value);

        var records = await query.ToListAsync();

        var features = records.Select(r => new {
            type = "Feature",
            geometry = new { type = "Point", coordinates = new[] { r.Location!.X, r.Location.Y } },
            properties = new {
                id = r.Id,
                entityName = r.EntityName,
                entityType = r.EntityType,
                encroachmentType = r.EncroachmentType,
                status = r.Status,
                severity = r.Severity,
                discoveryDate = r.DiscoveryDate.ToString("dd/MM/yyyy"),
                description = r.Description,
                detailsUrl = Url.Action("Details", "Encroachements", new { id = r.Id })
            }
        });

        return Json(new { type = "FeatureCollection", features });
    }

    // =================== HELPERS ===================
    private EncroachmentViewModel MapToViewModel(EncroachmentRecord record) => new()
    {
        Id = record.Id,
        EntityType = record.EntityType,
        EntityId = record.EntityId,
        EntityName = record.EntityName,
        ProvinceId = record.ProvinceId,
        ProvinceName = record.Province?.NameAr,
        EncroachmentType = record.EncroachmentType,
        Severity = record.Severity,
        Description = record.Description,
        EncroachmentAreaSqm = record.EncroachmentAreaSqm,
        EncroachwrName = record.EncroachwrName,
        EncroachwrPhone = record.EncroachwrPhone,
        EncroachwrNationalId = record.EncroachwrNationalId,
        Latitude = record.Location?.Y,
        Longitude = record.Location?.X,
        LocationDescription = record.LocationDescription,
        DiscoveryDate = record.DiscoveryDate,
        ReportDate = record.ReportDate,
        RemovalDate = record.RemovalDate,
        Status = record.Status,
        ActionTaken = record.ActionTaken,
        IsReportedToAuthorities = record.IsReportedToAuthorities,
        ReportReferenceNumber = record.ReportReferenceNumber,
        AuthorityReportDate = record.AuthorityReportDate,
        HasLegalCase = record.HasLegalCase,
        LegalDisputeId = record.LegalDisputeId,
        Notes = record.Notes
    };

    private async Task UploadPhotosAsync(int encroachmentId, IFormFileCollection files)
    {
        var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "encroachments", encroachmentId.ToString());
        Directory.CreateDirectory(uploadDir);

        foreach (var file in files)
        {
            if (file.Length == 0) continue;
            var ext = Path.GetExtension(file.FileName);
            var savedName = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(uploadDir, savedName);
            using var stream = System.IO.File.Create(fullPath);
            await file.CopyToAsync(stream);

            var photo = new EncroachmentPhoto
            {
                EncroachmentId = encroachmentId,
                FileName = file.FileName,
                FilePath = $"/uploads/encroachments/{encroachmentId}/{savedName}",
                FileSize = file.Length,
                MimeType = file.ContentType,
                CreatedBy = User.Identity?.Name
            };
            await _unitOfWork.Repository<EncroachmentPhoto>().AddAsync(photo);
        }
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<string?> GetEntityNameAsync(string entityType, int entityId)
    {
        return entityType switch {
            "WaqfProperty" => (await _unitOfWork.Repository<WaqfProperty>().GetByIdAsync(entityId))?.NameAr,
            "WaqfLand" => (await _unitOfWork.Repository<WaqfLand>().GetByIdAsync(entityId))?.NameAr,
            "Mosque" => (await _unitOfWork.Repository<Mosque>().GetByIdAsync(entityId))?.NameAr,
            _ => null
        };
    }

    private async Task<(double lat, double lng)?> GetEntityCoordsAsync(string entityType, int entityId)
    {
        Point? loc = null;
        if (entityType == "WaqfProperty")
        {
            var e = await _unitOfWork.Repository<WaqfProperty>().GetByIdAsync(entityId);
            loc = e?.Location;
        }
        else if (entityType == "Mosque")
        {
            var e = await _unitOfWork.Repository<Mosque>().GetByIdAsync(entityId);
            loc = e?.Location;
        }
        else if (entityType == "WaqfLand")
        {
            var e = await _unitOfWork.Repository<WaqfLand>().GetByIdAsync(entityId);
            loc = e?.CenterPoint;
        }
        if (loc == null) return null;
        return (loc.Y, loc.X);
    }

    private async Task LoadViewDataAsync()
    {
        var provinces = await _permissionService.GetAuthorizedProvincesAsync(User);
        ViewBag.Provinces = new SelectList(provinces, "Id", "NameAr");
        ViewBag.EncroachmentTypes = EncroachmentViewModel.EncroachmentTypesList;
        ViewBag.SeverityList = EncroachmentViewModel.SeverityList;
        ViewBag.StatusList = EncroachmentViewModel.StatusList;
    }
}
