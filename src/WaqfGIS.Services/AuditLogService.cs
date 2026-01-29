using WaqfGIS.Core.Entities;
using WaqfGIS.Core.Interfaces;

namespace WaqfGIS.Services;

/// <summary>
/// خدمة سجل التدقيق
/// </summary>
public class AuditLogService
{
    private readonly IUnitOfWork _unitOfWork;

    public AuditLogService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task LogAsync(string action, string entityType, int entityId, string? entityName, 
        string? userId, string? userName, string? oldValues = null, string? newValues = null, string? ipAddress = null)
    {
        var log = new AuditLog
        {
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            EntityName = entityName,
            UserId = userId,
            UserName = userName,
            OldValues = oldValues,
            NewValues = newValues,
            IpAddress = ipAddress,
            Timestamp = DateTime.UtcNow
        };

        await _unitOfWork.AuditLogs.AddAsync(log);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task LogCreateAsync(string entityType, int entityId, string? entityName, string? userId, string? userName, string? ipAddress = null)
    {
        await LogAsync("إنشاء", entityType, entityId, entityName, userId, userName, ipAddress: ipAddress);
    }

    public async Task LogUpdateAsync(string entityType, int entityId, string? entityName, string? userId, string? userName, string? oldValues = null, string? newValues = null, string? ipAddress = null)
    {
        await LogAsync("تعديل", entityType, entityId, entityName, userId, userName, oldValues, newValues, ipAddress);
    }

    public async Task LogDeleteAsync(string entityType, int entityId, string? entityName, string? userId, string? userName, string? ipAddress = null)
    {
        await LogAsync("حذف", entityType, entityId, entityName, userId, userName, ipAddress: ipAddress);
    }

    public async Task<IEnumerable<AuditLog>> GetLogsAsync(string? entityType = null, int? entityId = null, int take = 100)
    {
        var query = _unitOfWork.AuditLogs.Query().OrderByDescending(l => l.Timestamp);

        if (!string.IsNullOrEmpty(entityType))
            query = (IOrderedQueryable<AuditLog>)query.Where(l => l.EntityType == entityType);

        if (entityId.HasValue)
            query = (IOrderedQueryable<AuditLog>)query.Where(l => l.EntityId == entityId.Value);

        return await Task.FromResult(query.Take(take).ToList());
    }

    public async Task<IEnumerable<AuditLog>> GetUserLogsAsync(string userId, int take = 100)
    {
        return await Task.FromResult(_unitOfWork.AuditLogs.Query()
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.Timestamp)
            .Take(take)
            .ToList());
    }

    public async Task<IEnumerable<AuditLog>> GetRecentLogsAsync(int take = 50)
    {
        return await Task.FromResult(_unitOfWork.AuditLogs.Query()
            .OrderByDescending(l => l.Timestamp)
            .Take(take)
            .ToList());
    }
}
