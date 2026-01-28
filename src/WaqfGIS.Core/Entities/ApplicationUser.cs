using Microsoft.AspNetCore.Identity;

namespace WaqfGIS.Core.Entities;

/// <summary>
/// المستخدم
/// </summary>
public class ApplicationUser : IdentityUser
{
    public int? WaqfOfficeId { get; set; }
    public string FullNameAr { get; set; } = string.Empty;
    public string? FullNameEn { get; set; } = string.Empty;
    public string? JobTitle { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;
    public bool MustChangePassword { get; set; } = true;
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LockedUntil { get; set; }
    public DateTime? LastLogin { get; set; }
    public DateTime? LastPasswordChange { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }

    // Navigation Properties
    public virtual WaqfOffice? WaqfOffice { get; set; }
}
