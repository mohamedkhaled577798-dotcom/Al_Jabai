using System;
using System.Collections.Generic;
using WaqfSystem.Core.Enums;

namespace WaqfSystem.Core.Entities
{
    /// <summary>
    /// دور — System role.
    /// </summary>
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? DisplayNameAr { get; set; }
        public string? DisplayNameEn { get; set; }
        public string? Description { get; set; }
        public bool IsSystemRole { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Color { get; set; }
        public string? Icon { get; set; }
        public GeographicScopeLevel GeographicScopeLevel { get; set; } = GeographicScopeLevel.None;
        public bool HasGlobalScope { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? CreatedById { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public virtual ICollection<RolePermission> Permissions { get; set; } = new List<RolePermission>();
        public virtual ICollection<User> Users { get; set; } = new List<User>();
        public virtual User? CreatedBy { get; set; }
    }
}
