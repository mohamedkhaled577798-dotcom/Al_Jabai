using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using WaqfGIS.Core.Entities;
using WaqfGIS.Core.Interfaces;
using WaqfGIS.Services.GIS;

namespace WaqfGIS.Services;

public class RoadService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly GeometryService _geometryService;

    public RoadService(IUnitOfWork unitOfWork, GeometryService geometryService)
    {
        _unitOfWork = unitOfWork;
        _geometryService = geometryService;
    }

    public async Task<List<Road>> GetAllAsync()
    {
        return await _unitOfWork.Repository<Road>().Query()
            .Include(r => r.Province)
            .Include(r => r.District)
            .OrderBy(r => r.NameAr)
            .ToListAsync();
    }

    public async Task<Road?> GetByIdAsync(int id)
    {
        return await _unitOfWork.Repository<Road>().Query()
            .Include(r => r.Province)
            .Include(r => r.District)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<List<Road>> SearchAsync(int? provinceId, string? roadType, string? search)
    {
        var query = _unitOfWork.Repository<Road>().Query()
            .Include(r => r.Province)
            .Include(r => r.District)
            .AsQueryable();

        if (provinceId.HasValue)
            query = query.Where(r => r.ProvinceId == provinceId.Value);
        
        if (!string.IsNullOrEmpty(roadType))
            query = query.Where(r => r.RoadType == roadType);
        
        if (!string.IsNullOrEmpty(search))
            query = query.Where(r => r.NameAr.Contains(search) || r.Code.Contains(search));

        return await query.OrderBy(r => r.NameAr).ToListAsync();
    }

    public async Task<Road> CreateAsync(Road road, string? geoJson)
    {
        // Generate code
        road.Code = await GenerateCodeAsync(road.ProvinceId);
        
        // Process geometry
        if (!string.IsNullOrEmpty(geoJson))
        {
            var geometry = _geometryService.FromGeoJson(geoJson) as LineString;
            if (geometry != null)
            {
                road.Geometry = geometry;
                road.LengthMeters = _geometryService.CalculateLengthMeters(geometry);
            }
        }

        await _unitOfWork.Repository<Road>().AddAsync(road);
        await _unitOfWork.SaveChangesAsync();

        return road;
    }

    public async Task UpdateAsync(Road road, string? geoJson)
    {
        if (!string.IsNullOrEmpty(geoJson))
        {
            var geometry = _geometryService.FromGeoJson(geoJson) as LineString;
            if (geometry != null)
            {
                road.Geometry = geometry;
                road.LengthMeters = _geometryService.CalculateLengthMeters(geometry);
            }
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var road = await _unitOfWork.Repository<Road>().GetByIdAsync(id);
        if (road != null)
        {
            road.IsDeleted = true;
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public string? GetGeoJson(Road road)
    {
        return road.Geometry != null ? _geometryService.ToGeoJson(road.Geometry) : null;
    }

    private async Task<string> GenerateCodeAsync(int? provinceId)
    {
        var prefix = "RD";
        if (provinceId.HasValue)
        {
            var province = await _unitOfWork.Provinces.GetByIdAsync(provinceId.Value);
            if (province != null)
                prefix = province.Code ?? "RD";
        }

        var count = await _unitOfWork.Repository<Road>().Query()
            .Where(r => r.ProvinceId == provinceId)
            .CountAsync();

        return $"{prefix}-RD-{(count + 1):D4}";
    }

    public async Task<RoadStatistics> GetStatisticsAsync()
    {
        var roads = await _unitOfWork.Repository<Road>().Query()
            .Include(r => r.Province)
            .ToListAsync();

        return new RoadStatistics
        {
            TotalCount = roads.Count,
            TotalLengthKm = roads.Sum(r => r.LengthMeters ?? 0) / 1000,
            ByRoadType = roads.GroupBy(r => r.RoadType).ToDictionary(g => g.Key, g => g.Count()),
            ByProvince = roads.Where(r => r.Province != null).GroupBy(r => r.Province!.NameAr).ToDictionary(g => g.Key, g => g.Count())
        };
    }
}

public class RoadStatistics
{
    public int TotalCount { get; set; }
    public double TotalLengthKm { get; set; }
    public Dictionary<string, int> ByRoadType { get; set; } = new();
    public Dictionary<string, int> ByProvince { get; set; } = new();
}
