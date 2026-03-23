using System;
using System.Threading.Tasks;

namespace WaqfSystem.Application.Services
{
    public interface IRevenueReportService
    {
        Task<byte[]> GenerateDailyCollectionReportAsync(DateTime date, int userId);
        Task<byte[]> GenerateBatchReceiptPdfAsync(string batchCode);
        Task<byte[]> GenerateMonthlyStatementAsync(long propertyId, string periodLabel);
    }
}
