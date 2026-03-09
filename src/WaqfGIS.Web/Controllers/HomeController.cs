using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WaqfGIS.Core.Entities;
using WaqfGIS.Core.Interfaces;
using WaqfGIS.Services;

namespace WaqfGIS.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ReportService _reportService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ReportService reportService, IUnitOfWork unitOfWork, ILogger<HomeController> logger)
    {
        _reportService = reportService;
        _unitOfWork    = unitOfWork;
        _logger        = logger;
    }

    public async Task<IActionResult> Index()
    {
        var stats = await _reportService.GetDashboardStatisticsAsync();
        var adv   = await _reportService.GetAdvancedDashboardAsync();
        ViewBag.Adv = adv;
        return View(stats);
    }

    [AllowAnonymous]
    public IActionResult Error() => View();
}
