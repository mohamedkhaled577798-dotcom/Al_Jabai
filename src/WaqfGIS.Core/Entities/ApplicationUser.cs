using Microsoft.AspNetCore.Identity;
using WaqfGIS.Core.Enums;

namespace WaqfGIS.Core.Entities;

/// <summary>
/// المستخدم
/// </summary>
public class ApplicationUser : IdentityUser
{
    public int? WaqfOfficeId { get; set; }
    
    /// <summary>
    /// المحافظة المسؤول عنها (للصلاحية على مستوى المحافظة)
    /// </summary>
    public int? ProvinceId { get; set; }

    /// <summary>
    /// مستوى الصلاحية
    /// </summary>
    public PermissionLevel PermissionLevel { get; set; } = PermissionLevel.ViewOnly;

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
    public virtual Province? Province { get; set; }

    /// <summary>
    /// هل المستخدم لديه صلاحية كاملة؟
    /// </summary>
    public bool HasFullAccess => PermissionLevel == PermissionLevel.SuperAdmin || PermissionLevel == PermissionLevel.Admin;

    /// <summary>
    /// هل المستخدم لديه صلاحية على المحافظة المحددة؟
    /// </summary>
    public bool HasProvinceAccess(int provinceId)
    {
        if (HasFullAccess) return true;
        if (PermissionLevel == PermissionLevel.ProvinceLevel && ProvinceId == provinceId) return true;
        return false;
    }

    /// <summary>
    /// هل المستخدم لديه صلاحية على الدائرة المحددة؟
    /// </summary>
    public bool HasOfficeAccess(int officeId)
    {
        if (HasFullAccess) return true;
        if (PermissionLevel == PermissionLevel.OfficeLevel && WaqfOfficeId == officeId) return true;
        return false;
    }
}
