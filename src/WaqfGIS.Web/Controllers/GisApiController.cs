using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using WaqfGIS.Core.Entities;
using WaqfGIS.Core.Enums;
using WaqfGIS.Core.Interfaces;
using WaqfGIS.Services.GIS;

namespace WaqfGIS.Web.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class GisApiController : ControllerBase
{
    private readonly GeometryService _geometryService;
    private readonly SpatialAnalysisService _spatialAnalysisService;
    private readonly LayerService _layerService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;

    public GisApiController(
        GeometryService geometryService,
        SpatialAnalysisService spatialAnalysisService,
        LayerService layerService,
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager)
    {
        _geometryService = geometryService;
        _spatialAnalysisService = spatialAnalysisService;
        _layerService = layerService;
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    /// <summary>
    /// يحدد معرف المحافظة للمستخدم بناءً على صلاحيته
    /// إذا كان SuperAdmin/Admin يرى كل شيء، غيره يرى محافظته فقط
    /// </summary>
    private async Task<int?> GetEffectiveProvinceFilter(int? requestedProvinceId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return requestedProvinceId;

        // SuperAdmin و Admin يرون كل شيء
        if (currentUser.PermissionLevel == PermissionLevel.SuperAdmin ||
            currentUser.PermissionLevel == PermissionLevel.Admin)
        {
            return requestedProvinceId; // بدون قيود
        }

        // مدير محافظة أو مستخدم محافظة - مقيّد بمحافظته دائماً
        if (currentUser.ProvinceId.HasValue)
        {
            return currentUser.ProvinceId.Value;
        }

        return requestedProvinceId;
    }

    #region Layer Endpoints

    [HttpGet("layers")]
    public async Task<IActionResult> GetLayers()
    {
        var layers = await _layerService.GetAllLayersAsync();
        return Ok(layers);
    }

    [HttpGet("layers/{layerCode}/geojson")]
    public async Task<IActionResult> GetLayerGeoJson(string layerCode, [FromQuery] int? provinceId)
    {
        // تطبيق فلتر الصلاحيات: المستخدم العادي محدود بمحافظته تلقائياً
        var effectiveProvinceId = await GetEffectiveProvinceFilter(provinceId);

        object? result = layerCode.ToLower() switch
        {
            "mosques" => await _layerService.GetMosquesGeoJson(effectiveProvinceId),
            "mosqueboundaries" => await _layerService.GetMosqueBoundariesGeoJson(effectiveProvinceId),
            "properties" => await _layerService.GetPropertiesGeoJson(effectiveProvinceId),
            "propertyboundaries" => await _layerService.GetPropertyBoundariesGeoJson(effectiveProvinceId),
            "waqflands" or "lands" => await _layerService.GetWaqfLandsGeoJson(effectiveProvinceId),
            "roads" => await _layerService.GetRoadsGeoJson(effectiveProvinceId),
            "projects" => await _layerService.GetNearbyProjectsGeoJson(effectiveProvinceId),
            "all" => await _layerService.GetAllLayersGeoJson(effectiveProvinceId),
            _ => null
        };

        if (result == null)
            return NotFound(new { error = "Layer not found" });

        return Ok(result);
    }

    [HttpPut("layers/{id}/style")]
    public async Task<IActionResult> UpdateLayerStyle(int id, [FromBody] LayerStyleDto style)
    {
        await _layerService.UpdateLayerStyle(id, style.FillColor, style.FillOpacity,
            style.StrokeColor, style.StrokeWidth);
        return Ok(new { success = true });
    }

    #endregion

    #region Mosque Boundary Endpoints

    [HttpPost("mosques/{id}/boundary")]
    public async Task<IActionResult> SaveMosqueBoundary(int id, [FromBody] GeometryDto dto)
    {
        var mosque = await _unitOfWork.Mosques.GetByIdAsync(id);
        if (mosque == null)
            return NotFound(new { error = "Mosque not found" });

        var geometry = _geometryService.FromGeoJson(dto.GeoJson);
        if (geometry == null || !(geometry is Polygon polygon))
            return BadRequest(new { error = "Invalid polygon geometry" });

        if (!_geometryService.IsValid(polygon))
            polygon = (Polygon?)_geometryService.MakeValid(polygon);

        var existingBoundary = await _unitOfWork.Repository<MosqueBoundary>().Query()
            .FirstOrDefaultAsync(b => b.MosqueId == id && b.BoundaryType == (dto.BoundaryType ?? "Building"));

        if (existingBoundary != null)
        {
            await LogGeometryChange("MosqueBoundary", existingBoundary.Id, mosque.NameAr,
                "Modified", existingBoundary.Boundary, polygon);

            existingBoundary.Boundary = polygon;
            existingBoundary.CalculatedAreaSqm = _geometryService.CalculateAreaSquareMeters(polygon);
            existingBoundary.PerimeterMeters = _geometryService.CalculatePerimeterMeters(polygon);
            existingBoundary.UpdatedBy = User.Identity?.Name;
        }
        else
        {
            var boundary = new MosqueBoundary
            {
                MosqueId = id,
                Boundary = polygon,
                BoundaryType = dto.BoundaryType ?? "Building",
                CalculatedAreaSqm = _geometryService.CalculateAreaSquareMeters(polygon),
                PerimeterMeters = _geometryService.CalculatePerimeterMeters(polygon),
                CreatedBy = User.Identity?.Name
            };

            await _unitOfWork.Repository<MosqueBoundary>().AddAsync(boundary);
        }

        await _unitOfWork.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            areaSqm = _geometryService.CalculateAreaSquareMeters(polygon),
            perimeterMeters = _geometryService.CalculatePerimeterMeters(polygon)
        });
    }

    [HttpGet("mosques/{id}/boundary")]
    public async Task<IActionResult> GetMosqueBoundary(int id)
    {
        var boundaries = await _unitOfWork.Repository<MosqueBoundary>().Query()
            .Where(b => b.MosqueId == id)
            .ToListAsync();

        return Ok(boundaries.Select(b => new
        {
            id = b.Id,
            boundaryType = b.BoundaryType,
            geoJson = _geometryService.ToGeoJson(b.Boundary),
            areaSqm = b.CalculatedAreaSqm,
            perimeterMeters = b.PerimeterMeters
        }));
    }

    [HttpDelete("mosques/{mosqueId}/boundary/{boundaryId}")]
    public async Task<IActionResult> DeleteMosqueBoundary(int mosqueId, int boundaryId)
    {
        var boundary = await _unitOfWork.Repository<MosqueBoundary>().GetByIdAsync(boundaryId);
        if (boundary == null || boundary.MosqueId != mosqueId)
            return NotFound();

        await _unitOfWork.Repository<MosqueBoundary>().DeleteAsync(boundary);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { success = true });
    }

    #endregion

    #region Property Boundary Endpoints

    [HttpPost("properties/{id}/boundary")]
    public async Task<IActionResult> SavePropertyBoundary(int id, [FromBody] GeometryDto dto)
    {
        var property = await _unitOfWork.WaqfProperties.GetByIdAsync(id);
        if (property == null)
            return NotFound(new { error = "Property not found" });

        var geometry = _geometryService.FromGeoJson(dto.GeoJson);
        if (geometry == null || !(geometry is Polygon polygon))
            return BadRequest(new { error = "Invalid polygon geometry" });

        if (!_geometryService.IsValid(polygon))
            polygon = (Polygon?)_geometryService.MakeValid(polygon);

        var existingBoundary = await _unitOfWork.Repository<PropertyBoundary>().Query()
            .FirstOrDefaultAsync(b => b.PropertyId == id && b.BoundaryType == (dto.BoundaryType ?? "Building"));

        if (existingBoundary != null)
        {
            await LogGeometryChange("PropertyBoundary", existingBoundary.Id, property.NameAr,
                "Modified", existingBoundary.Boundary, polygon);

            existingBoundary.Boundary = polygon;
            existingBoundary.CalculatedAreaSqm = _geometryService.CalculateAreaSquareMeters(polygon);
            existingBoundary.PerimeterMeters = _geometryService.CalculatePerimeterMeters(polygon);
            existingBoundary.UpdatedBy = User.Identity?.Name;
        }
        else
        {
            var boundary = new PropertyBoundary
            {
                PropertyId = id,
                Boundary = polygon,
                BoundaryType = dto.BoundaryType ?? "Building",
                CalculatedAreaSqm = _geometryService.CalculateAreaSquareMeters(polygon),
                PerimeterMeters = _geometryService.CalculatePerimeterMeters(polygon),
                CreatedBy = User.Identity?.Name
            };

            await _unitOfWork.Repository<PropertyBoundary>().AddAsync(boundary);
        }

        // Update property area
        property.TotalArea = (decimal?)_geometryService.CalculateAreaSquareMeters(polygon);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            areaSqm = _geometryService.CalculateAreaSquareMeters(polygon),
            perimeterMeters = _geometryService.CalculatePerimeterMeters(polygon)
        });
    }

    [HttpGet("properties/{id}/boundary")]
    public async Task<IActionResult> GetPropertyBoundary(int id)
    {
        var boundaries = await _unitOfWork.Repository<PropertyBoundary>().Query()
            .Where(b => b.PropertyId == id)
            .ToListAsync();

        return Ok(boundaries.Select(b => new
        {
            id = b.Id,
            boundaryType = b.BoundaryType,
            geoJson = _geometryService.ToGeoJson(b.Boundary),
            areaSqm = b.CalculatedAreaSqm,
            perimeterMeters = b.PerimeterMeters
        }));
    }

    [HttpDelete("properties/{propertyId}/boundary/{boundaryId}")]
    public async Task<IActionResult> DeletePropertyBoundary(int propertyId, int boundaryId)
    {
        var boundary = await _unitOfWork.Repository<PropertyBoundary>().GetByIdAsync(boundaryId);
        if (boundary == null || boundary.PropertyId != propertyId)
            return NotFound();

        await _unitOfWork.Repository<PropertyBoundary>().DeleteAsync(boundary);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { success = true });
    }

    #endregion

    #region Waqf Land Endpoints

    [HttpPost("lands")]
    public async Task<IActionResult> SaveWaqfLand([FromBody] WaqfLandDto dto)
    {
        var geometry = _geometryService.FromGeoJson(dto.BoundaryGeoJson);

        WaqfLand land;
        if (dto.Id.HasValue)
        {
            land = await _unitOfWork.Repository<WaqfLand>().GetByIdAsync(dto.Id.Value);
            if (land == null) return NotFound();

            land.NameAr = dto.NameAr;
            land.NameEn = dto.NameEn;
            land.ProvinceId = dto.ProvinceId;
            land.WaqfOfficeId = dto.WaqfOfficeId;
            land.LandType = dto.LandType ?? "Waqf";
            land.LandUse = dto.LandUse ?? "Vacant";
            land.Boundary = geometry;
            land.CenterPoint = _geometryService.CalculateCentroid(geometry);
            land.CalculatedAreaSqm = _geometryService.CalculateAreaSquareMeters(geometry);
            land.PerimeterMeters = _geometryService.CalculatePerimeterMeters(geometry);
            land.LegalAreaSqm = dto.LegalAreaSqm;
            land.DeedNumber = dto.DeedNumber;
            land.Notes = dto.Notes;
            land.UpdatedBy = User.Identity?.Name;
        }
        else
        {
            land = new WaqfLand
            {
                Code = GenerateCode("WL"),
                NameAr = dto.NameAr,
                NameEn = dto.NameEn,
                ProvinceId = dto.ProvinceId,
                WaqfOfficeId = dto.WaqfOfficeId,
                LandType = dto.LandType ?? "Waqf",
                LandUse = dto.LandUse ?? "Vacant",
                Boundary = geometry,
                CenterPoint = _geometryService.CalculateCentroid(geometry),
                CalculatedAreaSqm = _geometryService.CalculateAreaSquareMeters(geometry),
                PerimeterMeters = _geometryService.CalculatePerimeterMeters(geometry),
                LegalAreaSqm = dto.LegalAreaSqm,
                DeedNumber = dto.DeedNumber,
                Notes = dto.Notes,
                CreatedBy = User.Identity?.Name
            };

            await _unitOfWork.Repository<WaqfLand>().AddAsync(land);
        }

        await _unitOfWork.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            id = land.Id,
            code = land.Code,
            areaSqm = land.CalculatedAreaSqm,
            perimeterMeters = land.PerimeterMeters
        });
    }

    [HttpGet("lands/{id}")]
    public async Task<IActionResult> GetWaqfLand(int id)
    {
        var land = await _unitOfWork.Repository<WaqfLand>().Query()
            .Include(l => l.Province)
            .Include(l => l.WaqfOffice)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (land == null) return NotFound();

        return Ok(new
        {
            id = land.Id,
            code = land.Code,
            nameAr = land.NameAr,
            nameEn = land.NameEn,
            landType = land.LandType,
            landUse = land.LandUse,
            province = land.Province?.NameAr,
            waqfOffice = land.WaqfOffice?.NameAr,
            boundaryGeoJson = _geometryService.ToGeoJson(land.Boundary),
            centerPointGeoJson = _geometryService.ToGeoJson(land.CenterPoint),
            calculatedAreaSqm = land.CalculatedAreaSqm,
            legalAreaSqm = land.LegalAreaSqm,
            perimeterMeters = land.PerimeterMeters,
            deedNumber = land.DeedNumber,
            notes = land.Notes
        });
    }

    [HttpDelete("lands/{id}")]
    public async Task<IActionResult> DeleteWaqfLand(int id)
    {
        var land = await _unitOfWork.Repository<WaqfLand>().GetByIdAsync(id);
        if (land == null) return NotFound();

        land.IsDeleted = true;
        land.UpdatedBy = User.Identity?.Name;
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { success = true });
    }

    #endregion

    #region Road Endpoints

    [HttpPost("roads")]
    public async Task<IActionResult> SaveRoad([FromBody] RoadDto dto)
    {
        var geometry = _geometryService.FromGeoJson(dto.GeometryGeoJson) as LineString;
        if (geometry == null)
            return BadRequest(new { error = "Invalid LineString geometry" });

        Road road;
        if (dto.Id.HasValue)
        {
            road = await _unitOfWork.Repository<Road>().GetByIdAsync(dto.Id.Value);
            if (road == null) return NotFound();

            road.NameAr = dto.NameAr;
            road.NameEn = dto.NameEn;
            road.RoadType = dto.RoadType ?? "Local";
            road.WidthMeters = dto.WidthMeters;
            road.SurfaceType = dto.SurfaceType;
            road.Geometry = geometry;
            road.LengthMeters = _geometryService.CalculateLengthMeters(geometry);
            road.ProvinceId = dto.ProvinceId;
            road.UpdatedBy = User.Identity?.Name;
        }
        else
        {
            road = new Road
            {
                Code = GenerateCode("RD"),
                NameAr = dto.NameAr,
                NameEn = dto.NameEn,
                RoadType = dto.RoadType ?? "Local",
                WidthMeters = dto.WidthMeters,
                SurfaceType = dto.SurfaceType,
                Geometry = geometry,
                LengthMeters = _geometryService.CalculateLengthMeters(geometry),
                ProvinceId = dto.ProvinceId,
                CreatedBy = User.Identity?.Name
            };

            await _unitOfWork.Repository<Road>().AddAsync(road);
        }

        await _unitOfWork.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            id = road.Id,
            code = road.Code,
            lengthMeters = road.LengthMeters
        });
    }

    [HttpDelete("roads/{id}")]
    public async Task<IActionResult> DeleteRoad(int id)
    {
        var road = await _unitOfWork.Repository<Road>().GetByIdAsync(id);
        if (road == null) return NotFound();

        road.IsDeleted = true;
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { success = true });
    }

    #endregion

    #region Spatial Analysis Endpoints

    [HttpGet("analysis/buffer/mosque/{id}")]
    public async Task<IActionResult> MosqueBufferAnalysis(int id, [FromQuery] double distance = 100)
    {
        try
        {
            var result = await _spatialAnalysisService.AnalyzeMosqueBuffer(id, distance);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("analysis/buffer/land/{id}")]
    public async Task<IActionResult> LandBufferAnalysis(int id, [FromQuery] double distance = 100)
    {
        try
        {
            var result = await _spatialAnalysisService.AnalyzeWaqfLandBuffer(id, distance);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("analysis/roads/land/{id}")]
    public async Task<IActionResult> RoadIntersectionAnalysis(int id)
    {
        try
        {
            var result = await _spatialAnalysisService.AnalyzeRoadIntersections(id);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("analysis/overlaps")]
    public async Task<IActionResult> DetectOverlaps([FromQuery] int? landId)
    {
        var result = await _spatialAnalysisService.DetectLandOverlaps(landId);
        return Ok(result);
    }

    [HttpPost("analysis/area-stats")]
    public async Task<IActionResult> CalculateAreaStatistics([FromBody] GeometryDto dto)
    {
        var geometry = _geometryService.FromGeoJson(dto.GeoJson);
        if (geometry == null)
            return BadRequest(new { error = "Invalid geometry" });

        var stats = await _spatialAnalysisService.CalculateAreaStatistics(geometry);
        return Ok(stats);
    }

    [HttpPost("analysis/calculate-area")]
    public IActionResult CalculateArea([FromBody] GeometryDto dto)
    {
        var geometry = _geometryService.FromGeoJson(dto.GeoJson);
        if (geometry == null)
            return BadRequest(new { error = "Invalid geometry" });

        return Ok(new
        {
            areaSqm = _geometryService.CalculateAreaSquareMeters(geometry),
            areaDonum = _geometryService.CalculateAreaSquareMeters(geometry) / 2500.0,
            perimeterMeters = _geometryService.CalculatePerimeterMeters(geometry),
            isValid = _geometryService.IsValid(geometry)
        });
    }

    #endregion

    #region Geometry Audit Log

    [HttpGet("audit/{entityType}/{entityId}")]
    public async Task<IActionResult> GetGeometryAuditLog(string entityType, int entityId)
    {
        var logs = await _unitOfWork.Repository<GeometryAuditLog>().Query()
            .Where(l => l.EntityType == entityType && l.EntityId == entityId)
            .OrderByDescending(l => l.Timestamp)
            .Take(50)
            .ToListAsync();

        return Ok(logs);
    }

    #endregion

    #region Private Helpers

    private async Task LogGeometryChange(string entityType, int entityId, string entityName,
        string action, Geometry? oldGeometry, Geometry? newGeometry)
    {
        var log = new GeometryAuditLog
        {
            EntityType = entityType,
            EntityId = entityId,
            EntityName = entityName,
            Action = action,
            OldGeometryWkt = _geometryService.ToWkt(oldGeometry),
            NewGeometryWkt = _geometryService.ToWkt(newGeometry),
            OldAreaSqm = oldGeometry != null ? _geometryService.CalculateAreaSquareMeters(oldGeometry) : null,
            NewAreaSqm = newGeometry != null ? _geometryService.CalculateAreaSquareMeters(newGeometry) : null,
            UserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
            UserName = User.Identity?.Name,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            Timestamp = DateTime.UtcNow
        };

        await _unitOfWork.Repository<GeometryAuditLog>().AddAsync(log);
    }

    private string GenerateCode(string prefix)
    {
        return $"{prefix}-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
    }

    #endregion
}

#region DTOs

public class GeometryDto
{
    public string GeoJson { get; set; } = string.Empty;
    public string? BoundaryType { get; set; }
}

public class LayerStyleDto
{
    public string? FillColor { get; set; }
    public double? FillOpacity { get; set; }
    public string? StrokeColor { get; set; }
    public double? StrokeWidth { get; set; }
}

public class WaqfLandDto
{
    public int? Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public int ProvinceId { get; set; }
    public int? WaqfOfficeId { get; set; }
    public string? LandType { get; set; }
    public string? LandUse { get; set; }
    public string? BoundaryGeoJson { get; set; }
    public decimal? LegalAreaSqm { get; set; }
    public string? DeedNumber { get; set; }
    public string? Notes { get; set; }
}

public class RoadDto
{
    public int? Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string? RoadType { get; set; }
    public double? WidthMeters { get; set; }
    public string? SurfaceType { get; set; }
    public string? GeometryGeoJson { get; set; }
    public int? ProvinceId { get; set; }
}

#endregion
