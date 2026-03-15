using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WaqfSystem.Application.Services;
using WaqfSystem.Core.Entities;
using WaqfSystem.Core.Interfaces;

namespace WaqfSystem.Infrastructure.BackgroundJobs
{
    public class PartnershipExpiryJob : IPartnershipExpiryJob
    {
        private readonly IPartnershipService _partnershipService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PartnershipExpiryJob> _logger;

        public PartnershipExpiryJob(
            IPartnershipService partnershipService,
            IUnitOfWork unitOfWork,
            ILogger<PartnershipExpiryJob> logger)
        {
            _partnershipService = partnershipService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            try
            {
                await _partnershipService.ProcessExpiryNotificationsAsync();

                var today = DateTime.Today;
                var expired = await _unitOfWork.GetQueryable<PropertyPartnership>()
                    .Where(x => x.IsActive && !x.IsDeleted &&
                                ((x.PartnershipEndDate != null && x.PartnershipEndDate < today) ||
                                 (x.UsufructEndDate != null && x.UsufructEndDate < today)))
                    .ToListAsync();

                foreach (var item in expired)
                {
                    item.IsActive = false;
                    item.DeactivationReason = "انتهاء مدة الشراكة تلقائياً";
                    item.DeactivatedAt = DateTime.UtcNow;
                    item.UpdatedAt = DateTime.UtcNow;

                    await _unitOfWork.AddAsync(new AuditLog
                    {
                        TableName = "PropertyPartnerships",
                        RecordId = item.Id,
                        Action = "AUTO_EXPIRE",
                        NewValues = "{\"IsActive\":false}",
                        CreatedAt = DateTime.UtcNow,
                        UserId = 1
                    });

                    await _unitOfWork.UpdateAsync(item);
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Partnership expiry job completed: {WarningsSent} warnings sent, {Expired} expired", 0, expired.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Partnership expiry job failed");
                throw;
            }
        }
    }
}
