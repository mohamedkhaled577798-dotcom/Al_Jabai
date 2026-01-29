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
public class MosquesController : Controller
{
    private readonly MosqueService _mosqueService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ExcelExportService _excelExportService;

    public MosquesController(MosqueService mosqueService, IUnitOfWork unitOfWork, ExcelExportService excelExportService)
    {
        _mosqueService = mosqueService;
        _unitOfWork = unitOfWork;
        _excelExportService = excelExportService;
    }

    public async Task<IActionResult> Index(int? provinceId, int? typeId, string? search)
    {
        var mosques = await _unitOfWork.Mosques.Query()
            .Include(m => m.MosqueType).Include(m => m.MosqueStatus)
            .Include(m => m.Province).Include(m => m.WaqfOffice)
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
        return View(mosques);
    }

    public async Task<IActionResult> Details(int id)
    {
        var mosque = await _mosqueService.GetByIdAsync(id);
        if (mosque == null) return NotFound();
        return View(mosque);
    }

    public async Task<IActionResult> Create()
    {
        await LoadViewDataAsync();
        return View(new MosqueViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MosqueViewModel model)
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
            ImamName = model.ImamName,
            ImamPhone = model.ImamPhone,
            EstablishedYear = model.EstablishedYear,
            Notes = model.Notes,
            CreatedBy = User.Identity?.Name
        };

        await _mosqueService.CreateAsync(mosque);
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

        await LoadViewDataAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MosqueViewModel model)
    {
        if (id != model.Id) return NotFound();

        if (!ModelState.IsValid)
        {
            await LoadViewDataAsync();
            return View(model);
        }

        var mosque = await _mosqueService.GetByIdAsync(id);
        if (mosque == null) return NotFound();

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
        mosque.Notes = model.Notes;
        mosque.UpdatedBy = User.Identity?.Name;

        await _mosqueService.UpdateAsync(mosque);
        TempData["Success"] = "تم تحديث بيانات المسجد بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _mosqueService.DeleteAsync(id);
        TempData["Success"] = "تم حذف المسجد بنجاح";
        return RedirectToAction(nameof(Index));
    }

    // ========== تصدير Excel ==========
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

        var fileContent = _excelExportService.ExportMosques(mosques);
        var fileName = $"المساجد_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        
        return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    private async Task LoadViewDataAsync()
    {
        ViewBag.Provinces = new SelectList(await _unitOfWork.Provinces.GetAllAsync(), "Id", "NameAr");
        ViewBag.MosqueTypes = new SelectList(await _unitOfWork.MosqueTypes.GetAllAsync(), "Id", "NameAr");
        ViewBag.MosqueStatuses = new SelectList(await _unitOfWork.MosqueStatuses.GetAllAsync(), "Id", "NameAr");
        ViewBag.WaqfOffices = new SelectList(await _unitOfWork.WaqfOffices.GetAllAsync(), "Id", "NameAr");
        ViewBag.Districts = new SelectList(await _unitOfWork.Districts.GetAllAsync(), "Id", "NameAr");
    }
}
