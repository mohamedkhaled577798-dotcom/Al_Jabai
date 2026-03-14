using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WaqfSystem.Core.Entities;
using WaqfSystem.Core.Enums;
using WaqfSystem.Core.Interfaces;
using WaqfSystem.Infrastructure.Data;

namespace WaqfSystem.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly WaqfDbContext _dbContext;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(WaqfDbContext dbContext, ILogger<NotificationService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task SendToRoleAsync(string roleCode, string title, string message, string? referenceTable = null, int? referenceId = null)
        {
            try
            {
                var userIds = await _dbContext.Users
                    .AsNoTracking()
                    .Where(u => !u.IsDeleted && u.IsActive && u.Role.Code == roleCode)
                    .Select(u => u.Id)
                    .ToListAsync();

                foreach (var userId in userIds)
                {
                    _dbContext.Notifications.Add(new Notification
                    {
                        UserId = userId,
                        Title = title,
                        Message = message,
                        NotificationType = NotificationType.SystemAlert,
                        ReferenceTable = referenceTable,
                        ReferenceId = referenceId,
                        CreatedAt = DateTime.UtcNow,
                        CreatedById = 1
                    });
                }

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send role notification to {RoleCode}", roleCode);
            }
        }
    }
}
