using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using WaqfGIS.Core.Entities;
using WaqfGIS.Core.Enums;
using WaqfGIS.Core.Interfaces;
using WaqfGIS.Services;
using WaqfGIS.Services.GIS;
using WaqfGIS.Web.Models;

namespace WaqfGIS.Web.Controllers;

[Authorize]
public class PropertiesController : Controller
{
    private readonly PropertyService _propertyService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ExcelExportService _excelExportService;
    private readonly AuditLogService _auditLogService;
    private readonly ImageUploadService _imageUploadService;
    private readonly PermissionService _permissionService;
    private readonly GeometryService _geometryService;
    private readonly ILogger<PropertiesController> _logger;

    public PropertiesController(
        PropertyService propertyService, 
        IUnitOfWork unitOfWork,
        ExcelExportService excelExportService,
        AuditLogService auditLogService,
        ImageUploadService imageUploadService,
        PermissionService permissionService,
        GeometryService geometryService,
        ILogger<PropertiesController> logger)
    {
        _propertyService = propertyService;
        _unitOfWork = unitOfWork;
        _excelExportService = excelExportService;
        _auditLogService = auditLogService;
        _imageUploadService = imageUploadService;
        _permissionService = permissionService;
        _geometryService = geometryService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(int? provinceId, int? typeId, string? search)
    {
        var query = await _permissionService.GetAuthorizedPropertiesAsync(User);
        
        var properties = await query
            .Include(p => p.PropertyType)
            .Include(p => p.UsageType)
            .Include(p => p.Province)
            .Include(p => p.WaqfOffice)
            .ToListAsync();

        if (provinceId.HasValue)
            properties = properties.Where(p => p.ProvinceId == provinceId.Value).ToList();
        if (typeId.HasValue)
            properties = properties.Where(p => p.PropertyTypeId == typeId.Value).ToList();
        if (!string.IsNullOrEmpty(search))
            properties = properties.Where(p => p.NameAr.Contains(search) || p.Code.Contains(search)).ToList();

        await LoadViewDataAsync();
        ViewBag.CurrentSearch = search;
        ViewBag.CanEdit = await _permissionService.CanEditAsync(User);
        ViewBag.CanDelete = await _permissionService.CanDeleteAsync(User);

        return View(properties);
    }

    public async Task<IActionResult> Details(int id)
    {
        var property = await _propertyService.GetByIdAsync(id);
        if (property == null) return NotFound();

        if (!await _permissionService.CanAccessPropertyAsync(User, property))
            return Forbid();
        
        ViewBag.Images = await _imageUploadService.GetPropertyImagesAsync(id);
        ViewBag.RegistrationFiles = await _unitOfWork.Repository<PropertyRegistrationFile>().Query()
            .Where(f => f.PropertyId == id).OrderByDescending(f => f.UploadedAt).ToListAsync();
        ViewBag.AuditLogs = await _auditLogService.GetLogsAsync("Property", id);
        ViewBag.CanEdit = await _permissionService.CanEditAsync(User);
        
        // Load boundary
        var boundary = await _unitOfWork.Repository<PropertyBoundary>().Query()
            .FirstOrDefaultAsync(b => b.PropertyId == id && b.BoundaryType == "Building");
        ViewBag.BoundaryGeoJson = boundary != null ? _geometryService.ToGeoJson(boundary.Boundary) : null;
        ViewBag.BoundaryArea = boundary?.CalculatedAreaSqm;
        
        return View(property);
    }

    public async Task<IActionResult> Create()
    {
        if (!await _permissionService.CanCreateAsync(User))
            return Forbid();

        await LoadViewDataAsync();
        return View(new PropertyViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PropertyViewModel model, IFormFileCollection images)
    {
        if (!await _permissionService.CanCreateAsync(User))
            return Forbid();

        var user = await _permissionService.GetCurrentUserAsync(User);
        if (user != null && user.PermissionLevel == PermissionLevel.ProvinceLevel && user.ProvinceId != model.ProvinceId)
            ModelState.AddModelError("ProvinceId", "لا يمكنك إضافة عقار في محافظة أخرى");
        if (user != null && user.PermissionLevel == PermissionLevel.OfficeLevel && user.WaqfOfficeId != model.WaqfOfficeId)
            ModelState.AddModelError("WaqfOfficeId", "لا يمكنك إضافة عقار في دائرة أخرى");

        if (!ModelState.IsValid)
        {
            await LoadViewDataAsync();
            return View(model);
        }

        var property = new WaqfProperty
        {
            NameAr = model.NameAr,
            NameEn = model.NameEn,
            WaqfOfficeId = model.WaqfOfficeId,
            PropertyTypeId = model.PropertyTypeId,
            UsageTypeId = model.UsageTypeId,
            ProvinceId = model.ProvinceId,
            DistrictId = model.DistrictId,
            Location = new Point(model.Longitude, model.Latitude) { SRID = 4326 },
            Address = model.Address,
            Neighborhood = model.Neighborhood,
            AreaSqm = model.AreaSqm,
            BuiltAreaSqm = model.BuiltAreaSqm,
            FloorsCount = model.FloorsCount,
            RoomsCount = model.RoomsCount,
            EstimatedValue = model.EstimatedValue,
            DeedNumber = model.DeedNumber,
            OwnershipType = model.OwnershipType,
            RentalStatus = model.RentalStatus,
            MonthlyRent = model.MonthlyRent,
            TenantName = model.TenantName,
            TenantPhone = model.TenantPhone,
            ConditionStatus = model.ConditionStatus,
            Notes = model.Notes,
            WaqfNature = model.WaqfNature,
            IsAdminReceived = model.WaqfNature == "Ahli" ? model.IsAdminReceived : null,
            WaqfCondition = model.WaqfCondition,
            WaqifName = model.WaqifName,
            WaqfDocumentDate = model.WaqfDocumentDate,
            WaqfDocumentNumber = model.WaqfDocumentNumber,
            WaqfConditionText = model.WaqfCondition == "WithCondition" ? model.WaqfConditionText : null,
            HasEncroachment = model.HasEncroachment,
            EncroachmentNotes = model.EncroachmentNotes,
            OccupantEmployeeName = model.OccupantEmployeeName,
            IsOccupantRetired = model.OccupantEmployeeName != null ? model.IsOccupantRetired : null,
            CreatedBy = User.Identity?.Name
        };

        await _propertyService.CreateAsync(property);

        await _auditLogService.LogCreateAsync("Property", property.Id, property.NameAr,
            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
            User.Identity?.Name, HttpContext.Connection.RemoteIpAddress?.ToString());

        if (images != null && images.Count > 0)
            await _imageUploadService.UploadPropertyImagesAsync(property.Id, images, User.Identity?.Name);

        await UploadPropertyRegistrationFilesAsync(property.Id, Request.Form.Files, "regFiles");

        TempData["Success"] = "تم إضافة العقار بنجاح";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.CanEditAsync(User))
            return Forbid();

        var property = await _propertyService.GetByIdAsync(id);
        if (property == null) return NotFound();

        if (!await _permissionService.CanAccessPropertyAsync(User, property))
            return Forbid();

        var model = new PropertyViewModel
        {
            Id = property.Id,
            NameAr = property.NameAr,
            NameEn = property.NameEn,
            WaqfOfficeId = property.WaqfOfficeId,
            PropertyTypeId = property.PropertyTypeId,
            UsageTypeId = property.UsageTypeId,
            ProvinceId = property.ProvinceId,
            DistrictId = property.DistrictId,
            Latitude = property.Location.Y,
            Longitude = property.Location.X,
            Address = property.Address,
            Neighborhood = property.Neighborhood,
            AreaSqm = property.AreaSqm,
            BuiltAreaSqm = property.BuiltAreaSqm,
            FloorsCount = property.FloorsCount,
            RoomsCount = property.RoomsCount,
            EstimatedValue = property.EstimatedValue,
            DeedNumber = property.DeedNumber,
            OwnershipType = property.OwnershipType,
            RentalStatus = property.RentalStatus,
            MonthlyRent = property.MonthlyRent,
            TenantName = property.TenantName,
            TenantPhone = property.TenantPhone,
            ConditionStatus = property.ConditionStatus,
            Notes = property.Notes,
            WaqfNature = property.WaqfNature,
            IsAdminReceived = property.IsAdminReceived,
            WaqfCondition = property.WaqfCondition,
            WaqifName = property.WaqifName,
            WaqfDocumentDate = property.WaqfDocumentDate,
            WaqfDocumentNumber = property.WaqfDocumentNumber,
            WaqfConditionText = property.WaqfConditionText,
            HasEncroachment = property.HasEncroachment,
            EncroachmentNotes = property.EncroachmentNotes,
            OccupantEmployeeName = property.OccupantEmployeeName,
            IsOccupantRetired = property.IsOccupantRetired
        };

        ViewBag.Images = await _imageUploadService.GetPropertyImagesAsync(id);
        ViewBag.RegistrationFiles = await _unitOfWork.Repository<PropertyRegistrationFile>().Query()
            .Where(f => f.PropertyId == id).ToListAsync();
        
        // Load boundary if exists
        var boundary = await _unitOfWork.Repository<PropertyBoundary>().Query()
            .FirstOrDefaultAsync(b => b.PropertyId == id && b.BoundaryType == "Building");
        ViewBag.BoundaryGeoJson = boundary != null ? _geometryService.ToGeoJson(boundary.Boundary) : null;
        
        await LoadViewDataAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PropertyViewModel model, IFormFileCollection images, string? BoundaryGeoJson)
    {
        if (id != model.Id) return NotFound();

        if (!await _permissionService.CanEditAsync(User))
            return Forbid();

        var property = await _propertyService.GetByIdAsync(id);
        if (property == null) return NotFound();

        if (!await _permissionService.CanAccessPropertyAsync(User, property))
            return Forbid();

        if (!ModelState.IsValid)
        {
            ViewBag.Images = await _imageUploadService.GetPropertyImagesAsync(id);
            await LoadViewDataAsync();
            return View(model);
        }

        var oldValues = $"الاسم: {property.NameAr}, القيمة: {property.EstimatedValue}";

        property.NameAr = model.NameAr;
        property.NameEn = model.NameEn;
        property.WaqfOfficeId = model.WaqfOfficeId;
        property.PropertyTypeId = model.PropertyTypeId;
        property.UsageTypeId = model.UsageTypeId;
        property.ProvinceId = model.ProvinceId;
        property.DistrictId = model.DistrictId;
        property.Location = new Point(model.Longitude, model.Latitude) { SRID = 4326 };
        property.Address = model.Address;
        property.AreaSqm = model.AreaSqm;
        property.EstimatedValue = model.EstimatedValue;
        property.RentalStatus = model.RentalStatus;
        property.MonthlyRent = model.MonthlyRent;
        property.TenantName = model.TenantName;
        property.Notes = model.Notes;
        property.WaqfNature = model.WaqfNature;
        property.IsAdminReceived = model.WaqfNature == "Ahli" ? model.IsAdminReceived : null;
        property.WaqfCondition = model.WaqfCondition;
        property.WaqifName = model.WaqifName;
        property.WaqfDocumentDate = model.WaqfDocumentDate;
        property.WaqfDocumentNumber = model.WaqfDocumentNumber;
        property.WaqfConditionText = model.WaqfCondition == "WithCondition" ? model.WaqfConditionText : null;
        property.HasEncroachment = model.HasEncroachment;
        property.EncroachmentNotes = model.EncroachmentNotes;
        property.OccupantEmployeeName = model.OccupantEmployeeName;
        property.IsOccupantRetired = model.OccupantEmployeeName != null ? model.IsOccupantRetired : null;
        property.UpdatedBy = User.Identity?.Name;

        await _propertyService.UpdateAsync(property);

        var newValues = $"الاسم: {property.NameAr}, القيمة: {property.EstimatedValue}";
        await _auditLogService.LogUpdateAsync("Property", property.Id, property.NameAr,
            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
            User.Identity?.Name, oldValues, newValues, HttpContext.Connection.RemoteIpAddress?.ToString());

        if (images != null && images.Count > 0)
            await _imageUploadService.UploadPropertyImagesAsync(property.Id, images, User.Identity?.Name);

        await UploadPropertyRegistrationFilesAsync(id, Request.Form.Files, "regFiles");

        // حفظ الحدود إذا وجدت
        if (!string.IsNullOrEmpty(BoundaryGeoJson))
        {
            var geometry = _geometryService.FromGeoJson(BoundaryGeoJson);
            if (geometry != null && geometry is Polygon polygon)
            {
                var existingBoundary = await _unitOfWork.Repository<PropertyBoundary>().Query()
                    .FirstOrDefaultAsync(b => b.PropertyId == id && b.BoundaryType == "Building");
                
                if (existingBoundary != null)
                {
                    existingBoundary.Boundary = polygon;
                    existingBoundary.CalculatedAreaSqm = _geometryService.CalculateAreaSquareMeters(polygon);
                    existingBoundary.PerimeterMeters = _geometryService.CalculatePerimeterMeters(polygon);
                    existingBoundary.UpdatedBy = User.Identity?.Name;
                }
                else
                {
                    var newBoundary = new PropertyBoundary
                    {
                        PropertyId = id,
                        Boundary = polygon,
                        BoundaryType = "Building",
                        CalculatedAreaSqm = _geometryService.CalculateAreaSquareMeters(polygon),
                        PerimeterMeters = _geometryService.CalculatePerimeterMeters(polygon),
                        CreatedBy = User.Identity?.Name
                    };
                    await _unitOfWork.Repository<PropertyBoundary>().AddAsync(newBoundary);
                }
                
                // Update property area from boundary
                property.TotalArea = (decimal?)_geometryService.CalculateAreaSquareMeters(polygon);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        TempData["Success"] = "تم تحديث بيانات العقار بنجاح";
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

        var property = await _propertyService.GetByIdAsync(id);
        if (property != null)
        {
            if (!await _permissionService.CanAccessPropertyAsync(User, property))
                return Forbid();

            await _propertyService.DeleteAsync(id);
            await _auditLogService.LogDeleteAsync("Property", id, property.NameAr,
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                User.Identity?.Name, HttpContext.Connection.RemoteIpAddress?.ToString());
        }
        TempData["Success"] = "تم حذف العقار بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Export(int? provinceId, int? typeId)
    {
        var query = await _permissionService.GetAuthorizedPropertiesAsync(User);
        var properties = await query
            .Include(p => p.PropertyType).Include(p => p.UsageType)
            .Include(p => p.Province).Include(p => p.WaqfOffice)
            .ToListAsync();

        if (provinceId.HasValue)
            properties = properties.Where(p => p.ProvinceId == provinceId.Value).ToList();
        if (typeId.HasValue)
            properties = properties.Where(p => p.PropertyTypeId == typeId.Value).ToList();

        var fileContent = _excelExportService.ExportPropertiesToExcel(properties);
        var fileName = $"العقارات_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        
        return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    [HttpPost]
    public async Task<IActionResult> UploadImages(int propertyId, IFormFileCollection images)
    {
        if (!await _permissionService.CanEditAsync(User))
            return Forbid();

        if (images != null && images.Count > 0)
        {
            await _imageUploadService.UploadPropertyImagesAsync(propertyId, images, User.Identity?.Name);
            TempData["Success"] = $"تم رفع {images.Count} صورة بنجاح";
        }
        return RedirectToAction(nameof(Edit), new { id = propertyId });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteImage(int imageId, int propertyId)
    {
        if (!await _permissionService.CanEditAsync(User))
            return Forbid();

        await _imageUploadService.DeletePropertyImageAsync(imageId);
        TempData["Success"] = "تم حذف الصورة بنجاح";
        return RedirectToAction(nameof(Edit), new { id = propertyId });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteRegistrationFile(int fileId, int propertyId)
    {
        var file = await _unitOfWork.Repository<PropertyRegistrationFile>().GetByIdAsync(fileId);
        if (file != null && file.PropertyId == propertyId)
        {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", file.FilePath.TrimStart('/'));
            if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
            await _unitOfWork.Repository<PropertyRegistrationFile>().DeleteAsync(file);
            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = "تم حذف الملف";
        }
        return RedirectToAction(nameof(Edit), new { id = propertyId });
    }

    private async Task UploadPropertyRegistrationFilesAsync(int propertyId, IFormFileCollection files, string inputName)
    {
        var regFiles = files.Where(f => f.Name == inputName).ToList();
        if (!regFiles.Any()) return;

        var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "property-docs", propertyId.ToString());
        Directory.CreateDirectory(uploadDir);

        foreach (var file in regFiles)
        {
            if (file.Length == 0) continue;
            var ext = Path.GetExtension(file.FileName);
            var savedName = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(uploadDir, savedName);
            using var stream = System.IO.File.Create(fullPath);
            await file.CopyToAsync(stream);

            var regFile = new PropertyRegistrationFile
            {
                PropertyId = propertyId,
                DocumentType = Request.Form["regDocType"].FirstOrDefault() ?? "وثيقة",
                FileName = file.FileName,
                FilePath = $"/uploads/property-docs/{propertyId}/{savedName}",
                FileSize = file.Length,
                MimeType = file.ContentType,
                UploadedBy = User.Identity?.Name
            };
            await _unitOfWork.Repository<PropertyRegistrationFile>().AddAsync(regFile);
        }
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task LoadViewDataAsync()
    {
        var provinces = await _permissionService.GetAuthorizedProvincesAsync(User);
        ViewBag.Provinces = new SelectList(provinces, "Id", "NameAr");

        var offices = await _permissionService.GetAuthorizedOfficesAsync(User);
        ViewBag.WaqfOffices = new SelectList(await offices.ToListAsync(), "Id", "NameAr");

        ViewBag.PropertyTypes = new SelectList(await _unitOfWork.PropertyTypes.GetAllAsync(), "Id", "NameAr");
        ViewBag.UsageTypes = new SelectList(await _unitOfWork.UsageTypes.GetAllAsync(), "Id", "NameAr");
        ViewBag.Districts = new SelectList(await _unitOfWork.Districts.GetAllAsync(), "Id", "NameAr");
    }

    // API endpoint للحصول على قائمة العقارات للمقارنة
    [HttpGet]
    public async Task<IActionResult> GetPropertiesList()
    {
        try
        {
            var properties = await _unitOfWork.Repository<WaqfProperty>().GetAllAsync();
            
            var result = properties.Select(p => new
            {
                id = p.Id,
                name = p.NameAr,
                area = p.AreaSqm ?? 0,
                pricePerSqm = p.PricePerSqm ?? 0,
                location = $"{p.Province?.NameAr} - {p.District?.NameAr}"
            }).ToList();

            return Json(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting properties list");
            return Json(new List<object>());
        }
    }
}
