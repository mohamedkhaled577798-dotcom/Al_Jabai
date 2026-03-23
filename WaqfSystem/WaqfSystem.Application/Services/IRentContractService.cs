using System.Collections.Generic;
using System.Threading.Tasks;
using WaqfSystem.Application.DTOs.Common;
using WaqfSystem.Application.DTOs.Revenue;
using WaqfSystem.Core.Entities;

namespace WaqfSystem.Application.Services
{
    public interface IRentContractService
    {
        Task<PagedResult<RentContract>> GetPagedAsync(ContractFilterRequest filter, int userId);
        Task<RentContract?> GetByIdAsync(long id, int userId);
        Task<RentContract> CreateAsync(CreateContractDto dto, int userId);
        Task TerminateAsync(TerminateContractDto dto, int userId);
        Task<RentContract?> GetActiveByUnitIdAsync(long unitId, int userId);
        Task<List<RentContract>> GetExpiringAsync(int days, int userId);
        Task<List<RentPaymentSchedule>> GetScheduleAsync(long contractId, int userId);
    }
}
