using WaqfGIS.Core.Entities;
using WaqfGIS.Core.Interfaces;

namespace WaqfGIS.Services;

/// <summary>
/// خدمة إدارة العقود الاستثمارية
/// </summary>
public class ContractService
{
    private readonly IUnitOfWork _unitOfWork;

    public ContractService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// الحصول على العقود القريبة من الانتهاء
    /// </summary>
    public async Task<List<InvestmentContract>> GetExpiringContractsAsync(int monthsAhead = 6)
    {
        var contracts = await _unitOfWork.Repository<InvestmentContract>()
            .FindAsync(c => c.IsActive && 
                           c.EndDate <= DateTime.Now.AddMonths(monthsAhead) &&
                           c.EndDate >= DateTime.Now);
        
        return contracts.OrderBy(c => c.EndDate).ToList();
    }

    /// <summary>
    /// الحصول على العقود المنتهية
    /// </summary>
    public async Task<List<InvestmentContract>> GetExpiredContractsAsync()
    {
        var contracts = await _unitOfWork.Repository<InvestmentContract>()
            .FindAsync(c => c.IsActive && c.EndDate < DateTime.Now);
        
        return contracts.OrderByDescending(c => c.EndDate).ToList();
    }

    /// <summary>
    /// إرسال تنبيهات العقود
    /// </summary>
    public async Task<Dictionary<string, List<InvestmentContract>>> GetContractNotificationsAsync()
    {
        var notifications = new Dictionary<string, List<InvestmentContract>>();
        
        // العقود التي تنتهي خلال 6 أشهر
        var sixMonths = await _unitOfWork.Repository<InvestmentContract>()
            .FindAsync(c => c.IsActive && 
                           c.NotifyBeforeSixMonths &&
                           c.EndDate <= DateTime.Now.AddMonths(6) &&
                           c.EndDate > DateTime.Now.AddMonths(5));
        notifications["SixMonths"] = sixMonths.ToList();
        
        // العقود التي تنتهي خلال 3 أشهر
        var threeMonths = await _unitOfWork.Repository<InvestmentContract>()
            .FindAsync(c => c.IsActive && 
                           c.NotifyBeforeThreeMonths &&
                           c.EndDate <= DateTime.Now.AddMonths(3) &&
                           c.EndDate > DateTime.Now.AddMonths(2));
        notifications["ThreeMonths"] = threeMonths.ToList();
        
        // العقود التي تنتهي خلال شهر
        var oneMonth = await _unitOfWork.Repository<InvestmentContract>()
            .FindAsync(c => c.IsActive && 
                           c.NotifyBeforeOneMonth &&
                           c.EndDate <= DateTime.Now.AddMonths(1) &&
                           c.EndDate > DateTime.Now);
        notifications["OneMonth"] = oneMonth.ToList();
        
        // العقود المنتهية
        var expired = await GetExpiredContractsAsync();
        notifications["Expired"] = expired;
        
        return notifications;
    }

    /// <summary>
    /// حساب الإيرادات المتوقعة من العقد
    /// </summary>
    public decimal CalculateExpectedRevenue(InvestmentContract contract, DateTime startDate, DateTime endDate)
    {
        if (startDate >= endDate || startDate > contract.EndDate || endDate < contract.StartDate)
            return 0;

        var effectiveStart = startDate < contract.StartDate ? contract.StartDate : startDate;
        var effectiveEnd = endDate > contract.EndDate ? contract.EndDate : endDate;

        var months = ((effectiveEnd.Year - effectiveStart.Year) * 12) + effectiveEnd.Month - effectiveStart.Month + 1;
        return contract.MonthlyRent * months;
    }

    /// <summary>
    /// الحصول على إحصائيات العقود
    /// </summary>
    public async Task<ContractStatistics> GetContractStatisticsAsync()
    {
        var allContracts = await _unitOfWork.Repository<InvestmentContract>().GetAllAsync();
        
        return new ContractStatistics
        {
            TotalContracts = allContracts.Count,
            ActiveContracts = allContracts.Count(c => c.IsActive),
            ExpiredContracts = allContracts.Count(c => c.EndDate < DateTime.Now && c.IsActive),
            ExpiringIn30Days = allContracts.Count(c => c.IsActive && c.EndDate <= DateTime.Now.AddDays(30) && c.EndDate >= DateTime.Now),
            ExpiringIn90Days = allContracts.Count(c => c.IsActive && c.EndDate <= DateTime.Now.AddDays(90) && c.EndDate >= DateTime.Now),
            TotalMonthlyRevenue = allContracts.Where(c => c.IsActive).Sum(c => c.MonthlyRent),
            TotalAnnualRevenue = allContracts.Where(c => c.IsActive).Sum(c => c.AnnualRent),
            TotalPaidAmount = allContracts.Sum(c => c.TotalPaidAmount),
            TotalOutstandingAmount = allContracts.Sum(c => c.TotalOutstandingAmount)
        };
    }
}

/// <summary>
/// إحصائيات العقود
/// </summary>
public class ContractStatistics
{
    public int TotalContracts { get; set; }
    public int ActiveContracts { get; set; }
    public int ExpiredContracts { get; set; }
    public int ExpiringIn30Days { get; set; }
    public int ExpiringIn90Days { get; set; }
    public decimal TotalMonthlyRevenue { get; set; }
    public decimal TotalAnnualRevenue { get; set; }
    public decimal TotalPaidAmount { get; set; }
    public decimal TotalOutstandingAmount { get; set; }
}
