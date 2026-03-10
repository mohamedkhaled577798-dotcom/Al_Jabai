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
using WaqfGIS.Services.Storage;
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
    private readonly GeometryService _geometryService;
    private readonly SecureFileStorageService _storage;

    public MosquesController(
        MosqueService mosqueService, 
        IUnitOfWork unitOfWork, 
        ExcelExportService excelExportService,
        AuditLogService auditLogService,
        ImageUploadService imageUploadService,
        PermissionService permissionService,
        GeometryService geometryService,
        SecureFileStorageService storage)
    {
        _mosqueService      = mosqueService;
        _unitOfWork         = unitOfWork;
        _excelExportService = excelExportService;
        _auditLogService    = auditLogService;
        _imageUploadService = imageUploadService;
        _permissionService  = permissionService;
        _geometryService    = geometryService;
        _storage            = storage;
    }

    public async Task<IActionResult> Index(int? provinceId, int? typeId, string? search)
    {
        var mosques = await _unitOfWork.Mosques.Query()
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
        ViewBag.CanEdit = true;
        ViewBag.CanDelete = User.IsInRole("SuperAdmin") || User.IsInRole("Admin");

        return View(mosques);
    }

    public async Task<IActionResult> Details(int id)
    {
        var mosque = await _unitOfWork.Mosques.Query()
            .Include(m => m.MosqueType)
            .Include(m => m.MosqueStatus)
            .Include(m => m.Province)
            .Include(m => m.District)
            .Include(m => m.WaqfOffice)
            .FirstOrDefaultAsync(m => m.Id == id);
            
        if (mosque == null) return NotFound();

        ViewBag.Images = await _imageUploadService.GetMosqueImagesAsync(id);
        ViewBag.RegistrationFiles = await _unitOfWork.Repository<MosqueRegistrationFile>().Query()
            .Where(f => f.MosqueId == id).OrderByDescending(f => f.UploadedAt).ToListAsync();
        
        // Load boundary
        var boundary = await _unitOfWork.Repository<MosqueBoundary>().Query()
            .FirstOrDefaultAsync(b => b.MosqueId == id && b.BoundaryType == "Building");
        ViewBag.BoundaryGeoJson = boundary != null ? _geometryService.ToGeoJson(boundary.Boundary) : null;
        ViewBag.BoundaryArea = boundary?.CalculatedAreaSqm;
        
        ViewBag.CanEdit = true;
        return View(mosque);
    }

    public async Task<IActionResult> Create()
    {
        await LoadViewDataAsync();
        return View(new MosqueViewModel { Latitude = 33.3, Longitude = 44.4 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MosqueViewModel model, IFormFileCollection images)
    {
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
            HasAirConditioning = model.HasAirConditioning,
            HasAblutionFacility = model.HasWuduArea,
            ImamName = model.ImamName,
            ImamPhone = model.ImamPhone,
            MuezzinName = model.MuezzinName,
            MuezzinPhone = model.MuezzinPhone,
            EstablishedYear = model.EstablishedYear,
            LastRenovationYear = model.LastRenovationYear,
            Notes = model.Notes,
            WaqfNature = model.WaqfNature,
            IsAdminReceived = model.WaqfNature == "Ahli" ? model.IsAdminReceived : null,
            WaqfCondition = model.WaqfCondition,
            WaqifName = model.WaqifName,
            WaqfDocumentDate = model.WaqfDocumentDate,
            WaqfDocumentNumber = model.WaqfDocumentNumber,
            WaqfConditionText = model.WaqfCondition == "WithCondition" ? model.WaqfConditionText : null,
            IsUsurped = model.IsUsurped,
            IsClosed = model.IsClosed,
            IsContested = model.IsContested,
            CreatedBy = User.Identity?.Name
        };

        await _mosqueService.CreateAsync(mosque);

        // رفع الصور إذا وجدت
        if (images != null && images.Count > 0)
            await _imageUploadService.UploadMosqueImagesAsync(mosque.Id, images, User.Identity?.Name);

        // رفع ملفات التسجيل
        await UploadMosqueRegistrationFilesAsync(mosque.Id, Request.Form.Files, "regFiles");

        TempData["Success"] = "تم إضافة المسجد بنجاح";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var mosque = await _mosqueService.GetByIdAsync(id);
        if (mosque == null) return NotFound();

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
            Latitude = mosque.Location?.Y ?? 33.3,
            Longitude = mosque.Location?.X ?? 44.4,
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
            HasAirConditioning = mosque.HasAirConditioning,
            HasWuduArea = mosque.HasAblutionFacility,
            ImamName = mosque.ImamName,
            ImamPhone = mosque.ImamPhone,
            MuezzinName = mosque.MuezzinName,
            MuezzinPhone = mosque.MuezzinPhone,
            EstablishedYear = mosque.EstablishedYear,
            LastRenovationYear = mosque.LastRenovationYear,
            Notes = mosque.Notes,
            WaqfNature = mosque.WaqfNature,
            IsAdminReceived = mosque.IsAdminReceived,
            WaqfCondition = mosque.WaqfCondition,
            WaqifName = mosque.WaqifName,
            WaqfDocumentDate = mosque.WaqfDocumentDate,
            WaqfDocumentNumber = mosque.WaqfDocumentNumber,
            WaqfConditionText = mosque.WaqfConditionText,
            IsUsurped = mosque.IsUsurped,
            IsClosed = mosque.IsClosed,
            IsContested = mosque.IsContested
        };

        ViewBag.Images = await _imageUploadService.GetMosqueImagesAsync(id);
        ViewBag.RegistrationFiles = await _unitOfWork.Repository<MosqueRegistrationFile>().Query()
            .Where(f => f.MosqueId == id).ToListAsync();
        
        // Load boundary if exists
        var boundary = await _unitOfWork.Repository<MosqueBoundary>().Query()
            .FirstOrDefaultAsync(b => b.MosqueId == id && b.BoundaryType == "Building");
        ViewBag.BoundaryGeoJson = boundary != null ? _geometryService.ToGeoJson(boundary.Boundary) : null;
        
        await LoadViewDataAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MosqueViewModel model, IFormFileCollection images, string? BoundaryGeoJson)
    {
        if (id != model.Id) return NotFound();

        var mosque = await _mosqueService.GetByIdAsync(id);
        if (mosque == null) return NotFound();

        if (!ModelState.IsValid)
        {
            ViewBag.Images = await _imageUploadService.GetMosqueImagesAsync(id);
            await LoadViewDataAsync();
            return View(model);
        }

        // تحديث جميع البيانات
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
        mosque.NearestLandmark = model.NearestLandmark;
        mosque.Capacity = model.Capacity;
        mosque.AreaSqm = model.AreaSqm;
        mosque.HasFridayPrayer = model.HasFridayPrayer;
        mosque.HasMinaret = model.HasMinaret;
        mosque.HasDome = model.HasDome;
        mosque.HasParking = model.HasParking;
        mosque.HasWomenSection = model.HasWomenSection;
        mosque.HasAirConditioning = model.HasAirConditioning;
        mosque.HasAblutionFacility = model.HasWuduArea;
        mosque.ImamName = model.ImamName;
        mosque.ImamPhone = model.ImamPhone;
        mosque.MuezzinName = model.MuezzinName;
        mosque.MuezzinPhone = model.MuezzinPhone;
        mosque.EstablishedYear = model.EstablishedYear;
        mosque.LastRenovationYear = model.LastRenovationYear;
        mosque.Notes = model.Notes;
        mosque.WaqfNature = model.WaqfNature;
        mosque.IsAdminReceived = model.WaqfNature == "Ahli" ? model.IsAdminReceived : null;
        mosque.WaqfCondition = model.WaqfCondition;
        mosque.WaqifName = model.WaqifName;
        mosque.WaqfDocumentDate = model.WaqfDocumentDate;
        mosque.WaqfDocumentNumber = model.WaqfDocumentNumber;
        mosque.WaqfConditionText = model.WaqfCondition == "WithCondition" ? model.WaqfConditionText : null;
        mosque.IsUsurped = model.IsUsurped;
        mosque.IsClosed = model.IsClosed;
        mosque.IsContested = model.IsContested;
        mosque.UpdatedBy = User.Identity?.Name;

        await _mosqueService.UpdateAsync(mosque);

        // رفع الصور الجديدة
        if (images != null && images.Count > 0)
            await _imageUploadService.UploadMosqueImagesAsync(mosque.Id, images, User.Identity?.Name);

        // رفع ملفات التسجيل
        await UploadMosqueRegistrationFilesAsync(id, Request.Form.Files, "regFiles");

        // حفظ الحدود إذا وجدت
        if (!string.IsNullOrEmpty(BoundaryGeoJson))
        {
            var geometry = _geometryService.FromGeoJson(BoundaryGeoJson);
            if (geometry != null && geometry is Polygon polygon)
            {
                var existingBoundary = await _unitOfWork.Repository<MosqueBoundary>().Query()
                    .FirstOrDefaultAsync(b => b.MosqueId == id && b.BoundaryType == "Building");
                
                if (existingBoundary != null)
                {
                    existingBoundary.Boundary = polygon;
                    existingBoundary.CalculatedAreaSqm = _geometryService.CalculateAreaSquareMeters(polygon);
                    existingBoundary.PerimeterMeters = _geometryService.CalculatePerimeterMeters(polygon);
                    existingBoundary.UpdatedBy = User.Identity?.Name;
                }
                else
                {
                    var newBoundary = new MosqueBoundary
                    {
                        MosqueId = id,
                        Boundary = polygon,
                        BoundaryType = "Building",
                        CalculatedAreaSqm = _geometryService.CalculateAreaSquareMeters(polygon),
                        PerimeterMeters = _geometryService.CalculatePerimeterMeters(polygon),
                        CreatedBy = User.Identity?.Name
                    };
                    await _unitOfWork.Repository<MosqueBoundary>().AddAsync(newBoundary);
                }
                
                // Update mosque area from boundary
                mosque.AreaSqm = (decimal?)_geometryService.CalculateAreaSquareMeters(polygon);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        TempData["Success"] = "تم تحديث بيانات المسجد بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var mosque = await _mosqueService.GetByIdAsync(id);
        if (mosque != null)
        {
            await _mosqueService.DeleteAsync(id);
            TempData["Success"] = "تم حذف المسجد بنجاح";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Export(int? provinceId, int? typeId)
    {
        var mosques = await _unitOfWork.Mosques.Query()
            .Include(m => m.MosqueType).Include(m => m.MosqueStatus)
            .Include(m => m.Province).Include(m => m.WaqfOffice)
            .ToListAsync();

        if (provinceId.HasValue)
            mosques = mosques.Where(m => m.ProvinceId == provinceId.Value).ToList();
        if (typeId.HasValue)
            mosques = mosques.Where(m => m.MosqueTypeId == typeId.Value).ToList();

        var fileContent = _excelExportService.ExportMosquesToExcel(mosques);
        return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
            $"المساجد_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteImage(int imageId, int mosqueId)
    {
        await _imageUploadService.DeleteMosqueImageAsync(imageId);
        TempData["Success"] = "تم حذف الصورة";
        return RedirectToAction(nameof(Edit), new { id = mosqueId });
    }

    [HttpPost]
    public async Task<IActionResult> SetMainImage(int imageId, int mosqueId)
    {
        await _imageUploadService.SetMainMosqueImageAsync(mosqueId, imageId);
        TempData["Success"] = "تم تعيين الصورة الرئيسية";
        return RedirectToAction(nameof(Edit), new { id = mosqueId });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteRegistrationFile(int fileId, int mosqueId)
    {
        var file = await _unitOfWork.Repository<MosqueRegistrationFile>().GetByIdAsync(fileId);
        if (file != null && file.MosqueId == mosqueId)
        {
            // حذف الملف المشفَّر من القرص
            var parts = file.FilePath.Split('/');
            if (parts.Length >= 2)
                _storage.DeleteFile(_storage.GetDiskPath(parts[0], parts[1]));

            await _unitOfWork.Repository<MosqueRegistrationFile>().DeleteAsync(file);
            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = "تم حذف الملف";
        }
        return RedirectToAction(nameof(Edit), new { id = mosqueId });
    }

    private async Task UploadMosqueRegistrationFilesAsync(int mosqueId, IFormFileCollection files, string inputName)
    {
        var regFiles = files.Where(f => f.Name == inputName && f.Length > 0).ToList();
        if (!regFiles.Any()) return;

        var docType = Request.Form["regDocType"].FirstOrDefault() ?? "وثيقة";

        foreach (var file in regFiles)
        {
            // حفظ مشفَّر في MosqueDocs
            var saved = await _storage.SaveFileAsync(file, "MosqueDocs");

            var regFile = new MosqueRegistrationFile
            {
                MosqueId     = mosqueId,
                DocumentType = docType,
                FileName     = saved.OriginalName,
                FilePath     = saved.DbPath,            // "MosqueDocs/uid"
                FileSize     = saved.FileSize,
                MimeType     = saved.MimeType,
                UploadedBy   = User.Identity?.Name
            };
            await _unitOfWork.Repository<MosqueRegistrationFile>().AddAsync(regFile);
        }
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task LoadViewDataAsync()
    {
        // تحميل جميع البيانات
        ViewBag.Provinces = new SelectList(await _unitOfWork.Provinces.GetAllAsync(), "Id", "NameAr");
        ViewBag.WaqfOffices = new SelectList(await _unitOfWork.WaqfOffices.GetAllAsync(), "Id", "NameAr");
        ViewBag.MosqueTypes = new SelectList(await _unitOfWork.MosqueTypes.GetAllAsync(), "Id", "NameAr");
        ViewBag.MosqueStatuses = new SelectList(await _unitOfWork.MosqueStatuses.GetAllAsync(), "Id", "NameAr");
        ViewBag.Districts = new SelectList(await _unitOfWork.Districts.GetAllAsync(), "Id", "NameAr");
    }
}
