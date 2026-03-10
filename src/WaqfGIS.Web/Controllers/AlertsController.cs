using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WaqfGIS.Services;

namespace WaqfGIS.Web.Controllers;

[Authorize]
public class AlertsController : Controller
{
    private readonly AlertService _alertService;

    public AlertsController(AlertService alertService)
    {
        _alertService = alertService;
    }

    // GET: /Alerts  — صفحة التنبيهات الرئيسية
    public async Task<IActionResult> Index(string? type = null, string? severity = null)
    {
        var summary = await _alertService.GetAlertSummaryAsync();
        var alerts  = await _alertService.GetActiveAlertsAsync(limit: 200);

        if (!string.IsNullOrEmpty(type))
            alerts = alerts.Where(a => a.AlertType == type).ToList();
        if (!string.IsNullOrEmpty(severity))
            alerts = alerts.Where(a => a.Severity == severity).ToList();

        ViewBag.Summary  = summary;
        ViewBag.Filter   = type;
        ViewBag.Severity = severity;
        return View(alerts);
    }

    // POST: /Alerts/MarkRead/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(int id)
    {
        await _alertService.MarkAsReadAsync(id, User.Identity?.Name ?? "");
        return RedirectToAction(nameof(Index));
    }

    // POST: /Alerts/MarkAllRead
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllRead()
    {
        await _alertService.MarkAllAsReadAsync(null, User.Identity?.Name ?? "");
        return RedirectToAction(nameof(Index));
    }

    // POST: /Alerts/Dismiss/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Dismiss(int id)
    {
        await _alertService.DismissAlertAsync(id);
        return RedirectToAction(nameof(Index));
    }

    // POST: /Alerts/GenerateNow  — تشغيل يدوي لتوليد التنبيهات
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GenerateNow()
    {
        await _alertService.GenerateAllAlertsAsync();
        TempData["Success"] = "تم توليد التنبيهات بنجاح";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Alerts/Summary — API للـ bell icon في الـ Layout
    [HttpGet]
    public async Task<IActionResult> Summary()
    {
        var summary = await _alertService.GetAlertSummaryAsync();
        return Json(summary);
    }
}
