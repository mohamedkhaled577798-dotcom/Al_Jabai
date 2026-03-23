using System.Collections.Generic;
using System.Threading.Tasks;
using WaqfSystem.Application.DTOs.Common;
using WaqfSystem.Application.DTOs.Revenue;
using WaqfSystem.Core.Entities;

namespace WaqfSystem.Application.Services
{
    public interface IRevenueCollectionService
    {
        Task<PagedResult<PropertyRevenue>> GetPagedAsync(RevenueFilterRequest filter, int userId);
        Task<PropertyRevenue> CollectAsync(CollectRevenueDto dto, int userId);
        Task<PropertyRevenue> QuickCollectAsync(QuickCollectDto dto, int userId);
        Task<VarianceAlertDto> PreviewVarianceAsync(long? contractId, decimal enteredAmount);
        Task<(bool HasCollision, string Message, string? LockedBy)> CheckCollisionAsync(long propertyId, string level, long? floorId, long? unitId, string periodLabel);
        Task<List<PropertyRevenue>> GetRecentAsync(int userId, int take = 20);
        Task DeleteTodayAsync(long revenueId, string reason, int userId);
    }
}
