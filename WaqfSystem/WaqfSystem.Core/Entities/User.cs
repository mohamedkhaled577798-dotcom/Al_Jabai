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
        public string? PhoneNumber { get; set; }
        public string? NationalId { get; set; }
        public int RoleId { get; set; }
        public int? GovernorateId { get; set; }
        public int? TeamId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? LastLoginAt { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiresAt { get; set; }
        public string? DeviceId { get; set; }
        public string? ProfilePhotoUrl { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public int? CreatedById { get; set; }

        // Navigation
        public virtual Role Role { get; set; } = null!;
        public virtual Governorate? Governorate { get; set; }
        public virtual User? CreatedBy { get; set; }
    }
}
