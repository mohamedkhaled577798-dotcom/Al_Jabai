using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WaqfGIS.Core.Interfaces;
using WaqfGIS.Services;

namespace WaqfGIS.Web.Controllers;

[Authorize]
public class MapController : Controller
{
    private readonly MosqueService _mosqueService;
    private readonly PropertyService _propertyService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ExcelExportService _excelExportService;

    public MapController(
        MosqueService mosqueService, 
        PropertyService propertyService,
        IUnitOfWork unitOfWork,
        ExcelExportService excelExportService)
    {
        _mosqueService = mosqueService;
        _propertyService = propertyService;
        _unitOfWork = unitOfWork;
        _excelExportService = excelExportService;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> ExportAll()
    {
        var mosques = await _unitOfWork.Mosques.Query()
            .Include(m => m.MosqueType).Include(m => m.Province)
            .ToListAsync();
        
        var properties = await _unitOfWork.WaqfProperties.Query()
            .Include(p => p.PropertyType).Include(p => p.Province)
            .ToListAsync();

        var offices = await _unitOfWork.WaqfOffices.Query()
            .Include(o => o.OfficeType).Include(o => o.Province)
            .ToListAsync();

        var fileContent = _excelExportService.ExportAllDataToExcel(mosques, properties, offices);
        var fileName = $"بيانات_الخريطة_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        
        return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    [HttpGet]
    public async Task<IActionResult> ExportGeoJson()
    {
        var mosques = await _mosqueService.GetForMapAsync();
        var properties = await _propertyService.GetForMapAsync();

        var geoJson = new
        {
            type = "FeatureCollection",
            features = mosques.Select(m => new
            {
                type = "Feature",
                geometry = new { type = "Point", coordinates = new[] { m.GetType().GetProperty("Longitude")?.GetValue(m), m.GetType().GetProperty("Latitude")?.GetValue(m) } },
                properties = m
            }).Concat(properties.Select(p => new
            {
                type = "Feature",
                geometry = new { type = "Point", coordinates = new[] { p.GetType().GetProperty("Longitude")?.GetValue(p), p.GetType().GetProperty("Latitude")?.GetValue(p) } },
                properties = p
            }))
        };

        var json = System.Text.Json.JsonSerializer.Serialize(geoJson);
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        
        return File(bytes, "application/geo+json", $"خريطة_الاوقاف_{DateTime.Now:yyyyMMdd}.geojson");
    }
}
