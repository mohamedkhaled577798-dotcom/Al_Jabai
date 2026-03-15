using System;
using System.Collections.Generic;
using WaqfSystem.Core.Enums;

namespace WaqfSystem.Core.Entities
{
    public class Permission
    {
        public int Id { get; set; }
        public string PermissionKey { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string DisplayNameAr { get; set; } = string.Empty;
        public string DisplayNameEn { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }

    public class RolePermission
    {
        public int RoleId { get; set; }
        public int PermissionId { get; set; }
        public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
        public int GrantedById { get; set; }

        public virtual Role Role { get; set; } = null!;
        public virtual Permission Permission { get; set; } = null!;
        public virtual User GrantedBy { get; set; } = null!;
    }

    public class UserGeographicScope
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public GeographicScopeLevel ScopeLevel { get; set; }
        public int? GovernorateId { get; set; }
        public int? DistrictId { get; set; }
        public int? SubDistrictId { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? CreatedById { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual Governorate? Governorate { get; set; }
        public virtual District? District { get; set; }
        public virtual SubDistrict? SubDistrict { get; set; }
        public virtual User? CreatedBy { get; set; }
    }
}
