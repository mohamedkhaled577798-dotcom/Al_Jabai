using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WaqfSystem.Infrastructure.Data;

namespace WaqfSystem.Infrastructure.BackgroundJobs
{
    public class ScheduleOverdueChecker
    {
        private readonly WaqfDbContext _db;
        private readonly ILogger<ScheduleOverdueChecker> _logger;

        public ScheduleOverdueChecker(WaqfDbContext db, ILogger<ScheduleOverdueChecker> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            var today = DateTime.Today;
            var overdue = await _db.RentPaymentSchedules
                .AsNoTracking()
                .Where(x => !x.IsDeleted && !x.IsPaid && x.DueDate.Date < today)
                .ToListAsync();

            _logger.LogInformation("عدد الدفعات المتأخرة: {Count}", overdue.Count);
        }
    }
}
