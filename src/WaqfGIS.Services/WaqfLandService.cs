using Microsoft.EntityFrameworkCore;
using WaqfGIS.Core.Entities;
using WaqfGIS.Core.Interfaces;
using WaqfGIS.Services.GIS;

namespace WaqfGIS.Services;

/// <summary>
/// خدمة إدارة أراضي الوقف
/// </summary>
public class WaqfLandService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly GeometryService _geometryService;

    public WaqfLandService(IUnitOfWork unitOfWork, GeometryService geometryService)
    {
        _unitOfWork = unitOfWork;
        _geometryService = geometryService;
    }

    /// <summary>
    /// جلب جميع الأراضي
    /// </summary>
    public async Task<List<WaqfLand>> GetAllAsync()
    {
        return await _unitOfWork.Repository<WaqfLand>().Query()
            .Include(l => l.Province)
            .Include(l => l.District)
            .Include(l => l.WaqfOffice)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// جلب أرض بالمعرف
    /// </summary>
    public async Task<WaqfLand?> GetByIdAsync(int id)
    {
        return await _unitOfWork.Repository<WaqfLand>().Query()
            .Include(l => l.Province)
            .Include(l => l.District)
            .Include(l => l.WaqfOffice)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    /// <summary>
    /// البحث المتقدم
    /// </summary>
    public async Task<(List<WaqfLand> Items, int TotalCount)> SearchAsync(
        string? searchTerm = null,
        int? provinceId = null,
        int? districtId = null,
        int? waqfOfficeId = null,
        string? landType = null,
        string? landUse = null,
        decimal? minArea = null,
        decimal? maxArea = null,
        bool? hasBoundary = null,
        int pageNumber = 1,
        int pageSize = 10,
        string sortBy = "CreatedAt",
        bool sortDescending = true)
    {
        var query = _unitOfWork.Repository<WaqfLand>().Query()
            .Include(l => l.Province)
            .Include(l => l.District)
            .Include(l => l.WaqfOffice)
            .AsQueryable();

        // Filters
        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(l => 
                l.NameAr.Contains(searchTerm) || 
                (l.NameEn != null && l.NameEn.Contains(searchTerm)) ||
                (l.Code != null && l.Code.Contains(searchTerm)) ||
                (l.DeedNumber != null && l.DeedNumber.Contains(searchTerm)));
        }

        if (provinceId.HasValue)
            query = query.Where(l => l.ProvinceId == provinceId.Value);

        if (districtId.HasValue)
            query = query.Where(l => l.DistrictId == districtId.Value);

        if (waqfOfficeId.HasValue)
            query = query.Where(l => l.WaqfOfficeId == waqfOfficeId.Value);

        if (!string.IsNullOrEmpty(landType))
            query = query.Where(l => l.LandType == landType);

        if (!string.IsNullOrEmpty(landUse))
            query = query.Where(l => l.LandUse == landUse);

        if (minArea.HasValue)
            query = query.Where(l => l.LegalAreaSqm >= minArea || l.CalculatedAreaSqm >= (double)minArea);

        if (maxArea.HasValue)
            query = query.Where(l => l.LegalAreaSqm <= maxArea || l.CalculatedAreaSqm <= (double)maxArea);

        if (hasBoundary.HasValue)
        {
            if (hasBoundary.Value)
                query = query.Where(l => l.Boundary != null);
            else
                query = query.Where(l => l.Boundary == null);
        }

        var totalCount = await query.CountAsync();

        // Sorting
        query = sortBy switch
        {
            "NameAr" => sortDescending ? query.OrderByDescending(l => l.NameAr) : query.OrderBy(l => l.NameAr),
            "Code" => sortDescending ? query.OrderByDescending(l => l.Code) : query.OrderBy(l => l.Code),
            "Province" => sortDescending ? query.OrderByDescending(l => l.Province.NameAr) : query.OrderBy(l => l.Province.NameAr),
            "LandType" => sortDescending ? query.OrderByDescending(l => l.LandType) : query.OrderBy(l => l.LandType),
            "Area" => sortDescending ? query.OrderByDescending(l => l.CalculatedAreaSqm) : query.OrderBy(l => l.CalculatedAreaSqm),
            _ => sortDescending ? query.OrderByDescending(l => l.CreatedAt) : query.OrderBy(l => l.CreatedAt)
        };

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    /// <summary>
    /// إنشاء أرض جديدة
    /// </summary>
    public async Task<WaqfLand> CreateAsync(WaqfLand land, string? boundaryGeoJson, string? createdBy)
    {
        // Generate code
        land.Code = await GenerateCodeAsync(land.ProvinceId);
        land.Uuid = Guid.NewGuid();
        land.CreatedBy = createdBy;

        // Process geometry
        if (!string.IsNullOrEmpty(boundaryGeoJson))
        {
            var geometry = _geometryService.FromGeoJson(boundaryGeoJson);
            if (geometry != null)
            {
                land.Boundary = geometry;
                land.CenterPoint = _geometryService.CalculateCentroid(geometry);
                land.CalculatedAreaSqm = _geometryService.CalculateAreaSquareMeters(geometry);
                land.PerimeterMeters = _geometryService.CalculatePerimeterMeters(geometry);
                
                // Convert to Donum (1 Donum = 2500 m²)
                if (land.CalculatedAreaSqm.HasValue)
                    land.AreaDonum = (decimal)(land.CalculatedAreaSqm.Value / 2500.0);
            }
        }

        await _unitOfWork.Repository<WaqfLand>().AddAsync(land);
        await _unitOfWork.SaveChangesAsync();

        // Log geometry change
        await LogGeometryChangeAsync(land.Id, land.NameAr, "Created", null, land.Boundary, createdBy);

        return land;
    }

    /// <summary>
    /// تحديث أرض
    /// </summary>
    public async Task<WaqfLand?> UpdateAsync(int id, WaqfLand updatedLand, string? boundaryGeoJson, string? updatedBy)
    {
        var land = await _unitOfWork.Repository<WaqfLand>().GetByIdAsync(id);
        if (land == null) return null;

        var oldBoundary = land.Boundary;

        // Update properties
        land.NameAr = updatedLand.NameAr;
        land.NameEn = updatedLand.NameEn;
        land.Description = updatedLand.Description;
        land.ProvinceId = updatedLand.ProvinceId;
        land.DistrictId = updatedLand.DistrictId;
        land.WaqfOfficeId = updatedLand.WaqfOfficeId;
        land.Address = updatedLand.Address;
        land.Neighborhood = updatedLand.Neighborhood;
        land.LandType = updatedLand.LandType;
        land.LandUse = updatedLand.LandUse;
        land.ZoningCode = updatedLand.ZoningCode;
        land.LegalAreaSqm = updatedLand.LegalAreaSqm;
        land.DeedNumber = updatedLand.DeedNumber;
        land.RegistrationNumber = updatedLand.RegistrationNumber;
        land.RegistrationDate = updatedLand.RegistrationDate;
        land.OwnershipStatus = updatedLand.OwnershipStatus;
        land.EstimatedValue = updatedLand.EstimatedValue;
        land.AnnualRevenue = updatedLand.AnnualRevenue;
        land.Notes = updatedLand.Notes;
        land.UpdatedBy = updatedBy;

        // Process geometry
        if (!string.IsNullOrEmpty(boundaryGeoJson))
        {
            var geometry = _geometryService.FromGeoJson(boundaryGeoJson);
            if (geometry != null)
            {
                land.Boundary = geometry;
                land.CenterPoint = _geometryService.CalculateCentroid(geometry);
                land.CalculatedAreaSqm = _geometryService.CalculateAreaSquareMeters(geometry);
                land.PerimeterMeters = _geometryService.CalculatePerimeterMeters(geometry);
                
                if (land.CalculatedAreaSqm.HasValue)
                    land.AreaDonum = (decimal)(land.CalculatedAreaSqm.Value / 2500.0);

                // Log geometry change
                await LogGeometryChangeAsync(land.Id, land.NameAr, "Modified", oldBoundary, land.Boundary, updatedBy);
            }
        }

        await _unitOfWork.SaveChangesAsync();
        return land;
    }

    /// <summary>
    /// حذف أرض (Soft Delete)
    /// </summary>
    public async Task<bool> DeleteAsync(int id, string? deletedBy)
    {
        var land = await _unitOfWork.Repository<WaqfLand>().GetByIdAsync(id);
        if (land == null) return false;

        // Log before delete
        await LogGeometryChangeAsync(land.Id, land.NameAr, "Deleted", land.Boundary, null, deletedBy);

        land.IsDeleted = true;
        land.UpdatedBy = deletedBy;
        await _unitOfWork.SaveChangesAsync();
        
        return true;
    }

    /// <summary>
    /// الحصول على GeoJSON للحدود
    /// </summary>
    public string? GetBoundaryGeoJson(WaqfLand land)
    {
        if (land.Boundary == null) return null;
        return _geometryService.ToGeoJson(land.Boundary);
    }

    /// <summary>
    /// الحصول على إحصائيات
    /// </summary>
    public async Task<WaqfLandStatistics> GetStatisticsAsync(int? provinceId = null)
    {
        var query = _unitOfWork.Repository<WaqfLand>().Query();

        if (provinceId.HasValue)
            query = query.Where(l => l.ProvinceId == provinceId.Value);

        var lands = await query.ToListAsync();

        return new WaqfLandStatistics
        {
            TotalCount = lands.Count,
            TotalAreaSqm = lands.Sum(l => l.CalculatedAreaSqm ?? 0),
            TotalAreaDonum = lands.Sum(l => (double)(l.AreaDonum ?? 0)),
            TotalEstimatedValue = lands.Sum(l => l.EstimatedValue ?? 0),
            TotalAnnualRevenue = lands.Sum(l => l.AnnualRevenue ?? 0),
            WithBoundaryCount = lands.Count(l => l.Boundary != null),
            WithoutBoundaryCount = lands.Count(l => l.Boundary == null),
            ByLandType = lands.GroupBy(l => l.LandType)
                .ToDictionary(g => g.Key, g => g.Count()),
            ByLandUse = lands.GroupBy(l => l.LandUse)
                .ToDictionary(g => g.Key, g => g.Count()),
            ByProvince = lands.GroupBy(l => l.Province?.NameAr ?? "غير محدد")
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }

    /// <summary>
    /// توليد كود فريد
    /// </summary>
    private async Task<string> GenerateCodeAsync(int provinceId)
    {
        var province = await _unitOfWork.Provinces.GetByIdAsync(provinceId);
        var prefix = province?.Code ?? "WL";
        var count = await _unitOfWork.Repository<WaqfLand>().Query()
            .Where(l => l.ProvinceId == provinceId)
            .CountAsync();
        
        return $"{prefix}-WL-{(count + 1):D4}";
    }

    /// <summary>
    /// تسجيل تغييرات الهندسة
    /// </summary>
    private async Task LogGeometryChangeAsync(int entityId, string entityName, string action, 
        NetTopologySuite.Geometries.Geometry? oldGeometry, 
        NetTopologySuite.Geometries.Geometry? newGeometry, 
        string? userName)
    {
        var log = new GeometryAuditLog
        {
            EntityType = "WaqfLand",
            EntityId = entityId,
            EntityName = entityName,
            Action = action,
            OldGeometryWkt = oldGeometry != null ? _geometryService.ToWkt(oldGeometry) : null,
            NewGeometryWkt = newGeometry != null ? _geometryService.ToWkt(newGeometry) : null,
            OldAreaSqm = oldGeometry != null ? _geometryService.CalculateAreaSquareMeters(oldGeometry) : null,
            NewAreaSqm = newGeometry != null ? _geometryService.CalculateAreaSquareMeters(newGeometry) : null,
            UserName = userName,
            Timestamp = DateTime.UtcNow
        };

        await _unitOfWork.Repository<GeometryAuditLog>().AddAsync(log);
        await _unitOfWork.SaveChangesAsync();
    }
}

/// <summary>
/// إحصائيات أراضي الوقف
/// </summary>
public class WaqfLandStatistics
{
    public int TotalCount { get; set; }
    public double TotalAreaSqm { get; set; }
    public double TotalAreaDonum { get; set; }
    public decimal TotalEstimatedValue { get; set; }
    public decimal TotalAnnualRevenue { get; set; }
    public int WithBoundaryCount { get; set; }
    public int WithoutBoundaryCount { get; set; }
    public Dictionary<string, int> ByLandType { get; set; } = new();
    public Dictionary<string, int> ByLandUse { get; set; } = new();
    public Dictionary<string, int> ByProvince { get; set; } = new();
}
