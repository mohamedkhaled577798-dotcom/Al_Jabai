using System;
using System.Text;
using System.Threading.Tasks;
using WaqfSystem.Core.Interfaces;

namespace WaqfSystem.Infrastructure.Services
{
    public class ReportService : IReportService
    {
        public Task<byte[]> GeneratePartnerStatementAsync(long partnershipId, DateTime? from = null, DateTime? to = null)
        {
            var body = $"كشف حساب شراكة رقم {partnershipId} للفترة من {(from?.ToString("yyyy/MM/dd") ?? "-")} إلى {(to?.ToString("yyyy/MM/dd") ?? "-")}";
            var bytes = Encoding.UTF8.GetBytes(body);
            return Task.FromResult(bytes);
        }
    }
}
