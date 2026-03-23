using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using WaqfSystem.Application.Services;
using WaqfSystem.Infrastructure.Data;

namespace WaqfSystem.Infrastructure.BackgroundJobs
{
    public class SmartSuggestionRefresher
    {
        private readonly WaqfDbContext _db;
        private readonly ISmartCollectionService _smartCollectionService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<SmartSuggestionRefresher> _logger;

        public SmartSuggestionRefresher(
            WaqfDbContext db,
            ISmartCollectionService smartCollectionService,
            IMemoryCache cache,
            ILogger<SmartSuggestionRefresher> logger)
        {
            _db = db;
            _smartCollectionService = smartCollectionService;
            _cache = cache;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            var period = DateTime.Today.ToString("yyyy-MM", CultureInfo.InvariantCulture);
            var collectors = await _db.Users
                .AsNoTracking()
                .Where(x => x.IsActive && !x.IsDeleted)
                .Select(x => x.Id)
                .ToListAsync();

            foreach (var userId in collectors)
            {
                var list = await _smartCollectionService.GetSuggestionsAsync(userId, period);
                _cache.Set($"suggestions_{userId}_{period}", list, TimeSpan.FromHours(4));

                var overdueCritical = list.Where(x => x.OverdueDays > 3).ToList();
                if (overdueCritical.Count > 0)
                {
                    _logger.LogInformation("إشعار تحصيل متأخر للمستخدم {UserId} بعدد {Count}", userId, overdueCritical.Count);
                }
            }
        }
    }
}
