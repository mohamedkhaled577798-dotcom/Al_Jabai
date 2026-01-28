namespace WaqfGIS.Core.Entities;

/// <summary>
/// سجل التدقيق
/// </summary>
public class AuditLog : BaseEntity
{
    public long Id { get; set; }
    public string? UserId { get; set; }
    public string Action { get; set; } = string.Empty; // INSERT, UPDATE, DELETE, LOGIN, LOGOUT
    public string? TableName { get; set; }
    public int? RecordId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public virtual ApplicationUser? User { get; set; }
}
