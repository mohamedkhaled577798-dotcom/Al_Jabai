using Microsoft.EntityFrameworkCore;
using WaqfGIS.Core.Interfaces;

namespace WaqfGIS.Services;

/// <summary>
/// خدمة التقارير والإحصائيات
/// </summary>
public class ReportService
{
    private readonly IUnitOfWork _unitOfWork;

    public ReportService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DashboardStatistics> GetDashboardStatisticsAsync()
    {
        var stats = new DashboardStatistics
        {
            TotalMosques = await _unitOfWork.Mosques.CountAsync(),
            TotalProperties = await _unitOfWork.WaqfProperties.CountAsync(),
            TotalOffices = await _unitOfWork.WaqfOffices.CountAsync(),
            TotalProvinces = await _unitOfWork.Provinces.CountAsync()
        };

        // Mosques by type
        stats.MosquesByType = await _unitOfWork.Mosques.Query()
            .Include(m => m.MosqueType)
            .GroupBy(m => m.MosqueType.NameAr)
            .Select(g => new TypeCount { Name = g.Key, Count = g.Count() })
            .ToListAsync();

        // Properties by type
        stats.PropertiesByType = await _unitOfWork.WaqfProperties.Query()
            .Include(p => p.PropertyType)
            .GroupBy(p => p.PropertyType.NameAr)
            .Select(g => new TypeCount { Name = g.Key, Count = g.Count() })
            .ToListAsync();

        // By province
        stats.MosquesByProvince = await _unitOfWork.Mosques.Query()
            .Include(m => m.Province)
            .GroupBy(m => m.Province.NameAr)
            .Select(g => new TypeCount { Name = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToListAsync();

        return stats;
    }

    public async Task<ProvinceStatistics> GetProvinceStatisticsAsync(int provinceId)
    {
        var province = await _unitOfWork.Provinces.GetByIdAsync(provinceId);
        if (province == null) return new ProvinceStatistics();

        return new ProvinceStatistics
        {
            ProvinceName = province.NameAr,
            MosquesCount = await _unitOfWork.Mosques.CountAsync(m => m.ProvinceId == provinceId),
            PropertiesCount = await _unitOfWork.WaqfProperties.CountAsync(p => p.ProvinceId == provinceId),
            OfficesCount = await _unitOfWork.WaqfOffices.CountAsync(o => o.ProvinceId == provinceId)
        };
    }
}

public class DashboardStatistics
{
    public int TotalMosques { get; set; }
    public int TotalProperties { get; set; }
    public int TotalOffices { get; set; }
    public int TotalProvinces { get; set; }
    public List<TypeCount> MosquesByType { get; set; } = new();
    public List<TypeCount> PropertiesByType { get; set; } = new();
    public List<TypeCount> MosquesByProvince { get; set; } = new();
}

public class TypeCount
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class ProvinceStatistics
{
    public string ProvinceName { get; set; } = string.Empty;
    public int MosquesCount { get; set; }
    public int PropertiesCount { get; set; }
    public int OfficesCount { get; set; }
}
