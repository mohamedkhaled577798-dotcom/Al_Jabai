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
public class PropertiesController : Controller
{
    private readonly PropertyService _propertyService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ExcelExportService _excelExportService;

    public PropertiesController(PropertyService propertyService, IUnitOfWork unitOfWork, ExcelExportService excelExportService)
    {
        _propertyService = propertyService;
        _unitOfWork = unitOfWork;
        _excelExportService = excelExportService;
    }

    public async Task<IActionResult> Index(int? provinceId, int? typeId, string? search)
    {
        var properties = await _propertyService.GetAllAsync();

        if (provinceId.HasValue)
            properties = properties.Where(p => p.ProvinceId == provinceId.Value);
        if (typeId.HasValue)
            properties = properties.Where(p => p.PropertyTypeId == typeId.Value);
        if (!string.IsNullOrEmpty(search))
            properties = properties.Where(p => p.NameAr.Contains(search) || p.Code.Contains(search));

        await LoadViewDataAsync();
        ViewBag.CurrentSearch = search;
        return View(properties.ToList());
    }

    public async Task<IActionResult> Details(int id)
    {
        var property = await _propertyService.GetByIdAsync(id);
        if (property == null) return NotFound();
        return View(property);
    }

    public async Task<IActionResult> Create()
    {
        await LoadViewDataAsync();
        return View(new PropertyViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PropertyViewModel model)
    {
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
            CreatedBy = User.Identity?.Name
        };

        await _propertyService.CreateAsync(property);
        TempData["Success"] = "تم إضافة العقار بنجاح";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var property = await _propertyService.GetByIdAsync(id);
        if (property == null) return NotFound();

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
            Notes = property.Notes
        };

        await LoadViewDataAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PropertyViewModel model)
    {
        if (id != model.Id) return NotFound();

        if (!ModelState.IsValid)
        {
            await LoadViewDataAsync();
            return View(model);
        }

        var property = await _propertyService.GetByIdAsync(id);
        if (property == null) return NotFound();

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
        property.UpdatedBy = User.Identity?.Name;

        await _propertyService.UpdateAsync(property);
        TempData["Success"] = "تم تحديث بيانات العقار بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _propertyService.DeleteAsync(id);
        TempData["Success"] = "تم حذف العقار بنجاح";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadViewDataAsync()
    {
        ViewBag.Provinces = new SelectList(await _unitOfWork.Provinces.GetAllAsync(), "Id", "NameAr");
        ViewBag.PropertyTypes = new SelectList(await _unitOfWork.PropertyTypes.GetAllAsync(), "Id", "NameAr");
        ViewBag.UsageTypes = new SelectList(await _unitOfWork.UsageTypes.GetAllAsync(), "Id", "NameAr");
        ViewBag.WaqfOffices = new SelectList(await _unitOfWork.WaqfOffices.GetAllAsync(), "Id", "NameAr");
        ViewBag.Districts = new SelectList(await _unitOfWork.Districts.GetAllAsync(), "Id", "NameAr");
    }

    // ========== تصدير Excel ==========
    [HttpGet]
    public async Task<IActionResult> Export(int? provinceId, int? typeId)
    {
        var properties = await _unitOfWork.WaqfProperties.Query()
            .Include(p => p.PropertyType).Include(p => p.UsageType)
            .Include(p => p.Province).Include(p => p.WaqfOffice)
            .ToListAsync();

        if (provinceId.HasValue)
            properties = properties.Where(p => p.ProvinceId == provinceId.Value).ToList();
        if (typeId.HasValue)
            properties = properties.Where(p => p.PropertyTypeId == typeId.Value).ToList();

        var fileContent = _excelExportService.ExportProperties(properties);
        var fileName = $"العقارات_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        
        return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}
