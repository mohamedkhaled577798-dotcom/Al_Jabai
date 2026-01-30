using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WaqfGIS.Core.Entities;
using WaqfGIS.Core.Interfaces;
using WaqfGIS.Services;

namespace WaqfGIS.Web.Controllers;

[Authorize]
public class DistrictsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AuditLogService _auditLogService;

    public DistrictsController(IUnitOfWork unitOfWork, AuditLogService auditLogService)
    {
        _unitOfWork = unitOfWork;
        _auditLogService = auditLogService;
    }

    public async Task<IActionResult> Index(int? provinceId, string? search)
    {
        var query = _unitOfWork.Districts.Query()
            .Include(d => d.Province)
            .AsQueryable();

        if (provinceId.HasValue)
            query = query.Where(d => d.ProvinceId == provinceId);
        
        if (!string.IsNullOrEmpty(search))
            query = query.Where(d => d.NameAr.Contains(search) || d.Code.Contains(search));

        var districts = await query.OrderBy(d => d.Province.NameAr).ThenBy(d => d.NameAr).ToListAsync();

        ViewBag.Provinces = new SelectList(await _unitOfWork.Provinces.GetAllAsync(), "Id", "NameAr", provinceId);
        ViewBag.CurrentProvince = provinceId;
        ViewBag.CurrentSearch = search;

        return View(districts);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Provinces = new SelectList(await _unitOfWork.Provinces.GetAllAsync(), "Id", "NameAr");
        return View(new District());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(District model)
    {
        if (string.IsNullOrEmpty(model.NameAr))
        {
            ModelState.AddModelError("NameAr", "الاسم بالعربية مطلوب");
            ViewBag.Provinces = new SelectList(await _unitOfWork.Provinces.GetAllAsync(), "Id", "NameAr");
            return View(model);
        }

        // Generate code
        var province = await _unitOfWork.Provinces.GetByIdAsync(model.ProvinceId);
        var count = await _unitOfWork.Districts.Query().CountAsync(d => d.ProvinceId == model.ProvinceId);
        model.Code = $"{province?.Code ?? "XX"}-D{(count + 1):D2}";
        model.CreatedBy = User.Identity?.Name;

        await _unitOfWork.Districts.AddAsync(model);
        await _unitOfWork.SaveChangesAsync();

        await _auditLogService.LogCreateAsync("District", model.Id, model.NameAr,
            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
            User.Identity?.Name, HttpContext.Connection.RemoteIpAddress?.ToString());

        TempData["Success"] = "تم إضافة القضاء بنجاح";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var district = await _unitOfWork.Districts.GetByIdAsync(id);
        if (district == null) return NotFound();

        ViewBag.Provinces = new SelectList(await _unitOfWork.Provinces.GetAllAsync(), "Id", "NameAr", district.ProvinceId);
        return View(district);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, District model)
    {
        if (id != model.Id) return NotFound();

        var district = await _unitOfWork.Districts.GetByIdAsync(id);
        if (district == null) return NotFound();

        if (string.IsNullOrEmpty(model.NameAr))
        {
            ModelState.AddModelError("NameAr", "الاسم بالعربية مطلوب");
            ViewBag.Provinces = new SelectList(await _unitOfWork.Provinces.GetAllAsync(), "Id", "NameAr", model.ProvinceId);
            return View(model);
        }

        var oldValues = $"الاسم: {district.NameAr}, المحافظة: {district.ProvinceId}";

        district.NameAr = model.NameAr;
        district.NameEn = model.NameEn;
        district.ProvinceId = model.ProvinceId;
        district.UpdatedBy = User.Identity?.Name;

        await _unitOfWork.SaveChangesAsync();

        var newValues = $"الاسم: {district.NameAr}, المحافظة: {district.ProvinceId}";
        await _auditLogService.LogUpdateAsync("District", district.Id, district.NameAr,
            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
            User.Identity?.Name, oldValues, newValues, HttpContext.Connection.RemoteIpAddress?.ToString());

        TempData["Success"] = "تم تحديث القضاء بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var district = await _unitOfWork.Districts.GetByIdAsync(id);
        if (district == null) return NotFound();

        // Check if has mosques or properties
        var hasMosques = await _unitOfWork.Mosques.Query().AnyAsync(m => m.DistrictId == id);
        var hasProperties = await _unitOfWork.WaqfProperties.Query().AnyAsync(p => p.DistrictId == id);

        if (hasMosques || hasProperties)
        {
            TempData["Error"] = "لا يمكن حذف القضاء لوجود مساجد أو عقارات مرتبطة به";
            return RedirectToAction(nameof(Index));
        }

        district.IsDeleted = true;
        district.UpdatedBy = User.Identity?.Name;
        await _unitOfWork.SaveChangesAsync();

        await _auditLogService.LogDeleteAsync("District", district.Id, district.NameAr,
            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
            User.Identity?.Name, HttpContext.Connection.RemoteIpAddress?.ToString());

        TempData["Success"] = "تم حذف القضاء بنجاح";
        return RedirectToAction(nameof(Index));
    }

    // API للـ Cascading Dropdown
    [HttpGet]
    public async Task<IActionResult> GetByProvince(int provinceId)
    {
        var districts = await _unitOfWork.Districts.Query()
            .Where(d => d.ProvinceId == provinceId)
            .OrderBy(d => d.NameAr)
            .Select(d => new { d.Id, d.NameAr })
            .ToListAsync();

        return Json(districts);
    }
}
