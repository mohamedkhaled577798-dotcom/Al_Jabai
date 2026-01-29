using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using WaqfGIS.Core.Entities;
using WaqfGIS.Core.Enums;
using WaqfGIS.Core.Interfaces;
using WaqfGIS.Services;
using WaqfGIS.Web.Models;

namespace WaqfGIS.Web.Controllers;

[Authorize]
public class MosquesController : Controller
{
    private readonly MosqueService _mosqueService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ExcelExportService _excelExportService;
    private readonly AuditLogService _auditLogService;
    private readonly ImageUploadService _imageUploadService;
    private readonly PermissionService _permissionService;

    public MosquesController(
        MosqueService mosqueService, 
        IUnitOfWork unitOfWork, 
        ExcelExportService excelExportService,
        AuditLogService auditLogService,
        ImageUploadService imageUploadService,
        PermissionService permissionService)
    {
        _mosqueService = mosqueService;
        _unitOfWork = unitOfWork;
        _excelExportService = excelExportService;
        _auditLogService = auditLogService;
        _imageUploadService = imageUploadService;
        _permissionService = permissionService;
    }

    public async Task<IActionResult> Index(int? provinceId, int? typeId, string? search)
    {
        // الحصول على المساجد حسب صلاحيات المستخدم
        var query = await _permissionService.GetAuthorizedMosquesAsync(User);
        
        var mosques = await query
            .Include(m => m.MosqueType)
            .Include(m => m.MosqueStatus)
            .Include(m => m.Province)
            .Include(m => m.WaqfOffice)
            .ToListAsync();

        if (provinceId.HasValue)
            mosques = mosques.Where(m => m.ProvinceId == provinceId.Value).ToList();
        if (typeId.HasValue)
            mosques = mosques.Where(m => m.MosqueTypeId == typeId.Value).ToList();
        if (!string.IsNullOrEmpty(search))
            mosques = mosques.Where(m => m.NameAr.Contains(search) || m.Code.Contains(search)).ToList();

        await LoadViewDataAsync();
        ViewBag.CurrentProvinceId = provinceId;
        ViewBag.CurrentTypeId = typeId;
        ViewBag.CurrentSearch = search;
        ViewBag.CanEdit = await _permissionService.CanEditAsync(User);
        ViewBag.CanDelete = await _permissionService.CanDeleteAsync(User);

        return View(mosques);
    }

    public async Task<IActionResult> Details(int id)
    {
        var mosque = await _mosqueService.GetByIdAsync(id);
        if (mosque == null) return NotFound();

        // التحقق من الصلاحية
        if (!await _permissionService.CanAccessMosqueAsync(User, mosque))
            return Forbid();

        ViewBag.Images = await _imageUploadService.GetMosqueImagesAsync(id);
        ViewBag.AuditLogs = await _auditLogService.GetLogsAsync("Mosque", id);
        ViewBag.CanEdit = await _permissionService.CanEditAsync(User);
        return View(mosque);
    }

    public async Task<IActionResult> Create()
    {
        if (!await _permissionService.CanCreateAsync(User))
            return Forbid();

        await LoadViewDataAsync();
        return View(new MosqueViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MosqueViewModel model, IFormFileCollection images)
    {
        if (!await _permissionService.CanCreateAsync(User))
            return Forbid();

        // التحقق من صلاحية المحافظة أو الدائرة
        var user = await _permissionService.GetCurrentUserAsync(User);
        if (user != null && user.PermissionLevel == PermissionLevel.ProvinceLevel && user.ProvinceId != model.ProvinceId)
        {
            ModelState.AddModelError("ProvinceId", "لا يمكنك إضافة مسجد في محافظة أخرى");
        }
        if (user != null && user.PermissionLevel == PermissionLevel.OfficeLevel && user.WaqfOfficeId != model.WaqfOfficeId)
        {
            ModelState.AddModelError("WaqfOfficeId", "لا يمكنك إضافة مسجد في دائرة أخرى");
        }

        if (!ModelState.IsValid)
        {
            await LoadViewDataAsync();
            return View(model);
        }

        var mosque = new Mosque
        {
            NameAr = model.NameAr,
            NameEn = model.NameEn,
            WaqfOfficeId = model.WaqfOfficeId,
            MosqueTypeId = model.MosqueTypeId,
            MosqueStatusId = model.MosqueStatusId,
            ProvinceId = model.ProvinceId,
            DistrictId = model.DistrictId,
            Location = new Point(model.Longitude, model.Latitude) { SRID = 4326 },
            Address = model.Address,
            Neighborhood = model.Neighborhood,
            NearestLandmark = model.NearestLandmark,
            Capacity = model.Capacity,
            AreaSqm = model.AreaSqm,
            HasFridayPrayer = model.HasFridayPrayer,
            HasMinaret = model.HasMinaret,
            HasDome = model.HasDome,
            HasParking = model.HasParking,
            HasWomenSection = model.HasWomenSection,
            ImamName = model.ImamName,
            ImamPhone = model.ImamPhone,
            EstablishedYear = model.EstablishedYear,
            Notes = model.Notes,
            CreatedBy = User.Identity?.Name
        };

        await _mosqueService.CreateAsync(mosque);

        await _auditLogService.LogCreateAsync("Mosque", mosque.Id, mosque.NameAr,
            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
            User.Identity?.Name, HttpContext.Connection.RemoteIpAddress?.ToString());

        if (images != null && images.Count > 0)
            await _imageUploadService.UploadMosqueImagesAsync(mosque.Id, images, User.Identity?.Name);

        TempData["Success"] = "تم إضافة المسجد بنجاح";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.CanEditAsync(User))
            return Forbid();

        var mosque = await _mosqueService.GetByIdAsync(id);
        if (mosque == null) return NotFound();

        if (!await _permissionService.CanAccessMosqueAsync(User, mosque))
            return Forbid();

        var model = new MosqueViewModel
        {
            Id = mosque.Id,
            NameAr = mosque.NameAr,
            NameEn = mosque.NameEn,
            WaqfOfficeId = mosque.WaqfOfficeId,
            MosqueTypeId = mosque.MosqueTypeId,
            MosqueStatusId = mosque.MosqueStatusId,
            ProvinceId = mosque.ProvinceId,
            DistrictId = mosque.DistrictId,
            Latitude = mosque.Location.Y,
            Longitude = mosque.Location.X,
            Address = mosque.Address,
            Neighborhood = mosque.Neighborhood,
            NearestLandmark = mosque.NearestLandmark,
            Capacity = mosque.Capacity,
            AreaSqm = mosque.AreaSqm,
            HasFridayPrayer = mosque.HasFridayPrayer,
            HasMinaret = mosque.HasMinaret,
            HasDome = mosque.HasDome,
            HasParking = mosque.HasParking,
            HasWomenSection = mosque.HasWomenSection,
            ImamName = mosque.ImamName,
            ImamPhone = mosque.ImamPhone,
            EstablishedYear = mosque.EstablishedYear,
            Notes = mosque.Notes
        };

        ViewBag.Images = await _imageUploadService.GetMosqueImagesAsync(id);
        await LoadViewDataAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MosqueViewModel model, IFormFileCollection images)
    {
        if (id != model.Id) return NotFound();

        if (!await _permissionService.CanEditAsync(User))
            return Forbid();

        var mosque = await _mosqueService.GetByIdAsync(id);
        if (mosque == null) return NotFound();

        if (!await _permissionService.CanAccessMosqueAsync(User, mosque))
            return Forbid();

        if (!ModelState.IsValid)
        {
            ViewBag.Images = await _imageUploadService.GetMosqueImagesAsync(id);
            await LoadViewDataAsync();
            return View(model);
        }

        var oldValues = $"الاسم: {mosque.NameAr}, السعة: {mosque.Capacity}";

        mosque.NameAr = model.NameAr;
        mosque.NameEn = model.NameEn;
        mosque.WaqfOfficeId = model.WaqfOfficeId;
        mosque.MosqueTypeId = model.MosqueTypeId;
        mosque.MosqueStatusId = model.MosqueStatusId;
        mosque.ProvinceId = model.ProvinceId;
        mosque.DistrictId = model.DistrictId;
        mosque.Location = new Point(model.Longitude, model.Latitude) { SRID = 4326 };
        mosque.Address = model.Address;
        mosque.Neighborhood = model.Neighborhood;
        mosque.Capacity = model.Capacity;
        mosque.HasFridayPrayer = model.HasFridayPrayer;
        mosque.ImamName = model.ImamName;
        mosque.ImamPhone = model.ImamPhone;
        mosque.Notes = model.Notes;
        mosque.UpdatedBy = User.Identity?.Name;

        await _mosqueService.UpdateAsync(mosque);

        var newValues = $"الاسم: {mosque.NameAr}, السعة: {mosque.Capacity}";
        await _auditLogService.LogUpdateAsync("Mosque", mosque.Id, mosque.NameAr,
            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
            User.Identity?.Name, oldValues, newValues, HttpContext.Connection.RemoteIpAddress?.ToString());

        if (images != null && images.Count > 0)
            await _imageUploadService.UploadMosqueImagesAsync(mosque.Id, images, User.Identity?.Name);

        TempData["Success"] = "تم تحديث بيانات المسجد بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.CanDeleteAsync(User))
        {
            TempData["Error"] = "ليس لديك صلاحية الحذف";
            return RedirectToAction(nameof(Index));
        }

        var mosque = await _mosqueService.GetByIdAsync(id);
        if (mosque != null)
        {
            if (!await _permissionService.CanAccessMosqueAsync(User, mosque))
                return Forbid();

            await _mosqueService.DeleteAsync(id);
            
            await _auditLogService.LogDeleteAsync("Mosque", id, mosque.NameAr,
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                User.Identity?.Name, HttpContext.Connection.RemoteIpAddress?.ToString());
        }
        
        TempData["Success"] = "تم حذف المسجد بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Export(int? provinceId, int? typeId)
    {
        var query = await _permissionService.GetAuthorizedMosquesAsync(User);
        var mosques = await query
            .Include(m => m.MosqueType).Include(m => m.MosqueStatus)
            .Include(m => m.Province).Include(m => m.WaqfOffice)
            .ToListAsync();

        if (provinceId.HasValue)
            mosques = mosques.Where(m => m.ProvinceId == provinceId.Value).ToList();
        if (typeId.HasValue)
            mosques = mosques.Where(m => m.MosqueTypeId == typeId.Value).ToList();

        var fileContent = _excelExportService.ExportMosquesToExcel(mosques);
        var fileName = $"المساجد_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        
        return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    [HttpPost]
    public async Task<IActionResult> UploadImages(int mosqueId, IFormFileCollection images)
    {
        if (!await _permissionService.CanEditAsync(User))
            return Forbid();

        if (images != null && images.Count > 0)
        {
            await _imageUploadService.UploadMosqueImagesAsync(mosqueId, images, User.Identity?.Name);
            TempData["Success"] = $"تم رفع {images.Count} صورة بنجاح";
        }
        return RedirectToAction(nameof(Edit), new { id = mosqueId });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteImage(int imageId, int mosqueId)
    {
        if (!await _permissionService.CanEditAsync(User))
            return Forbid();

        await _imageUploadService.DeleteMosqueImageAsync(imageId);
        TempData["Success"] = "تم حذف الصورة بنجاح";
        return RedirectToAction(nameof(Edit), new { id = mosqueId });
    }

    [HttpPost]
    public async Task<IActionResult> SetMainImage(int imageId, int mosqueId)
    {
        if (!await _permissionService.CanEditAsync(User))
            return Forbid();

        await _imageUploadService.SetMainMosqueImageAsync(mosqueId, imageId);
        TempData["Success"] = "تم تعيين الصورة الرئيسية";
        return RedirectToAction(nameof(Edit), new { id = mosqueId });
    }

    private async Task LoadViewDataAsync()
    {
        var user = await _permissionService.GetCurrentUserAsync(User);
        
        // تحميل المحافظات حسب الصلاحية
        var provinces = await _permissionService.GetAuthorizedProvincesAsync(User);
        ViewBag.Provinces = new SelectList(provinces, "Id", "NameAr");

        // تحميل الدوائر حسب الصلاحية
        var offices = await _permissionService.GetAuthorizedOfficesAsync(User);
        ViewBag.WaqfOffices = new SelectList(await offices.ToListAsync(), "Id", "NameAr");

        ViewBag.MosqueTypes = new SelectList(await _unitOfWork.MosqueTypes.GetAllAsync(), "Id", "NameAr");
        ViewBag.MosqueStatuses = new SelectList(await _unitOfWork.MosqueStatuses.GetAllAsync(), "Id", "NameAr");
        ViewBag.Districts = new SelectList(await _unitOfWork.Districts.GetAllAsync(), "Id", "NameAr");

        // معلومات الصلاحية للـ View
        ViewBag.UserPermissionLevel = user?.PermissionLevel;
        ViewBag.UserProvinceId = user?.ProvinceId;
        ViewBag.UserOfficeId = user?.WaqfOfficeId;
    }
}
