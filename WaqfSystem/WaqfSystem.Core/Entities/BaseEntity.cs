using System;

namespace WaqfSystem.Core.Entities
{
    /// <summary>
    /// Base entity with common audit fields shared by all domain entities.
    /// </summary>
    public abstract class BaseEntity
    {
        public int Id { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public int CreatedById { get; set; }
        public virtual User? CreatedBy { get; set; }
        public int? UpdatedById { get; set; }
        public virtual User? UpdatedBy { get; set; }
    }
}
