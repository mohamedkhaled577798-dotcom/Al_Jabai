using System;
using System.Threading.Tasks;

namespace WaqfSystem.Core.Interfaces
{
    public interface IReportService
    {
        Task<byte[]> GeneratePartnerStatementAsync(long partnershipId, DateTime? from = null, DateTime? to = null);
    }
}
