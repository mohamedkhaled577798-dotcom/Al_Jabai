using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WaqfGIS.Services;

namespace WaqfGIS.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ReportService _reportService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ReportService reportService, ILogger<HomeController> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var stats = await _reportService.GetDashboardStatisticsAsync();
        return View(stats);
    }

    [AllowAnonymous]
    public IActionResult Error()
    {
        return View();
    }
}
