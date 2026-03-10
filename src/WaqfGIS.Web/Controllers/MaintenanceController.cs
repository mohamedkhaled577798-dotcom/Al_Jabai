using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WaqfGIS.Core.Entities;
using WaqfGIS.Core.Interfaces;
using WaqfGIS.Services;

namespace WaqfGIS.Web.Controllers;

[Authorize]
public class MaintenanceController : Controller
{
    private readonly MaintenanceService _maintenanceService;
    private readonly IUnitOfWork _unitOfWork;

    public MaintenanceController(MaintenanceService maintenanceService, IUnitOfWork unitOfWork)
    {
        _maintenanceService = maintenanceService;
        _unitOfWork = unitOfWork;
    }

    // =================== Index ===================
    public async Task<IActionResult> Index(string? status, string? entityType, int? provinceId)
    {
        var records = await _maintenanceService.GetAllAsync(provinceId, entityType, status);
        var summary = await _maintenanceService.GetSummaryAsync(provinceId);

        ViewBag.Summary    = summary;
        ViewBag.Provinces  = new SelectList(await _unitOfWork.Provinces.GetAllAsync(), "Id", "NameAr", provinceId);
        ViewBag.StatusFilter = status;
        ViewBag.TypeFilter   = entityType;
        ViewBag.ProvinceFilter = provinceId;
        return View(records);
    }

    // =================== Upcoming Schedule ===================
    public async Task<IActionResult> Schedule(int? provinceId)
    {
        var upcoming = await _maintenanceService.GetUpcomingAsync(90, provinceId);
        var overdue  = await _maintenanceService.GetOverdueAsync(provinceId);
        ViewBag.Upcoming  = upcoming;
        ViewBag.Overdue   = overdue;
        ViewBag.Provinces = new SelectList(await _unitOfWork.Provinces.GetAllAsync(), "Id", "NameAr", provinceId);
        return View();
    }

    // =================== Details ===================
    public async Task<IActionResult> Details(int id)
    {
        var record = await _maintenanceService.GetByIdAsync(id);
        if (record == null) return NotFound();
        return View(record);
    }

    // =================== Create ===================
    [HttpGet]
    public async Task<IActionResult> Create(string? entityType, int? entityId, string? entityName)
    {
        var record = new MaintenanceRecord
        {
            EntityType    = entityType ?? "",
            EntityId      = entityId ?? 0,
            EntityName    = entityName,
            ScheduledDate = DateTime.Today.AddDays(7),
            Priority      = "عادية",
            Status        = "مجدولة"
        };
        await PopulateViewBag();
        return View(record);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MaintenanceRecord record, List<IFormFile>? Photos)
    {
        // تحقق من الحقول الأساسية فقط
        ModelState.Remove("Province");
        ModelState.Remove("Photos");
        ModelState.Remove("EntityName");

        if (!ModelState.IsValid)
        {
            await PopulateViewBag();
            return View(record);
        }

        record.CreatedBy = User.Identity?.Name;
        await _maintenanceService.CreateAsync(record, Photos);
        TempData["Success"] = "تم إضافة سجل الصيانة بنجاح";
        return RedirectToAction(nameof(Details), new { id = record.Id });
    }

    // =================== Edit ===================
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var record = await _maintenanceService.GetByIdAsync(id);
        if (record == null) return NotFound();
        await PopulateViewBag();
        return View(record);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MaintenanceRecord record,
        List<IFormFile>? NewPhotos, string? PhotoType)
    {
        if (id != record.Id) return BadRequest();
        ModelState.Remove("Province");
        ModelState.Remove("Photos");
        ModelState.Remove("EntityName");

        if (!ModelState.IsValid)
        {
            await PopulateViewBag();
            record.Photos = (await _maintenanceService.GetByIdAsync(id))?.Photos
                            ?? new List<MaintenancePhoto>();
            return View(record);
        }

        record.UpdatedBy = User.Identity?.Name;
        await _maintenanceService.UpdateAsync(record, NewPhotos, PhotoType ?? "بعد");
        TempData["Success"] = "تم تحديث سجل الصيانة";
        return RedirectToAction(nameof(Details), new { id = record.Id });
    }

    // =================== Delete ===================
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _maintenanceService.DeleteAsync(id);
        TempData["Success"] = "تم حذف السجل";
        return RedirectToAction(nameof(Index));
    }

    // =================== Delete Photo ===================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePhoto(int photoId, int maintenanceId)
    {
        await _maintenanceService.DeletePhotoAsync(photoId);
        return RedirectToAction(nameof(Edit), new { id = maintenanceId });
    }

    // =================== Complete ===================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkComplete(int id, string? workDone, decimal? actualCost)
    {
        var record = await _maintenanceService.GetByIdAsync(id);
        if (record == null) return NotFound();

        record.Status         = "مكتملة";
        record.CompletionDate = DateTime.Now;
        record.WorkDone       = workDone;
        record.ActualCost     = actualCost ?? record.ActualCost;
        record.UpdatedBy      = User.Identity?.Name;

        await _maintenanceService.UpdateAsync(record);
        TempData["Success"] = "تم تسجيل اكتمال الصيانة";
        return RedirectToAction(nameof(Details), new { id });
    }

    // =================== Helper ===================
    private async Task PopulateViewBag()
    {
        ViewBag.Provinces = new SelectList(await _unitOfWork.Provinces.GetAllAsync(), "Id", "NameAr");

        ViewBag.MaintenanceTypes = new List<string>
        {
            "كهرباء", "سباكة", "هيكل وبناء", "تشطيب وديكور", "تكييف",
            "نوافذ وأبواب", "سقف وأسطح", "حدائق ومناظر", "طوارئ", "دورية", "شاملة", "أخرى"
        };
        ViewBag.Priorities = new List<string> { "عاجلة", "عالية", "عادية", "منخفضة" };
        ViewBag.Statuses   = new List<string> { "مجدولة", "جارية", "مكتملة", "ملغاة", "متأخرة" };
        ViewBag.PhotoTypes = new List<string> { "قبل", "أثناء", "بعد" };
        ViewBag.Recurrences= new List<string> { "شهري", "ربع سنوي", "نصف سنوي", "سنوي" };
    }
}
