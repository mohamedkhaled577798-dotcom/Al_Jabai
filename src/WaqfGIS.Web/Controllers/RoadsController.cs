using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using WaqfGIS.Core.Entities;
using WaqfGIS.Core.Interfaces;
using WaqfGIS.Services;
using WaqfGIS.Services.GIS;

namespace WaqfGIS.Web.Controllers;

[Authorize]
[Route("Roads")]
public class RoadsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly GeometryService _geometryService;
    private readonly AuditLogService _auditLogService;

    public RoadsController(IUnitOfWork unitOfWork, GeometryService geometryService, AuditLogService auditLogService)
    {
        _unitOfWork = unitOfWork;
        _geometryService = geometryService;
        _auditLogService = auditLogService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(int? provinceId, string? roadType, string? search)
    {
        var query = _unitOfWork.Repository<Road>().Query()
            .Include(r => r.Province)
            .AsQueryable();

        if (provinceId.HasValue)
            query = query.Where(r => r.ProvinceId == provinceId);
        if (!string.IsNullOrEmpty(roadType))
            query = query.Where(r => r.RoadType == roadType);
        if (!string.IsNullOrEmpty(search))
            query = query.Where(r => r.NameAr.Contains(search) || r.Code.Contains(search));

        var roads = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();

        ViewBag.Provinces = new SelectList(await _unitOfWork.Provinces.GetAllAsync(), "Id", "NameAr", provinceId);
        ViewBag.RoadTypes = GetRoadTypes();
        ViewBag.CurrentProvince = provinceId;
        ViewBag.CurrentRoadType = roadType;
        ViewBag.CurrentSearch = search;

        return View(roads);
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        await LoadViewData();
        return View(new RoadViewModel());
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RoadViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadViewData();
            return View(model);
        }

        var road = new Road
        {
            Code = await GenerateCodeAsync(),
            NameAr = model.NameAr,
            NameEn = model.NameEn,
            ProvinceId = model.ProvinceId,
            DistrictId = model.DistrictId,
            RoadType = model.RoadType ?? "Local",
            WidthMeters = model.WidthMeters,
            LanesCount = model.LanesCount,
            SurfaceType = model.SurfaceType,
            Notes = model.Notes,
            CreatedBy = User.Identity?.Name
        };

        if (!string.IsNullOrEmpty(model.GeometryGeoJson))
        {
            var geometry = _geometryService.FromGeoJson(model.GeometryGeoJson);
            if (geometry is LineString lineString)
            {
                road.Geometry = lineString;
                road.LengthMeters = _geometryService.CalculatePerimeterMeters(lineString);
            }
        }

        await _unitOfWork.Repository<Road>().AddAsync(road);
        await _unitOfWork.SaveChangesAsync();

        await _auditLogService.LogCreateAsync("Road", road.Id, road.NameAr,
            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, 
            User.Identity?.Name,
            HttpContext.Connection.RemoteIpAddress?.ToString());

        TempData["Success"] = "تم إضافة الطريق بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var road = await _unitOfWork.Repository<Road>().GetByIdAsync(id);
        if (road == null) return NotFound();

        var model = new RoadViewModel
        {
            Id = road.Id,
            Code = road.Code,
            NameAr = road.NameAr,
            NameEn = road.NameEn,
            ProvinceId = road.ProvinceId,
            DistrictId = road.DistrictId,
            RoadType = road.RoadType,
            WidthMeters = road.WidthMeters,
            LanesCount = road.LanesCount,
            SurfaceType = road.SurfaceType,
            LengthMeters = road.LengthMeters,
            Notes = road.Notes,
            GeometryGeoJson = road.Geometry != null ? _geometryService.ToGeoJson(road.Geometry) : null
        };

        await LoadViewData();
        return View(model);
    }

    [HttpPost("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, RoadViewModel model)
    {
        if (id != model.Id) return NotFound();

        var road = await _unitOfWork.Repository<Road>().GetByIdAsync(id);
        if (road == null) return NotFound();

        if (!ModelState.IsValid)
        {
            await LoadViewData();
            return View(model);
        }

        road.NameAr = model.NameAr;
        road.NameEn = model.NameEn;
        road.ProvinceId = model.ProvinceId;
        road.DistrictId = model.DistrictId;
        road.RoadType = model.RoadType ?? "Local";
        road.WidthMeters = model.WidthMeters;
        road.LanesCount = model.LanesCount;
        road.SurfaceType = model.SurfaceType;
        road.Notes = model.Notes;
        road.UpdatedBy = User.Identity?.Name;

        if (!string.IsNullOrEmpty(model.GeometryGeoJson))
        {
            var geometry = _geometryService.FromGeoJson(model.GeometryGeoJson);
            if (geometry is LineString lineString)
            {
                road.Geometry = lineString;
                road.LengthMeters = _geometryService.CalculatePerimeterMeters(lineString);
            }
        }

        await _unitOfWork.SaveChangesAsync();

        await _auditLogService.LogUpdateAsync("Road", road.Id, road.NameAr,
            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, 
            User.Identity?.Name,
            null, null,
            HttpContext.Connection.RemoteIpAddress?.ToString());

        TempData["Success"] = "تم تحديث الطريق بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Details/{id}")]
    public async Task<IActionResult> Details(int id)
    {
        var road = await _unitOfWork.Repository<Road>().Query()
            .Include(r => r.Province)
            .Include(r => r.District)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (road == null) return NotFound();

        ViewBag.GeometryGeoJson = road.Geometry != null ? _geometryService.ToGeoJson(road.Geometry) : null;
        return View(road);
    }

    [HttpGet("Delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var road = await _unitOfWork.Repository<Road>().Query()
            .Include(r => r.Province)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (road == null) return NotFound();
        return View(road);
    }

    [HttpPost("Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var road = await _unitOfWork.Repository<Road>().GetByIdAsync(id);
        if (road == null) return NotFound();

        road.IsDeleted = true;
        road.UpdatedBy = User.Identity?.Name;
        await _unitOfWork.SaveChangesAsync();

        await _auditLogService.LogDeleteAsync("Road", road.Id, road.NameAr,
            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, 
            User.Identity?.Name,
            HttpContext.Connection.RemoteIpAddress?.ToString());

        TempData["Success"] = "تم حذف الطريق بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Map")]
    public async Task<IActionResult> Map()
    {
        ViewBag.Provinces = new SelectList(await _unitOfWork.Provinces.GetAllAsync(), "Id", "NameAr");
        return View();
    }

    private async Task LoadViewData()
    {
        ViewBag.Provinces = new SelectList(await _unitOfWork.Provinces.GetAllAsync(), "Id", "NameAr");
        ViewBag.Districts = new SelectList(await _unitOfWork.Districts.GetAllAsync(), "Id", "NameAr");
        ViewBag.RoadTypes = GetRoadTypes();
        ViewBag.SurfaceTypes = GetSurfaceTypes();
    }

    private SelectList GetRoadTypes()
    {
        var types = new[] {
            new { Value = "Highway", Text = "طريق سريع" },
            new { Value = "Main", Text = "رئيسي" },
            new { Value = "Secondary", Text = "فرعي" },
            new { Value = "Local", Text = "محلي" },
            new { Value = "Alley", Text = "زقاق" }
        };
        return new SelectList(types, "Value", "Text");
    }

    private SelectList GetSurfaceTypes()
    {
        var types = new[] {
            new { Value = "Asphalt", Text = "إسفلت" },
            new { Value = "Concrete", Text = "خرسانة" },
            new { Value = "Gravel", Text = "حصى" },
            new { Value = "Unpaved", Text = "غير معبد" }
        };
        return new SelectList(types, "Value", "Text");
    }

    private async Task<string> GenerateCodeAsync()
    {
        var count = await _unitOfWork.Repository<Road>().Query().CountAsync();
        return $"RD-{DateTime.Now:yyyyMM}-{(count + 1):D4}";
    }
}

public class RoadViewModel
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public int? ProvinceId { get; set; }
    public int? DistrictId { get; set; }
    public string? RoadType { get; set; }
    public double? WidthMeters { get; set; }
    public int? LanesCount { get; set; }
    public string? SurfaceType { get; set; }
    public double? LengthMeters { get; set; }
    public string? Notes { get; set; }
    public string? GeometryGeoJson { get; set; }
}
