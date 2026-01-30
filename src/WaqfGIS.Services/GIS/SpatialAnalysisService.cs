using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using WaqfGIS.Core.Entities;
using WaqfGIS.Core.Interfaces;

namespace WaqfGIS.Services.GIS;

/// <summary>
/// خدمة التحليل المكاني
/// </summary>
public class SpatialAnalysisService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly GeometryService _geometryService;

    public SpatialAnalysisService(IUnitOfWork unitOfWork, GeometryService geometryService)
    {
        _unitOfWork = unitOfWork;
        _geometryService = geometryService;
    }

    #region Buffer Analysis

    /// <summary>
    /// تحليل Buffer للمسجد - البحث عن الكيانات ضمن مسافة معينة
    /// </summary>
    public async Task<BufferAnalysisResult> AnalyzeMosqueBuffer(int mosqueId, double bufferMeters)
    {
        var mosque = await _unitOfWork.Mosques.GetByIdAsync(mosqueId);
        if (mosque?.Location == null)
            throw new ArgumentException("Mosque not found or has no location");

        var buffer = _geometryService.CreateBuffer(mosque.Location, bufferMeters);
        if (buffer == null)
            throw new InvalidOperationException("Failed to create buffer");

        var result = new BufferAnalysisResult
        {
            CenterEntityId = mosqueId,
            CenterEntityType = "Mosque",
            CenterEntityName = mosque.NameAr,
            BufferDistanceMeters = bufferMeters,
            BufferAreaSqm = _geometryService.CalculateAreaSquareMeters(buffer),
            BufferGeoJson = _geometryService.ToGeoJson(buffer)
        };

        // Find mosques within buffer
        var nearbyMosques = await _unitOfWork.Mosques.Query()
            .Where(m => m.Id != mosqueId && m.Location != null)
            .ToListAsync();

        result.NearbyMosques = nearbyMosques
            .Where(m => _geometryService.Intersects(buffer, m.Location))
            .Select(m => new NearbyEntity
            {
                Id = m.Id,
                Name = m.NameAr,
                Type = "Mosque",
                DistanceMeters = _geometryService.CalculateDistance(mosque.Location, m.Location),
                GeoJson = _geometryService.ToGeoJson(m.Location)
            })
            .OrderBy(e => e.DistanceMeters)
            .ToList();

        // Find properties within buffer
        var nearbyProperties = await _unitOfWork.WaqfProperties.Query()
            .Where(p => p.Location != null)
            .ToListAsync();

        result.NearbyProperties = nearbyProperties
            .Where(p => _geometryService.Intersects(buffer, p.Location))
            .Select(p => new NearbyEntity
            {
                Id = p.Id,
                Name = p.NameAr,
                Type = "Property",
                DistanceMeters = _geometryService.CalculateDistance(mosque.Location, p.Location),
                GeoJson = _geometryService.ToGeoJson(p.Location)
            })
            .OrderBy(e => e.DistanceMeters)
            .ToList();

        return result;
    }

    /// <summary>
    /// تحليل Buffer لأرض الوقف
    /// </summary>
    public async Task<BufferAnalysisResult> AnalyzeWaqfLandBuffer(int landId, double bufferMeters)
    {
        var land = await _unitOfWork.Repository<WaqfLand>().GetByIdAsync(landId);
        if (land == null)
            throw new ArgumentException("Waqf Land not found");

        var geometry = land.Boundary ?? land.CenterPoint;
        if (geometry == null)
            throw new ArgumentException("Land has no geometry");

        var buffer = _geometryService.CreateBuffer(geometry, bufferMeters);
        if (buffer == null)
            throw new InvalidOperationException("Failed to create buffer");

        var result = new BufferAnalysisResult
        {
            CenterEntityId = landId,
            CenterEntityType = "WaqfLand",
            CenterEntityName = land.NameAr,
            BufferDistanceMeters = bufferMeters,
            BufferAreaSqm = _geometryService.CalculateAreaSquareMeters(buffer),
            BufferGeoJson = _geometryService.ToGeoJson(buffer)
        };

        // Find nearby entities...
        // Similar logic as above

        return result;
    }

    #endregion

    #region Road Intersection Analysis

    /// <summary>
    /// تحليل تقاطع الطرق مع أرض الوقف
    /// </summary>
    public async Task<RoadIntersectionResult> AnalyzeRoadIntersections(int landId)
    {
        var land = await _unitOfWork.Repository<WaqfLand>().GetByIdAsync(landId);
        if (land?.Boundary == null)
            throw new ArgumentException("Land not found or has no boundary");

        var result = new RoadIntersectionResult
        {
            EntityId = landId,
            EntityName = land.NameAr,
            EntityGeoJson = _geometryService.ToGeoJson(land.Boundary)
        };

        var roads = await _unitOfWork.Repository<Road>().Query()
            .Where(r => r.Geometry != null)
            .ToListAsync();

        foreach (var road in roads)
        {
            if (_geometryService.Intersects(land.Boundary, road.Geometry))
            {
                var intersection = _geometryService.GetIntersection(land.Boundary, road.Geometry);
                
                result.IntersectingRoads.Add(new RoadIntersection
                {
                    RoadId = road.Id,
                    RoadName = road.NameAr,
                    RoadType = road.RoadType,
                    IntersectionLengthMeters = intersection != null 
                        ? _geometryService.CalculatePerimeterMeters(intersection) 
                        : 0,
                    IntersectionGeoJson = _geometryService.ToGeoJson(intersection)
                });
            }
        }

        result.TotalIntersectionLength = result.IntersectingRoads.Sum(r => r.IntersectionLengthMeters);

        return result;
    }

    #endregion

    #region Spatial Queries

    /// <summary>
    /// البحث عن المساجد ضمن مضلع
    /// </summary>
    public async Task<List<Mosque>> FindMosquesWithinPolygon(Geometry polygon)
    {
        var mosques = await _unitOfWork.Mosques.Query()
            .Where(m => m.Location != null)
            .ToListAsync();

        return mosques
            .Where(m => _geometryService.Contains(polygon, m.Location))
            .ToList();
    }

    /// <summary>
    /// البحث عن أقرب مسجد
    /// </summary>
    public async Task<Mosque?> FindNearestMosque(Point point, int? excludeId = null)
    {
        var mosques = await _unitOfWork.Mosques.Query()
            .Where(m => m.Location != null)
            .Where(m => excludeId == null || m.Id != excludeId)
            .ToListAsync();

        return mosques
            .OrderBy(m => _geometryService.CalculateDistance(point, m.Location))
            .FirstOrDefault();
    }

    /// <summary>
    /// حساب إحصائيات المنطقة
    /// </summary>
    public async Task<AreaStatistics> CalculateAreaStatistics(Geometry boundary)
    {
        var stats = new AreaStatistics
        {
            TotalAreaSqm = _geometryService.CalculateAreaSquareMeters(boundary)
        };

        // Count mosques
        var mosques = await _unitOfWork.Mosques.Query()
            .Where(m => m.Location != null)
            .ToListAsync();
        stats.MosquesCount = mosques.Count(m => _geometryService.Contains(boundary, m.Location));

        // Count properties
        var properties = await _unitOfWork.WaqfProperties.Query()
            .Where(p => p.Location != null)
            .ToListAsync();
        stats.PropertiesCount = properties.Count(p => _geometryService.Contains(boundary, p.Location));

        // Count lands
        var lands = await _unitOfWork.Repository<WaqfLand>().Query()
            .Where(l => l.CenterPoint != null || l.Boundary != null)
            .ToListAsync();
        stats.WaqfLandsCount = lands.Count(l => 
            _geometryService.Contains(boundary, l.CenterPoint) ||
            _geometryService.Intersects(boundary, l.Boundary));

        return stats;
    }

    #endregion

    #region Overlap Detection

    /// <summary>
    /// كشف التداخل بين أراضي الوقف
    /// </summary>
    public async Task<List<OverlapResult>> DetectLandOverlaps(int? landId = null)
    {
        var overlaps = new List<OverlapResult>();

        var lands = await _unitOfWork.Repository<WaqfLand>().Query()
            .Where(l => l.Boundary != null)
            .ToListAsync();

        if (landId.HasValue)
        {
            var targetLand = lands.FirstOrDefault(l => l.Id == landId.Value);
            if (targetLand?.Boundary == null) return overlaps;

            foreach (var land in lands.Where(l => l.Id != landId.Value))
            {
                if (_geometryService.Intersects(targetLand.Boundary, land.Boundary))
                {
                    var intersection = _geometryService.GetIntersection(targetLand.Boundary, land.Boundary);
                    overlaps.Add(new OverlapResult
                    {
                        Entity1Id = targetLand.Id,
                        Entity1Name = targetLand.NameAr,
                        Entity2Id = land.Id,
                        Entity2Name = land.NameAr,
                        OverlapAreaSqm = _geometryService.CalculateAreaSquareMeters(intersection),
                        OverlapGeoJson = _geometryService.ToGeoJson(intersection)
                    });
                }
            }
        }
        else
        {
            // Check all pairs
            for (int i = 0; i < lands.Count; i++)
            {
                for (int j = i + 1; j < lands.Count; j++)
                {
                    if (_geometryService.Intersects(lands[i].Boundary, lands[j].Boundary))
                    {
                        var intersection = _geometryService.GetIntersection(lands[i].Boundary, lands[j].Boundary);
                        overlaps.Add(new OverlapResult
                        {
                            Entity1Id = lands[i].Id,
                            Entity1Name = lands[i].NameAr,
                            Entity2Id = lands[j].Id,
                            Entity2Name = lands[j].NameAr,
                            OverlapAreaSqm = _geometryService.CalculateAreaSquareMeters(intersection),
                            OverlapGeoJson = _geometryService.ToGeoJson(intersection)
                        });
                    }
                }
            }
        }

        return overlaps;
    }

    #endregion
}

#region Result DTOs

public class BufferAnalysisResult
{
    public int CenterEntityId { get; set; }
    public string CenterEntityType { get; set; } = string.Empty;
    public string CenterEntityName { get; set; } = string.Empty;
    public double BufferDistanceMeters { get; set; }
    public double BufferAreaSqm { get; set; }
    public string BufferGeoJson { get; set; } = string.Empty;
    public List<NearbyEntity> NearbyMosques { get; set; } = new();
    public List<NearbyEntity> NearbyProperties { get; set; } = new();
    public List<NearbyEntity> NearbyLands { get; set; } = new();
    public List<NearbyEntity> NearbyProjects { get; set; } = new();
}

public class NearbyEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public double DistanceMeters { get; set; }
    public string? GeoJson { get; set; }
}

public class RoadIntersectionResult
{
    public int EntityId { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityGeoJson { get; set; } = string.Empty;
    public List<RoadIntersection> IntersectingRoads { get; set; } = new();
    public double TotalIntersectionLength { get; set; }
}

public class RoadIntersection
{
    public int RoadId { get; set; }
    public string RoadName { get; set; } = string.Empty;
    public string RoadType { get; set; } = string.Empty;
    public double IntersectionLengthMeters { get; set; }
    public string? IntersectionGeoJson { get; set; }
}

public class AreaStatistics
{
    public double TotalAreaSqm { get; set; }
    public int MosquesCount { get; set; }
    public int PropertiesCount { get; set; }
    public int WaqfLandsCount { get; set; }
    public int RoadsCount { get; set; }
    public int NearbyProjectsCount { get; set; }
}

public class OverlapResult
{
    public int Entity1Id { get; set; }
    public string Entity1Name { get; set; } = string.Empty;
    public int Entity2Id { get; set; }
    public string Entity2Name { get; set; } = string.Empty;
    public double OverlapAreaSqm { get; set; }
    public string? OverlapGeoJson { get; set; }
}

#endregion
