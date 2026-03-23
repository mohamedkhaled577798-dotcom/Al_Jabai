using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WaqfSystem.Core.Enums;
using WaqfSystem.Infrastructure.Data;

namespace WaqfSystem.Infrastructure.BackgroundJobs
{
    public class ContractExpiryNotifier
    {
        private readonly WaqfDbContext _db;
        private readonly ILogger<ContractExpiryNotifier> _logger;

        public ContractExpiryNotifier(WaqfDbContext db, ILogger<ContractExpiryNotifier> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            var date = DateTime.Today.AddDays(30);
            var expiring = await _db.RentContracts
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.Status == ContractStatus.Active && x.EndDate.Date <= date)
                .ToListAsync();

            _logger.LogInformation("عدد العقود القريبة من الانتهاء: {Count}", expiring.Count);
        }
    }
}
