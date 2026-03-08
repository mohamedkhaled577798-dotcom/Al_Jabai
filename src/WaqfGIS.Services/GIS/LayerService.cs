using Microsoft.EntityFrameworkCore;
using WaqfGIS.Core.Entities;
using WaqfGIS.Core.Interfaces;

namespace WaqfGIS.Services.GIS;

/// <summary>
/// خدمة إدارة طبقات GIS
/// </summary>
public class LayerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly GeometryService _geometryService;

    public LayerService(IUnitOfWork unitOfWork, GeometryService geometryService)
    {
        _unitOfWork = unitOfWork;
        _geometryService = geometryService;
    }

    #region Layer Management

    public async Task<List<GisLayer>> GetAllLayersAsync()
    {
        return await _unitOfWork.Repository<GisLayer>().Query()
            .OrderBy(l => l.DisplayOrder)
            .ToListAsync();
    }

    public async Task<List<GisLayer>> GetVisibleLayersAsync()
    {
        return await _unitOfWork.Repository<GisLayer>().Query()
            .Where(l => l.IsVisible)
            .OrderBy(l => l.DisplayOrder)
            .ToListAsync();
    }

    public async Task UpdateLayerVisibility(int layerId, bool isVisible)
    {
        var layer = await _unitOfWork.Repository<GisLayer>().GetByIdAsync(layerId);
        if (layer != null)
        {
            layer.IsVisible = isVisible;
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task UpdateLayerStyle(int layerId, string? fillColor, double? fillOpacity,
        string? strokeColor, double? strokeWidth)
    {
        var layer = await _unitOfWork.Repository<GisLayer>().GetByIdAsync(layerId);
        if (layer != null)
        {
            if (fillColor != null) layer.FillColor = fillColor;
            if (fillOpacity.HasValue) layer.FillOpacity = fillOpacity.Value;
            if (strokeColor != null) layer.StrokeColor = strokeColor;
            if (strokeWidth.HasValue) layer.StrokeWidth = strokeWidth.Value;
            await _unitOfWork.SaveChangesAsync();
        }
    }

    #endregion

    #region GeoJSON Export

    public async Task<object> GetMosquesGeoJson(int? provinceId = null, bool includePolygons = true)
    {
        var query = _unitOfWork.Mosques.Query()
            .Include(m => m.MosqueType)
            .Include(m => m.MosqueStatus)
            .Include(m => m.Province)
            .AsQueryable();

        if (provinceId.HasValue)
            query = query.Where(m => m.ProvinceId == provinceId.Value);

        var mosques = await query.ToListAsync();

        Dictionary<int, MosqueBoundary?> boundaries = new();
        if (includePolygons)
        {
            var mosqueIds = mosques.Select(m => m.Id).ToList();
            var boundaryList = await _unitOfWork.Repository<MosqueBoundary>().Query()
                .Where(b => mosqueIds.Contains(b.MosqueId))
                .ToListAsync();
            boundaries = boundaryList.ToDictionary(b => b.MosqueId, b => (MosqueBoundary?)b);
        }

        var features = new List<object>();

        // Point features
        foreach (var m in mosques)
        {
            features.Add(new
            {
                type = "Feature",
                id = m.Id,
                geometry = ParseGeoJson(_geometryService.ToGeoJson(m.Location)),
                properties = new
                {
                    id = m.Id,
                    name = m.NameAr,
                    nameEn = m.NameEn,
                    code = m.Code,
                    type = m.MosqueType?.NameAr,
                    mosqueType = m.MosqueType?.NameAr,
                    status = m.MosqueStatus?.NameAr,
                    statusColor = m.MosqueStatus?.ColorCode ?? "#6c757d",
                    province = m.Province?.NameAr,
                    address = m.Address,
                    neighborhood = m.Neighborhood,
                    nearestLandmark = m.NearestLandmark,
                    capacity = m.Capacity,
                    areaSqm = m.AreaSqm,
                    hasFridayPrayer = m.HasFridayPrayer,
                    imamName = m.ImamName,
                    hasBoundary = boundaries.ContainsKey(m.Id) && boundaries[m.Id]?.Boundary != null,
                    isPolygon = false
                }
            });
        }

        // Polygon features
        if (includePolygons)
        {
            foreach (var kvp in boundaries.Where(b => b.Value?.Boundary != null))
            {
                var mosque = mosques.First(m => m.Id == kvp.Key);
                features.Add(new
                {
                    type = "Feature",
                    id = kvp.Key * -1,
                    geometry = ParseGeoJson(_geometryService.ToGeoJson(kvp.Value!.Boundary)),
                    properties = new
                    {
                        id = kvp.Key,
                        name = mosque.NameAr,
                        nameEn = mosque.NameEn,
                        code = mosque.Code,
                        mosqueType = mosque.MosqueType?.NameAr,
                        status = mosque.MosqueStatus?.NameAr,
                        statusColor = mosque.MosqueStatus?.ColorCode ?? "#6c757d",
                        province = mosque.Province?.NameAr,
                        capacity = mosque.Capacity,
                        areaSqm = kvp.Value!.CalculatedAreaSqm,
                        hasFridayPrayer = mosque.HasFridayPrayer,
                        imamName = mosque.ImamName,
                        hasBoundary = true,
                        isPolygon = true
                    }
                });
            }
        }

        return new { type = "FeatureCollection", features };
    }

    public async Task<object> GetPropertiesGeoJson(int? provinceId = null)
    {
        var query = _unitOfWork.WaqfProperties.Query()
            .Include(p => p.PropertyType)
            .Include(p => p.UsageType)
            .Include(p => p.Province)
            .AsQueryable();

        if (provinceId.HasValue)
            query = query.Where(p => p.ProvinceId == provinceId.Value);

        var properties = await query.ToListAsync();

        var features = properties
            .Where(p => p.Location != null)
            .Select(p => new
            {
                type = "Feature",
                id = p.Id,
                geometry = ParseGeoJson(_geometryService.ToGeoJson(p.Location)),
                properties = new
                {
                    id = p.Id,
                    name = p.NameAr,
                    nameEn = p.NameEn,
                    code = p.Code,
                    type = p.PropertyType?.NameAr,
                    usage = p.UsageType?.NameAr,
                    province = p.Province?.NameAr,
                    address = p.Address,
                    neighborhood = p.Neighborhood,
                    areaSqm = p.AreaSqm,
                    totalArea = p.TotalArea,
                    estimatedValue = p.EstimatedValue,
                    monthlyRent = p.MonthlyRent,
                    rentalStatus = p.RentalStatus
                }
            }).ToList();

        return new { type = "FeatureCollection", features };
    }

    public async Task<object> GetMosqueBoundariesGeoJson(int? provinceId = null)
    {
        var query = _unitOfWork.Repository<MosqueBoundary>().Query()
            .Include(b => b.Mosque)
            .ThenInclude(m => m.Province)
            .Where(b => b.Boundary != null);

        if (provinceId.HasValue)
            query = query.Where(b => b.Mosque.ProvinceId == provinceId.Value);

        var boundaries = await query.ToListAsync();

        var features = boundaries.Select(b => new
        {
            type = "Feature",
            id = b.Id,
            geometry = ParseGeoJson(_geometryService.ToGeoJson(b.Boundary)),
            properties = new
            {
                id = b.Id,
                entityId = b.MosqueId,
                entityName = b.Mosque?.NameAr,
                entityCode = b.Mosque?.Code,
                boundaryType = b.BoundaryType,
                calculatedAreaSqm = b.CalculatedAreaSqm,
                perimeterMeters = b.PerimeterMeters,
                province = b.Mosque?.Province?.NameAr,
                entityType = "mosque"
            }
        }).ToList();

        return new { type = "FeatureCollection", features };
    }

    public async Task<object> GetPropertyBoundariesGeoJson(int? provinceId = null)
    {
        var query = _unitOfWork.Repository<PropertyBoundary>().Query()
            .Include(b => b.Property)
            .ThenInclude(p => p.Province)
            .Where(b => b.Boundary != null);

        if (provinceId.HasValue)
            query = query.Where(b => b.Property.ProvinceId == provinceId.Value);

        var boundaries = await query.ToListAsync();

        var features = boundaries.Select(b => new
        {
            type = "Feature",
            id = b.Id,
            geometry = ParseGeoJson(_geometryService.ToGeoJson(b.Boundary)),
            properties = new
            {
                id = b.Id,
                entityId = b.PropertyId,
                entityName = b.Property?.NameAr,
                entityCode = b.Property?.Code,
                boundaryType = b.BoundaryType,
                calculatedAreaSqm = b.CalculatedAreaSqm,
                perimeterMeters = b.PerimeterMeters,
                province = b.Property?.Province?.NameAr,
                entityType = "property"
            }
        }).ToList();

        return new { type = "FeatureCollection", features };
    }

    public async Task<object> GetWaqfLandsGeoJson(int? provinceId = null)
    {
        var query = _unitOfWork.Repository<WaqfLand>().Query()
            .Include(l => l.Province)
            .AsQueryable();

        if (provinceId.HasValue)
            query = query.Where(l => l.ProvinceId == provinceId.Value);

        var lands = await query.ToListAsync();
        var features = new List<object>();

        foreach (var land in lands)
        {
            var geometry = land.Boundary ?? land.CenterPoint;
            if (geometry == null) continue;

            features.Add(new
            {
                type = "Feature",
                id = land.Id,
                geometry = ParseGeoJson(_geometryService.ToGeoJson(geometry)),
                properties = new
                {
                    id = land.Id,
                    name = land.NameAr,
                    code = land.Code,
                    landType = land.LandType,
                    landUse = land.LandUse,
                    province = land.Province?.NameAr,
                    calculatedAreaSqm = land.CalculatedAreaSqm,
                    legalAreaSqm = land.LegalAreaSqm,
                    areaDonum = land.AreaDonum,
                    estimatedValue = land.EstimatedValue,
                    isPolygon = land.Boundary != null
                }
            });
        }

        return new { type = "FeatureCollection", features };
    }

    public async Task<object> GetRoadsGeoJson(int? provinceId = null)
    {
        var query = _unitOfWork.Repository<Road>().Query();

        if (provinceId.HasValue)
            query = query.Where(r => r.ProvinceId == provinceId.Value);

        var roads = await query.ToListAsync();

        var features = roads
            .Where(r => r.Geometry != null)
            .Select(r => new
            {
                type = "Feature",
                id = r.Id,
                geometry = ParseGeoJson(_geometryService.ToGeoJson(r.Geometry)),
                properties = new
                {
                    id = r.Id,
                    name = r.NameAr,
                    nameEn = r.NameEn,
                    code = r.Code,
                    roadType = r.RoadType,
                    widthMeters = r.WidthMeters,
                    lanesCount = r.LanesCount,
                    surfaceType = r.SurfaceType,
                    lengthMeters = r.LengthMeters
                }
            }).ToList();

        return new { type = "FeatureCollection", features };
    }

    public async Task<object> GetNearbyProjectsGeoJson(int? provinceId = null)
    {
        var query = _unitOfWork.Repository<NearbyProject>().Query();

        if (provinceId.HasValue)
            query = query.Where(p => p.ProvinceId == provinceId.Value);

        var projects = await query.ToListAsync();
        var features = new List<object>();

        foreach (var project in projects)
        {
            var geometry = project.Boundary ?? project.Location;
            if (geometry == null) continue;

            features.Add(new
            {
                type = "Feature",
                id = project.Id,
                geometry = ParseGeoJson(_geometryService.ToGeoJson(geometry)),
                properties = new
                {
                    id = project.Id,
                    name = project.NameAr,
                    nameEn = project.NameEn,
                    code = project.Code,
                    projectType = project.ProjectType,
                    status = project.Status,
                    ownerName = project.OwnerName,
                    projectValue = project.ProjectValue,
                    areaSqm = project.CalculatedAreaSqm,
                    isPolygon = project.Boundary != null
                }
            });
        }

        return new { type = "FeatureCollection", features };
    }

    public async Task<object> GetAllLayersGeoJson(int? provinceId = null)
    {
        return new
        {
            mosques = await GetMosquesGeoJson(provinceId),
            properties = await GetPropertiesGeoJson(provinceId),
            waqfLands = await GetWaqfLandsGeoJson(provinceId),
            roads = await GetRoadsGeoJson(provinceId),
            nearbyProjects = await GetNearbyProjectsGeoJson(provinceId)
        };
    }

    #endregion

    private object? ParseGeoJson(string geoJson)
    {
        if (string.IsNullOrEmpty(geoJson) || geoJson == "null") return null;
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<object>(geoJson);
        }
        catch
        {
            return null;
        }
    }
}
