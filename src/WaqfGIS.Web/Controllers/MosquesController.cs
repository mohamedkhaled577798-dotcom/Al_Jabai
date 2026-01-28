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
    private readonly ILogger<MosquesController> _logger;

    public MosquesController(
        MosqueService mosqueService,
        IUnitOfWork unitOfWork,
        ILogger<MosquesController> logger)
    {
        _mosqueService = mosqueService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IActionResult> Index(int? provinceId, int? typeId, string? search)
    {
        var mosques = await _mosqueService.GetAllAsync();

        if (provinceId.HasValue)
            mosques = mosques.Where(m => m.ProvinceId == provinceId.Value);

        if (typeId.HasValue)
            mosques = mosques.Where(m => m.MosqueTypeId == typeId.Value);

        if (!string.IsNullOrEmpty(search))
            mosques = mosques.Where(m => m.NameAr.Contains(search) || m.Code.Contains(search));

        await LoadViewDataAsync();
        ViewBag.CurrentProvinceId = provinceId;
        ViewBag.CurrentTypeId = typeId;
        ViewBag.CurrentSearch = search;

        return View(mosques.ToList());
    }

    public async Task<IActionResult> Details(int id)
    {
        var mosque = await _mosqueService.GetByIdAsync(id);
        if (mosque == null)
            return NotFound();

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
            Code = model.Code,

            NameAr = model.NameAr,
            NameEn = model.NameEn,

            WaqfOfficeId = model.WaqfOfficeId,
            MosqueTypeId = model.MosqueTypeId,
            MosqueStatusId = model.MosqueStatusId,

            ProvinceId = model.ProvinceId,
            DistrictId = model.DistrictId,
            SubDistrictId = model.SubDistrictId,

            Location = new Point(model.Longitude, model.Latitude) { SRID = 4326 },

            Address = model.Address,
            Neighborhood = model.Neighborhood,
            NearestLandmark = model.NearestLandmark,

            Capacity = model.Capacity,
            AreaSqm = model.AreaSqm,
            FloorsCount = model.FloorsCount,

            HasFridayPrayer = model.HasFridayPrayer,
            HasMinaret = model.HasMinaret,
            HasDome = model.HasDome,
            HasParking = model.HasParking,
            HasWomenSection = model.HasWomenSection,
            HasLibrary = model.HasLibrary,
            HasAblutionFacility = model.HasAblutionFacility,

            ImamName = model.ImamName,
            ImamPhone = model.ImamPhone,
            MuezzinName = model.MuezzinName,

            EstablishedYear = model.EstablishedYear,
            LastRenovationYear = model.LastRenovationYear,

            DeedNumber = model.DeedNumber,
            RegistrationNumber = model.RegistrationNumber,
            RegistrationDate = model.RegistrationDate,

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
        if (mosque == null)
            return NotFound();

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
            Notes = mosque.Notes,
            Code = mosque.Code,
            FloorsCount = mosque.FloorsCount,
            HasLibrary = mosque.HasLibrary,
            HasAblutionFacility = mosque.HasAblutionFacility,
            MuezzinName = mosque.MuezzinName,
            LastRenovationYear = mosque.LastRenovationYear,
            DeedNumber = mosque.DeedNumber,
            RegistrationNumber = mosque.RegistrationNumber,
            RegistrationDate = mosque.RegistrationDate

        };

        await LoadViewDataAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MosqueViewModel model)
    {
        if (id != model.Id)
            return NotFound();

        if (!ModelState.IsValid)
        {
            await LoadViewDataAsync();
            return View(model);
        }

        var mosque = await _mosqueService.GetByIdAsync(id);
        if (mosque == null)
            return NotFound();

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

    private async Task LoadViewDataAsync()
    {
        ViewBag.Provinces = new SelectList(
            await _unitOfWork.Provinces.GetAllAsync(), "Id", "NameAr");
        ViewBag.MosqueTypes = new SelectList(
            await _unitOfWork.MosqueTypes.GetAllAsync(), "Id", "NameAr");
        ViewBag.MosqueStatuses = new SelectList(
            await _unitOfWork.MosqueStatuses.GetAllAsync(), "Id", "NameAr");
        ViewBag.WaqfOffices = new SelectList(
            await _unitOfWork.WaqfOffices.GetAllAsync(), "Id", "NameAr");
        ViewBag.Districts = new SelectList(
            await _unitOfWork.Districts.GetAllAsync(), "Id", "NameAr");
    }
}
