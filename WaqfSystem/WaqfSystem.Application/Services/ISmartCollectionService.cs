using System.Collections.Generic;
using System.Threading.Tasks;
using WaqfSystem.Application.DTOs.Common;
using WaqfSystem.Application.DTOs.Revenue;

namespace WaqfSystem.Application.Services
{
    public interface ISmartCollectionService
    {
        Task<List<SmartSuggestionDto>> GetSuggestionsAsync(int userId, string? periodLabel);
        Task<TodayDashboardDto> GetTodayDashboardAsync(int userId, string periodLabel);
        Task<PagedResult<SearchResultDto>> SearchAsync(string query, int userId, int page);
        Task<BatchCollectResultDto> BatchCollectAsync(BatchCollectDto dto, int userId);
        Task<(bool HasCollision, string Message, string? LockedBy)> CheckCollisionAsync(long propertyId, string level, long? floorId, long? unitId, string period);
        Task<List<SmartSuggestionDto>> GetPendingForBatchAsync(int userId, string periodLabel);
    }
}
