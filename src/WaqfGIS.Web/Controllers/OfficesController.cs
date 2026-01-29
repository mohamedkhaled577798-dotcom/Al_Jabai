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
public class OfficesController : Controller
{
    private readonly OfficeService _officeService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ExcelExportService _excelExportService;

    public OfficesController(OfficeService officeService, IUnitOfWork unitOfWork, ExcelExportService excelExportService)
    {
        _officeService = officeService;
        _unitOfWork = unitOfWork;
        _excelExportService = excelExportService;
    }

    public async Task<IActionResult> Index(int? provinceId, int? typeId, string? search)
    {
        var offices = await _officeService.GetAllAsync();

        if (provinceId.HasValue)
            offices = offices.Where(o => o.ProvinceId == provinceId.Value);
        if (typeId.HasValue)
            offices = offices.Where(o => o.OfficeTypeId == typeId.Value);
        if (!string.IsNullOrEmpty(search))
            offices = offices.Where(o => o.NameAr.Contains(search) || o.Code.Contains(search));

        await LoadViewDataAsync();
        ViewBag.CurrentSearch = search;
        return View(offices.ToList());
    }

    public async Task<IActionResult> Details(int id)
    {
        var office = await _officeService.GetByIdAsync(id);
        if (office == null) return NotFound();
        return View(office);
    }

    public async Task<IActionResult> Create()
    {
        await LoadViewDataAsync();
        return View(new OfficeViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OfficeViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadViewDataAsync();
            return View(model);
        }

        var office = new WaqfOffice
        {
            NameAr = model.NameAr,
            NameEn = model.NameEn,
            OfficeTypeId = model.OfficeTypeId,
            ParentOfficeId = model.ParentOfficeId,
            ProvinceId = model.ProvinceId,
            DistrictId = model.DistrictId,
            Address = model.Address,
            Phone = model.Phone,
            Email = model.Email,
            ManagerName = model.ManagerName,
            ManagerPhone = model.ManagerPhone,
            Notes = model.Notes,
            CreatedBy = User.Identity?.Name
        };

        if (model.Latitude != 0 && model.Longitude != 0)
            office.Location = new Point(model.Longitude, model.Latitude) { SRID = 4326 };

        await _officeService.CreateAsync(office);
        TempData["Success"] = "تم إضافة الدائرة الوقفية بنجاح";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var office = await _officeService.GetByIdAsync(id);
        if (office == null) return NotFound();

        var model = new OfficeViewModel
        {
            Id = office.Id,
            NameAr = office.NameAr,
            NameEn = office.NameEn,
            OfficeTypeId = office.OfficeTypeId,
            ParentOfficeId = office.ParentOfficeId,
            ProvinceId = office.ProvinceId,
            DistrictId = office.DistrictId,
            Address = office.Address,
            Phone = office.Phone,
            Email = office.Email,
            ManagerName = office.ManagerName,
            ManagerPhone = office.ManagerPhone,
            Notes = office.Notes,
            Latitude = office.Location?.Y ?? 0,
            Longitude = office.Location?.X ?? 0
        };

        await LoadViewDataAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, OfficeViewModel model)
    {
        if (id != model.Id) return NotFound();

        if (!ModelState.IsValid)
        {
            await LoadViewDataAsync();
            return View(model);
        }

        var office = await _officeService.GetByIdAsync(id);
        if (office == null) return NotFound();

        office.NameAr = model.NameAr;
        office.NameEn = model.NameEn;
        office.OfficeTypeId = model.OfficeTypeId;
        office.ParentOfficeId = model.ParentOfficeId;
        office.ProvinceId = model.ProvinceId;
        office.Address = model.Address;
        office.Phone = model.Phone;
        office.Email = model.Email;
        office.ManagerName = model.ManagerName;
        office.ManagerPhone = model.ManagerPhone;
        office.Notes = model.Notes;
        office.UpdatedBy = User.Identity?.Name;

        if (model.Latitude != 0 && model.Longitude != 0)
            office.Location = new Point(model.Longitude, model.Latitude) { SRID = 4326 };

        await _officeService.UpdateAsync(office);
        TempData["Success"] = "تم تحديث بيانات الدائرة بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var office = await _officeService.GetByIdAsync(id);
        if (office != null)
        {
            office.IsDeleted = true;
            await _officeService.UpdateAsync(office);
        }
        TempData["Success"] = "تم حذف الدائرة بنجاح";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadViewDataAsync()
    {
        ViewBag.Provinces = new SelectList(await _unitOfWork.Provinces.GetAllAsync(), "Id", "NameAr");
        ViewBag.OfficeTypes = new SelectList(await _unitOfWork.OfficeTypes.GetAllAsync(), "Id", "NameAr");
        ViewBag.WaqfOffices = new SelectList(await _unitOfWork.WaqfOffices.GetAllAsync(), "Id", "NameAr");
        ViewBag.Districts = new SelectList(await _unitOfWork.Districts.GetAllAsync(), "Id", "NameAr");
    }

    // ========== تصدير Excel ==========
    [HttpGet]
    public async Task<IActionResult> Export(int? provinceId, int? typeId)
    {
        var offices = await _unitOfWork.WaqfOffices.Query()
            .Include(o => o.OfficeType).Include(o => o.Province).Include(o => o.ParentOffice)
            .ToListAsync();

        if (provinceId.HasValue)
            offices = offices.Where(o => o.ProvinceId == provinceId.Value).ToList();
        if (typeId.HasValue)
            offices = offices.Where(o => o.OfficeTypeId == typeId.Value).ToList();

        var fileContent = _excelExportService.ExportOfficesToExcel(offices);
        var fileName = $"الدوائر_الوقفية_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        
        return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}
