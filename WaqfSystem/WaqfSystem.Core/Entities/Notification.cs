using System;
using WaqfSystem.Core.Enums;

namespace WaqfSystem.Core.Entities
{
    /// <summary>
    /// إشعار — System notification for users.
    /// </summary>
    public class Notification : BaseEntity
    {
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType NotificationType { get; set; } = NotificationType.SystemAlert;
        public string? ReferenceTable { get; set; }
        public int? ReferenceId { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }

        // Navigation
        public virtual User User { get; set; } = null!;
    }
}
