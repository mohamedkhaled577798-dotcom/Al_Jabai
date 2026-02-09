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
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var stats = await _reportService.GetDashboardStatisticsAsync();
        
        // إضافة إحصائيات العقود والدعاوى
        var contracts = await _unitOfWork.Repository<InvestmentContract>().GetAllAsync();
        var disputes = await _unitOfWork.Repository<LegalDispute>().GetAllAsync();
        var services = await _unitOfWork.Repository<ServiceFacility>().GetAllAsync();
        
        ViewBag.ActiveContracts = contracts.Count(c => c.IsActive);
        ViewBag.ExpiringContracts = contracts.Count(c => c.IsActive && c.EndDate <= DateTime.Now.AddMonths(3));
        ViewBag.ActiveDisputes = disputes.Count(d => d.CaseStatus == "جارية");
        ViewBag.TotalServices = services.Count();
        
        return View(stats);
    }

    [AllowAnonymous]
    public IActionResult Error()
    {
        return View();
    }
}
