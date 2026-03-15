using System;

namespace WaqfSystem.Core.Entities
{
    /// <summary>
    /// سجل تدقيق — Audit log for tracking all data changes.
    /// </summary>
    public class AuditLog
    {
        public long Id { get; set; }
        public string TableName { get; set; } = string.Empty;
        public int RecordId { get; set; }
        public string Action { get; set; } = string.Empty; // INSERT/UPDATE/DELETE
        public string? OldValues { get; set; } // JSON
        public string? NewValues { get; set; } // JSON
        public string? ChangedColumns { get; set; } // JSON array
        public int? UserId { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual User? User { get; set; }
    }
}
