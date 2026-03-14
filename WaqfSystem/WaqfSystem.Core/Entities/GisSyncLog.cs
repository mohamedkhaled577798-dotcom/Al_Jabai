using System;
using WaqfSystem.Core.Enums;

namespace WaqfSystem.Core.Entities
{
    /// <summary>
    /// سجل مزامنة GIS — GIS synchronization log entry.
    /// </summary>
    public class GisSyncLog : BaseEntity
    {
        public int PropertyId { get; set; }
        public GisSyncDirection Direction { get; set; } = GisSyncDirection.ToGis;
        public GisSyncStatus Status { get; set; } = GisSyncStatus.Pending;
        public string? RequestPayload { get; set; }
        public string? ResponsePayload { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        // Navigation
        public virtual Property Property { get; set; } = null!;
    }
}
