using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WaqfGIS.Core.Entities;
using WaqfGIS.Core.Enums;
using WaqfGIS.Core.Interfaces;

namespace WaqfGIS.Services;

/// <summary>
/// خدمة إدارة الصلاحيات
/// </summary>
public class PermissionService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;

    public PermissionService(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// الحصول على المستخدم الحالي مع معلومات الصلاحيات
    /// </summary>
    public async Task<ApplicationUser?> GetCurrentUserAsync(ClaimsPrincipal principal)
    {
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return null;

        return await _userManager.Users
            .Include(u => u.Province)
            .Include(u => u.WaqfOffice)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    /// <summary>
    /// هل المستخدم لديه صلاحية كاملة؟
    /// </summary>
    public async Task<bool> HasFullAccessAsync(ClaimsPrincipal principal)
    {
        var user = await GetCurrentUserAsync(principal);
        return user?.HasFullAccess ?? false;
    }

    /// <summary>
    /// هل المستخدم لديه صلاحية على المحافظة؟
    /// </summary>
    public async Task<bool> CanAccessProvinceAsync(ClaimsPrincipal principal, int provinceId)
    {
        var user = await GetCurrentUserAsync(principal);
        if (user == null) return false;

        // SuperAdmin و Admin لهم صلاحية على كل شيء
        if (user.HasFullAccess) return true;

        // مستوى المحافظة
        if (user.PermissionLevel == PermissionLevel.ProvinceLevel)
            return user.ProvinceId == provinceId;

        // مستوى الدائرة - نحتاج للتحقق من محافظة الدائرة
        if (user.PermissionLevel == PermissionLevel.OfficeLevel && user.WaqfOfficeId.HasValue)
        {
            var office = await _unitOfWork.WaqfOffices.GetByIdAsync(user.WaqfOfficeId.Value);
            return office?.ProvinceId == provinceId;
        }

        return false;
    }

    /// <summary>
    /// هل المستخدم لديه صلاحية على الدائرة؟
    /// </summary>
    public async Task<bool> CanAccessOfficeAsync(ClaimsPrincipal principal, int officeId)
    {
        var user = await GetCurrentUserAsync(principal);
        if (user == null) return false;

        if (user.HasFullAccess) return true;

        // مستوى المحافظة - يرى كل دوائر محافظته
        if (user.PermissionLevel == PermissionLevel.ProvinceLevel && user.ProvinceId.HasValue)
        {
            var office = await _unitOfWork.WaqfOffices.GetByIdAsync(officeId);
            return office?.ProvinceId == user.ProvinceId;
        }

        // مستوى الدائرة
        if (user.PermissionLevel == PermissionLevel.OfficeLevel)
            return user.WaqfOfficeId == officeId;

        return false;
    }

    /// <summary>
    /// هل المستخدم يستطيع الوصول للمسجد؟
    /// </summary>
    public async Task<bool> CanAccessMosqueAsync(ClaimsPrincipal principal, Mosque mosque)
    {
        var user = await GetCurrentUserAsync(principal);
        if (user == null) return false;

        if (user.HasFullAccess) return true;

        // مستوى المحافظة
        if (user.PermissionLevel == PermissionLevel.ProvinceLevel)
            return user.ProvinceId == mosque.ProvinceId;

        // مستوى الدائرة
        if (user.PermissionLevel == PermissionLevel.OfficeLevel)
            return user.WaqfOfficeId == mosque.WaqfOfficeId;

        // ViewOnly يستطيع العرض فقط
        if (user.PermissionLevel == PermissionLevel.ViewOnly)
            return true; // سيتم تحديد نوع العملية في مكان آخر

        return false;
    }

    /// <summary>
    /// هل المستخدم يستطيع الوصول للعقار؟
    /// </summary>
    public async Task<bool> CanAccessPropertyAsync(ClaimsPrincipal principal, WaqfProperty property)
    {
        var user = await GetCurrentUserAsync(principal);
        if (user == null) return false;

        if (user.HasFullAccess) return true;

        if (user.PermissionLevel == PermissionLevel.ProvinceLevel)
            return user.ProvinceId == property.ProvinceId;

        if (user.PermissionLevel == PermissionLevel.OfficeLevel)
            return user.WaqfOfficeId == property.WaqfOfficeId;

        if (user.PermissionLevel == PermissionLevel.ViewOnly)
            return true;

        return false;
    }

    /// <summary>
    /// الحصول على المساجد المصرح بها للمستخدم
    /// </summary>
    public async Task<IQueryable<Mosque>> GetAuthorizedMosquesAsync(ClaimsPrincipal principal)
    {
        var user = await GetCurrentUserAsync(principal);
        var query = _unitOfWork.Mosques.Query();

        if (user == null) return query.Where(m => false);

        if (user.HasFullAccess)
            return query;

        if (user.PermissionLevel == PermissionLevel.ProvinceLevel && user.ProvinceId.HasValue)
            return query.Where(m => m.ProvinceId == user.ProvinceId);

        if (user.PermissionLevel == PermissionLevel.OfficeLevel && user.WaqfOfficeId.HasValue)
            return query.Where(m => m.WaqfOfficeId == user.WaqfOfficeId);

        // ViewOnly يرى كل شيء
        if (user.PermissionLevel == PermissionLevel.ViewOnly)
            return query;

        return query.Where(m => false);
    }

    /// <summary>
    /// الحصول على العقارات المصرح بها للمستخدم
    /// </summary>
    public async Task<IQueryable<WaqfProperty>> GetAuthorizedPropertiesAsync(ClaimsPrincipal principal)
    {
        var user = await GetCurrentUserAsync(principal);
        var query = _unitOfWork.WaqfProperties.Query();

        if (user == null) return query.Where(p => false);

        if (user.HasFullAccess)
            return query;

        if (user.PermissionLevel == PermissionLevel.ProvinceLevel && user.ProvinceId.HasValue)
            return query.Where(p => p.ProvinceId == user.ProvinceId);

        if (user.PermissionLevel == PermissionLevel.OfficeLevel && user.WaqfOfficeId.HasValue)
            return query.Where(p => p.WaqfOfficeId == user.WaqfOfficeId);

        if (user.PermissionLevel == PermissionLevel.ViewOnly)
            return query;

        return query.Where(p => false);
    }

    /// <summary>
    /// الحصول على الدوائر المصرح بها للمستخدم
    /// </summary>
    public async Task<IQueryable<WaqfOffice>> GetAuthorizedOfficesAsync(ClaimsPrincipal principal)
    {
        var user = await GetCurrentUserAsync(principal);
        var query = _unitOfWork.WaqfOffices.Query();

        if (user == null) return query.Where(o => false);

        if (user.HasFullAccess)
            return query;

        if (user.PermissionLevel == PermissionLevel.ProvinceLevel && user.ProvinceId.HasValue)
            return query.Where(o => o.ProvinceId == user.ProvinceId);

        if (user.PermissionLevel == PermissionLevel.OfficeLevel && user.WaqfOfficeId.HasValue)
            return query.Where(o => o.Id == user.WaqfOfficeId);

        if (user.PermissionLevel == PermissionLevel.ViewOnly)
            return query;

        return query.Where(o => false);
    }

    /// <summary>
    /// هل المستخدم يستطيع تعديل/حذف؟
    /// </summary>
    public async Task<bool> CanEditAsync(ClaimsPrincipal principal)
    {
        var user = await GetCurrentUserAsync(principal);
        if (user == null) return false;

        // ViewOnly لا يستطيع التعديل
        return user.PermissionLevel != PermissionLevel.ViewOnly;
    }

    /// <summary>
    /// هل المستخدم يستطيع الإضافة؟
    /// </summary>
    public async Task<bool> CanCreateAsync(ClaimsPrincipal principal)
    {
        return await CanEditAsync(principal);
    }

    /// <summary>
    /// هل المستخدم يستطيع الحذف؟
    /// </summary>
    public async Task<bool> CanDeleteAsync(ClaimsPrincipal principal)
    {
        var user = await GetCurrentUserAsync(principal);
        if (user == null) return false;

        // فقط SuperAdmin و Admin يستطيعون الحذف
        return user.HasFullAccess;
    }

    /// <summary>
    /// الحصول على المحافظات المصرح بها
    /// </summary>
    public async Task<IEnumerable<Province>> GetAuthorizedProvincesAsync(ClaimsPrincipal principal)
    {
        var user = await GetCurrentUserAsync(principal);
        if (user == null) return Enumerable.Empty<Province>();

        var allProvinces = await _unitOfWork.Provinces.GetAllAsync();

        if (user.HasFullAccess || user.PermissionLevel == PermissionLevel.ViewOnly)
            return allProvinces;

        if (user.PermissionLevel == PermissionLevel.ProvinceLevel && user.ProvinceId.HasValue)
            return allProvinces.Where(p => p.Id == user.ProvinceId);

        if (user.PermissionLevel == PermissionLevel.OfficeLevel && user.WaqfOfficeId.HasValue)
        {
            var office = await _unitOfWork.WaqfOffices.GetByIdAsync(user.WaqfOfficeId.Value);
            if (office?.ProvinceId != null)
                return allProvinces.Where(p => p.Id == office.ProvinceId);
        }

        return Enumerable.Empty<Province>();
    }

    /// <summary>
    /// الحصول على وصف مستوى الصلاحية
    /// </summary>
    public static string GetPermissionLevelDescription(PermissionLevel level)
    {
        return level switch
        {
            PermissionLevel.SuperAdmin => "مدير النظام - صلاحية كاملة",
            PermissionLevel.Admin => "مدير - صلاحية إدارية",
            PermissionLevel.ProvinceLevel => "مسؤول محافظة",
            PermissionLevel.OfficeLevel => "مسؤول دائرة",
            PermissionLevel.ViewOnly => "عرض فقط",
            _ => "غير محدد"
        };
    }
}
