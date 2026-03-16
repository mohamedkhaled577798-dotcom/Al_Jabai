using System;
using System.Collections.Generic;

namespace WaqfSystem.Core.Entities
{
    /// <summary>
    /// مستخدم النظام — System user with role and geographic scope.
    /// </summary>
    public class User
    {
        public int Id { get; set; }
        public string FullNameAr { get; set; } = string.Empty;
        public string? FullNameEn { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Phone2 { get; set; }
        public string? NationalId { get; set; }
        public string? JobTitle { get; set; }
        public int RoleId { get; set; }
        public int? GovernorateId { get; set; }
        public int? DistrictId { get; set; }
        public int? SubDistrictId { get; set; }
        public int? TeamId { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsLocked { get; set; }
        public string? LockReason { get; set; }
        public DateTime? LockedAt { get; set; }
        public int? LockedById { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public string? LastLoginIp { get; set; }
        public int FailedLoginCount { get; set; }
        public DateTime? PasswordChangedAt { get; set; }
        public bool MustChangePassword { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiresAt { get; set; }
        public string? DeviceId { get; set; }
        public string? ProfilePhotoUrl { get; set; }
        public string? Notes { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public int? CreatedById { get; set; }

        // Navigation
        public virtual Role Role { get; set; } = null!;
        public virtual Governorate? Governorate { get; set; }
        public virtual District? District { get; set; }
        public virtual SubDistrict? SubDistrict { get; set; }
        public virtual User? CreatedBy { get; set; }
        public virtual User? LockedBy { get; set; }
        public virtual ICollection<RolePermission> GrantedRolePermissions { get; set; } = new List<RolePermission>();
        public virtual ICollection<UserGeographicScope> GeographicScopes { get; set; } = new List<UserGeographicScope>();
    }
}
