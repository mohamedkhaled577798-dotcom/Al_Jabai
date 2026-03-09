using Microsoft.EntityFrameworkCore;
using WaqfGIS.Core.Entities;
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
            TotalMosques    = await _unitOfWork.Mosques.CountAsync(),
            TotalProperties = await _unitOfWork.WaqfProperties.CountAsync(),
            TotalOffices    = await _unitOfWork.WaqfOffices.CountAsync(),
            TotalProvinces  = await _unitOfWork.Provinces.CountAsync()
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

    // =================== Dashboard المتقدم ===================
    public async Task<AdvancedDashboardStats> GetAdvancedDashboardAsync()
    {
        var stats = new AdvancedDashboardStats();

        // ---- الأرقام الإجمالية ----
        stats.TotalMosques      = await _unitOfWork.Mosques.CountAsync();
        stats.TotalProperties   = await _unitOfWork.WaqfProperties.CountAsync();
        stats.TotalLands        = (int)await _unitOfWork.Repository<WaqfLand>().Query().CountAsync();
        stats.TotalOffices      = await _unitOfWork.WaqfOffices.CountAsync();

        var contracts     = await _unitOfWork.Repository<InvestmentContract>().GetAllAsync();
        var disputes      = await _unitOfWork.Repository<LegalDispute>().GetAllAsync();
        var encroachments = await _unitOfWork.Repository<EncroachmentRecord>().GetAllAsync();
        var properties    = await _unitOfWork.WaqfProperties.GetAllAsync();
        var mosques       = await _unitOfWork.Mosques.Query().Include(m => m.Province).ToListAsync();
        var lands         = await _unitOfWork.Repository<WaqfLand>().Query().Include(l => l.Province).ToListAsync();

        // ---- العقود ----
        stats.ActiveContracts   = contracts.Count(c => c.IsActive);
        stats.ExpiredContracts  = contracts.Count(c => !c.IsActive);
        stats.ExpiringIn3Months = contracts.Count(c => c.IsActive && c.EndDate <= DateTime.Now.AddMonths(3));
        stats.ExpiringIn1Month  = contracts.Count(c => c.IsActive && c.EndDate <= DateTime.Now.AddMonths(1));
        stats.TotalAnnualRent   = contracts.Where(c => c.IsActive).Sum(c => c.AnnualRent);
        stats.ContractsByType   = contracts
            .GroupBy(c => c.ContractType)
            .Select(g => new TypeCount { Name = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count).ToList();

        // ---- الدعاوى ----
        stats.ActiveDisputes   = disputes.Count(d => d.CaseStatus == "جارية");
        stats.ResolvedDisputes = disputes.Count(d => d.CaseStatus == "منتهية");
        stats.DisputesByType   = disputes
            .GroupBy(d => d.DisputeType)
            .Select(g => new TypeCount { Name = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count).ToList();
        stats.VerdictResults   = disputes.Where(d => d.HasVerdict)
            .GroupBy(d => d.VerdictResult ?? "غير محدد")
            .Select(g => new TypeCount { Name = g.Key, Count = g.Count() })
            .ToList();

        // ---- التجاوزات ----
        stats.ActiveEncroachments  = encroachments.Count(e => e.Status == "قائم");
        stats.InProcessEncroachments = encroachments.Count(e => e.Status == "قيد المعالجة");
        stats.RemovedEncroachments = encroachments.Count(e => e.Status == "أُزيل");
        stats.LegalEncroachments   = encroachments.Count(e => e.Status == "مرفوع للقضاء");
        stats.EncroachmentsBySeverity = encroachments
            .GroupBy(e => e.Severity ?? "غير محدد")
            .Select(g => new TypeCount { Name = g.Key, Count = g.Count() })
            .ToList();

        // ---- العقارات ----
        stats.OccupiedProperties  = properties.Count(p => p.IsResidentialOccupancy);
        stats.EncroachProperties  = properties.Count(p => p.HasEncroachment);
        stats.PropertiesByProvince = properties
            .GroupBy(p => p.ProvinceId)
            .Select(g => new TypeCount { Name = g.Key.ToString(), Count = g.Count() })
            .ToList();

        // ---- المساجد حسب المحافظة ----
        stats.MosquesByProvince = mosques
            .GroupBy(m => m.Province?.NameAr ?? "غير محدد")
            .Select(g => new TypeCount { Name = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count).Take(10).ToList();

        // ---- الأراضي حسب المحافظة ----
        stats.LandsByProvince = lands
            .GroupBy(l => l.Province?.NameAr ?? "غير محدد")
            .Select(g => new TypeCount { Name = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count).Take(10).ToList();

        // ---- الأصول الإجمالية (سريع) ----
        stats.TotalAssets = stats.TotalMosques + stats.TotalProperties + stats.TotalLands;

        // ---- نشاط آخر 6 أشهر (عقود جديدة) ----
        var sixMonthsAgo = DateTime.Now.AddMonths(-6);
        stats.MonthlyContractActivity = Enumerable.Range(0, 6)
            .Select(i => {
                var month = DateTime.Now.AddMonths(-5 + i);
                return new MonthCount {
                    Month = month.ToString("MMM yyyy"),
                    Count = contracts.Count(c => c.ContractDate.Year == month.Year && c.ContractDate.Month == month.Month)
                };
            }).ToList();

        return stats;
    }

    public async Task<ProvinceStatistics> GetProvinceStatisticsAsync(int provinceId)
    {
        var province = await _unitOfWork.Provinces.GetByIdAsync(provinceId);
        if (province == null) return new ProvinceStatistics();

        return new ProvinceStatistics
        {
            ProvinceName    = province.NameAr,
            MosquesCount    = await _unitOfWork.Mosques.CountAsync(m => m.ProvinceId == provinceId),
            PropertiesCount = await _unitOfWork.WaqfProperties.CountAsync(p => p.ProvinceId == provinceId),
            OfficesCount    = await _unitOfWork.WaqfOffices.CountAsync(o => o.ProvinceId == provinceId)
        };
    }
}

// =================== Models ===================

public class DashboardStatistics
{
    public int TotalMosques    { get; set; }
    public int TotalProperties { get; set; }
    public int TotalOffices    { get; set; }
    public int TotalProvinces  { get; set; }
    public List<TypeCount> MosquesByType    { get; set; } = new();
    public List<TypeCount> PropertiesByType { get; set; } = new();
    public List<TypeCount> MosquesByProvince{ get; set; } = new();
}

public class AdvancedDashboardStats
{
    // الأرقام الرئيسية
    public int TotalMosques    { get; set; }
    public int TotalProperties { get; set; }
    public int TotalLands      { get; set; }
    public int TotalOffices    { get; set; }
    public int TotalAssets     { get; set; }

    // العقود
    public int     ActiveContracts    { get; set; }
    public int     ExpiredContracts   { get; set; }
    public int     ExpiringIn3Months  { get; set; }
    public int     ExpiringIn1Month   { get; set; }
    public decimal TotalAnnualRent    { get; set; }
    public List<TypeCount>  ContractsByType  { get; set; } = new();

    // الدعاوى
    public int ActiveDisputes   { get; set; }
    public int ResolvedDisputes { get; set; }
    public List<TypeCount> DisputesByType { get; set; } = new();
    public List<TypeCount> VerdictResults { get; set; } = new();

    // التجاوزات
    public int ActiveEncroachments    { get; set; }
    public int InProcessEncroachments { get; set; }
    public int RemovedEncroachments   { get; set; }
    public int LegalEncroachments     { get; set; }
    public List<TypeCount> EncroachmentsBySeverity { get; set; } = new();

    // العقارات
    public int OccupiedProperties { get; set; }
    public int EncroachProperties { get; set; }
    public List<TypeCount> PropertiesByProvince { get; set; } = new();

    // توزيع جغرافي
    public List<TypeCount>  MosquesByProvince { get; set; } = new();
    public List<TypeCount>  LandsByProvince   { get; set; } = new();

    // نشاط شهري
    public List<MonthCount> MonthlyContractActivity { get; set; } = new();
}

public class TypeCount
{
    public string Name  { get; set; } = string.Empty;
    public int    Count { get; set; }
}

public class MonthCount
{
    public string Month { get; set; } = string.Empty;
    public int    Count { get; set; }
}

public class ProvinceStatistics
{
    public string ProvinceName    { get; set; } = string.Empty;
    public int    MosquesCount    { get; set; }
    public int    PropertiesCount { get; set; }
    public int    OfficesCount    { get; set; }
}
