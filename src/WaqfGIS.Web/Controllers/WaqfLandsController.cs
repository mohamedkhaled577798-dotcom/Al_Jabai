using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WaqfGIS.Core.Entities;
using WaqfGIS.Core.Interfaces;
using WaqfGIS.Services;
using WaqfGIS.Services.GIS;
using WaqfGIS.Services.Storage;
using WaqfGIS.Web.Models;

namespace WaqfGIS.Web.Controllers;

[Authorize]
public class WaqfLandsController : Controller
{
    private readonly WaqfLandService _landService;
    private readonly GeometryService _geometryService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AuditLogService _auditLogService;
    private readonly SecureFileStorageService _storage;

    public WaqfLandsController(
        WaqfLandService landService,
        GeometryService geometryService,
        IUnitOfWork unitOfWork,
        AuditLogService auditLogService,
        SecureFileStorageService storage)
    {
        _landService     = landService;
        _geometryService = geometryService;
        _unitOfWork      = unitOfWork;
        _auditLogService = auditLogService;
        _storage         = storage;
    }

    // GET: WaqfLands
    public async Task<IActionResult> Index(WaqfLandSearchViewModel search)
    {
        var (items, totalCount) = await _landService.SearchAsync(
            search.SearchTerm,
            search.ProvinceId,
            search.DistrictId,
            search.WaqfOfficeId,
            search.LandType,
            search.LandUse,
            search.MinArea,
            search.MaxArea,
            search.HasBoundary,
            search.PageNumber,
            search.PageSize,
            search.SortBy,
            search.SortDescending);

        var viewModels = items.Select(MapToViewModel).ToList();

        ViewBag.Search = search;
        ViewBag.TotalCount = totalCount;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)search.PageSize);
        
        await LoadDropdownsAsync();

        return View(viewModels);
    }

    // GET: WaqfLands/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var land = await _landService.GetByIdAsync(id);
        if (land == null)
            return NotFound();

        var viewModel = MapToViewModel(land);
        viewModel.BoundaryGeoJson = _landService.GetBoundaryGeoJson(land);

        ViewBag.RegistrationFiles = await _unitOfWork.Repository<WaqfLandRegistrationFile>().Query()
            .Where(f => f.WaqfLandId == id).OrderByDescending(f => f.UploadedAt).ToListAsync();

        return View(viewModel);
    }

    // GET: WaqfLands/Create
    public async Task<IActionResult> Create()
    {
        var viewModel = new WaqfLandViewModel
        {
            CenterLatitude = 33.3,
            CenterLongitude = 44.4
        };
        
        await LoadDropdownsAsync(viewModel);
        return View(viewModel);
    }

    // POST: WaqfLands/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(WaqfLandViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            var land = MapToEntity(viewModel);
            
            await _landService.CreateAsync(land, viewModel.BoundaryGeoJson, User.Identity?.Name);

            await UploadLandRegistrationFilesAsync(land.Id, Request.Form.Files, "regFiles");
            
            await _auditLogService.LogAsync("Create", "WaqfLand", land.Id, land.NameAr, 
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                User.Identity?.Name);

            TempData["Success"] = "تم إضافة أرض الوقف بنجاح";
            return RedirectToAction(nameof(Details), new { id = land.Id });
        }

        await LoadDropdownsAsync(viewModel);
        return View(viewModel);
    }

    // GET: WaqfLands/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        if (!CanEditWaqfLand())
        {
            TempData["Error"] = "تعديل الأراضي الوقفية محجوز للمديرين فقط";
            return RedirectToAction(nameof(Index));
        }

        var land = await _landService.GetByIdAsync(id);
        if (land == null)
            return NotFound();

        var viewModel = MapToViewModel(land);
        viewModel.BoundaryGeoJson = _landService.GetBoundaryGeoJson(land);
        ViewBag.RegistrationFiles = await _unitOfWork.Repository<WaqfLandRegistrationFile>().Query()
            .Where(f => f.WaqfLandId == id).ToListAsync();
        
        await LoadDropdownsAsync(viewModel);
        return View(viewModel);
    }

    // GET: WaqfLands/Edit/5
    // تعديل الأراضي الوقفية محجوز لـ Admin وProvinceAdmin ومدير الدائرة فقط
    private bool CanEditWaqfLand() =>
        User.IsInRole("SuperAdmin") || User.IsInRole("Admin") ||
        User.IsInRole("ProvinceAdmin") || User.IsInRole("OfficeManager");

    // POST: WaqfLands/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, WaqfLandViewModel viewModel)
    {
        if (id != viewModel.Id)
            return NotFound();

        if (!CanEditWaqfLand())
        {
            TempData["Error"] = "تعديل الأراضي الوقفية محجوز للمديرين فقط";
            return RedirectToAction(nameof(Index));
        }

        if (ModelState.IsValid)
        {
            var land = MapToEntity(viewModel);
            
            var result = await _landService.UpdateAsync(id, land, viewModel.BoundaryGeoJson, User.Identity?.Name);
            if (result == null)
                return NotFound();

            await UploadLandRegistrationFilesAsync(id, Request.Form.Files, "regFiles");

            await _auditLogService.LogAsync("Update", "WaqfLand", id, land.NameAr, 
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                User.Identity?.Name);

            TempData["Success"] = "تم تحديث أرض الوقف بنجاح";
            return RedirectToAction(nameof(Details), new { id });
        }

        await LoadDropdownsAsync(viewModel);
        return View(viewModel);
    }

    // GET: WaqfLands/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var land = await _landService.GetByIdAsync(id);
        if (land == null)
            return NotFound();

        return View(MapToViewModel(land));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteRegistrationFile(int fileId, int landId)
    {
        if (!CanEditWaqfLand()) return Forbid();
        var file = await _unitOfWork.Repository<WaqfLandRegistrationFile>().GetByIdAsync(fileId);
        if (file != null && file.WaqfLandId == landId)
        {
            var parts = file.FilePath.Split('/');
            if (parts.Length >= 2)
                _storage.DeleteFile(_storage.GetDiskPath(parts[0], parts[1]));
            await _unitOfWork.Repository<WaqfLandRegistrationFile>().DeleteAsync(file);
            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = "تم حذف الملف";
        }
        return RedirectToAction(nameof(Edit), new { id = landId });
    }

    private async Task UploadLandRegistrationFilesAsync(int landId, IFormFileCollection files, string inputName)
    {
        var regFiles = files.Where(f => f.Name == inputName && f.Length > 0).ToList();
        if (!regFiles.Any()) return;
        var docType = Request.Form["regDocType"].FirstOrDefault() ?? "وثيقة";
        foreach (var file in regFiles)
        {
            var saved = await _storage.SaveFileAsync(file, "LandDocs");
            var regFile = new WaqfLandRegistrationFile
            {
                WaqfLandId   = landId,
                DocumentType = docType,
                FileName     = saved.OriginalName,
                FilePath     = saved.DbPath,
                FileSize     = saved.FileSize,
                MimeType     = saved.MimeType,
                UploadedBy   = User.Identity?.Name
            };
            await _unitOfWork.Repository<WaqfLandRegistrationFile>().AddAsync(regFile);
        }
        await _unitOfWork.SaveChangesAsync();
    }

    // POST: WaqfLands/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var land = await _landService.GetByIdAsync(id);
        if (land == null)
            return NotFound();

        await _landService.DeleteAsync(id, User.Identity?.Name);
        
        await _auditLogService.LogAsync("Delete", "WaqfLand", id, land.NameAr, 
            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
            User.Identity?.Name);

        TempData["Success"] = "تم حذف أرض الوقف بنجاح";
        return RedirectToAction(nameof(Index));
    }

    // GET: WaqfLands/Statistics
    public async Task<IActionResult> Statistics(int? provinceId)
    {
        var stats = await _landService.GetStatisticsAsync(provinceId);
        await LoadDropdownsAsync();
        ViewBag.ProvinceId = provinceId;
        return View(stats);
    }

    // GET: WaqfLands/Map
    public async Task<IActionResult> Map()
    {
        await LoadDropdownsAsync();
        return View();
    }

    #region Helper Methods

    private async Task LoadDropdownsAsync(WaqfLandViewModel? viewModel = null)
    {
        var provinces = await _unitOfWork.Provinces.GetAllAsync();
        var districts = await _unitOfWork.Districts.GetAllAsync();
        var offices = await _unitOfWork.WaqfOffices.GetAllAsync();

        var provincesList = new SelectList(provinces.OrderBy(p => p.NameAr), "Id", "NameAr", viewModel?.ProvinceId);
        var districtsList = new SelectList(districts.OrderBy(d => d.NameAr), "Id", "NameAr", viewModel?.DistrictId);
        var officesList = new SelectList(offices.OrderBy(o => o.NameAr), "Id", "NameAr", viewModel?.WaqfOfficeId);

        if (viewModel != null)
        {
            viewModel.Provinces = provincesList;
            viewModel.Districts = districtsList;
            viewModel.WaqfOffices = officesList;
        }
        else
        {
            ViewBag.Provinces = provincesList;
            ViewBag.Districts = districtsList;
            ViewBag.WaqfOffices = officesList;
        }

        ViewBag.LandTypes = WaqfLandViewModel.LandTypes;
        ViewBag.LandUses = WaqfLandViewModel.LandUses;
    }

    private WaqfLandViewModel MapToViewModel(WaqfLand land)
    {
        return new WaqfLandViewModel
        {
            Id = land.Id,
            Code = land.Code,
            NameAr = land.NameAr,
            NameEn = land.NameEn,
            Description = land.Description,
            ProvinceId = land.ProvinceId,
            DistrictId = land.DistrictId,
            WaqfOfficeId = land.WaqfOfficeId,
            Address = land.Address,
            Neighborhood = land.Neighborhood,
            LandType = land.LandType,
            LandUse = land.LandUse,
            ZoningCode = land.ZoningCode,
            CalculatedAreaSqm = land.CalculatedAreaSqm,
            LegalAreaSqm = land.LegalAreaSqm,
            AreaDonum = land.AreaDonum,
            PerimeterMeters = land.PerimeterMeters,
            DeedNumber = land.DeedNumber,
            RegistrationNumber = land.RegistrationNumber,
            RegistrationDate = land.RegistrationDate,
            OwnershipStatus = land.OwnershipStatus,
            EstimatedValue = land.EstimatedValue,
            AnnualRevenue = land.AnnualRevenue,
            Notes = land.Notes,
            WaqfNature = land.WaqfNature,
            IsAdminReceived = land.IsAdminReceived,
            WaqfCondition = land.WaqfCondition,
            WaqifName = land.WaqifName,
            WaqfDocumentDate = land.WaqfDocumentDate,
            WaqfDocumentNumber = land.WaqfDocumentNumber,
            WaqfConditionText = land.WaqfConditionText,
            HasEncroachment = land.HasEncroachment,
            EncroachmentNotes = land.EncroachmentNotes,
            ProvinceName = land.Province?.NameAr,
            DistrictName = land.District?.NameAr,
            WaqfOfficeName = land.WaqfOffice?.NameAr,
            CreatedAt = land.CreatedAt,
            CreatedBy = land.CreatedBy,
            CenterLatitude = land.CenterPoint?.Y,
            CenterLongitude = land.CenterPoint?.X
        };
    }

    private WaqfLand MapToEntity(WaqfLandViewModel viewModel)
    {
        return new WaqfLand
        {
            Id = viewModel.Id,
            NameAr = viewModel.NameAr,
            NameEn = viewModel.NameEn,
            Description = viewModel.Description,
            ProvinceId = viewModel.ProvinceId,
            DistrictId = viewModel.DistrictId,
            WaqfOfficeId = viewModel.WaqfOfficeId,
            Address = viewModel.Address,
            Neighborhood = viewModel.Neighborhood,
            LandType = viewModel.LandType,
            LandUse = viewModel.LandUse,
            ZoningCode = viewModel.ZoningCode,
            LegalAreaSqm = viewModel.LegalAreaSqm,
            DeedNumber = viewModel.DeedNumber,
            RegistrationNumber = viewModel.RegistrationNumber,
            RegistrationDate = viewModel.RegistrationDate,
            OwnershipStatus = viewModel.OwnershipStatus,
            EstimatedValue = viewModel.EstimatedValue,
            AnnualRevenue = viewModel.AnnualRevenue,
            Notes = viewModel.Notes,
            WaqfNature = viewModel.WaqfNature,
            IsAdminReceived = viewModel.WaqfNature == "Ahli" ? viewModel.IsAdminReceived : null,
            WaqfCondition = viewModel.WaqfCondition,
            WaqifName = viewModel.WaqifName,
            WaqfDocumentDate = viewModel.WaqfDocumentDate,
            WaqfDocumentNumber = viewModel.WaqfDocumentNumber,
            WaqfConditionText = viewModel.WaqfCondition == "WithCondition" ? viewModel.WaqfConditionText : null,
            HasEncroachment = viewModel.HasEncroachment,
            EncroachmentNotes = viewModel.EncroachmentNotes
        };
    }

    #endregion
}
