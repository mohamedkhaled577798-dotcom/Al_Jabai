using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AlJabai.Models;
using WaqfGIS.Services.GIS;
using Microsoft.EntityFrameworkCore;
using WaqfGIS.Infrastructure.Data;

namespace AlJabai.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly GeometryService _geometryService;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, GeometryService geometryService, ApplicationDbContext context)
    {
        _logger = logger;
        _geometryService = geometryService;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // Example of using GIS service in AlJabai project
        var mosqueCount = await _context.Mosques.CountAsync();
        ViewBag.Message = $"Welcome to AlJabai System. Connected to GIS Database. Total Mosques: {mosqueCount}";
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
