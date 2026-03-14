using System;
using System.Collections.Generic;

namespace WaqfSystem.Core.Entities
{
    /// <summary>
    /// دور — System role.
    /// </summary>
    public class Role
    {
        public int Id { get; set; }
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
