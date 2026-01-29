using System.ComponentModel.DataAnnotations;
using WaqfGIS.Core.Enums;

namespace WaqfGIS.Web.Models;

// قائمة المستخدمين
public class UserListViewModel
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string FullNameAr { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? OfficeName { get; set; }
    public string? ProvinceName { get; set; }
    public PermissionLevel PermissionLevel { get; set; }
    public string PermissionLevelText { get; set; } = string.Empty;
    public string Roles { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? LastLogin { get; set; }
}

// إنشاء مستخدم جديد
public class CreateUserViewModel
{
    [Required(ErrorMessage = "اسم المستخدم مطلوب")]
    [Display(Name = "اسم المستخدم")]
    [StringLength(50, MinimumLength = 3)]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
    [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
    [Display(Name = "البريد الإلكتروني")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "كلمة المرور مطلوبة")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "كلمة المرور يجب أن تكون 8 أحرف على الأقل")]
    [DataType(DataType.Password)]
    [Display(Name = "كلمة المرور")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "تأكيد كلمة المرور")]
    [Compare("Password", ErrorMessage = "كلمة المرور غير متطابقة")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "الاسم الكامل مطلوب")]
    [Display(Name = "الاسم الكامل بالعربية")]
    public string FullNameAr { get; set; } = string.Empty;

    [Display(Name = "الاسم بالإنجليزية")]
    public string? FullNameEn { get; set; }

    [Display(Name = "رقم الهاتف")]
    public string? Phone { get; set; }

    [Display(Name = "المسمى الوظيفي")]
    public string? JobTitle { get; set; }

    [Display(Name = "الدائرة الوقفية")]
    public int? WaqfOfficeId { get; set; }

    [Display(Name = "المحافظة")]
    public int? ProvinceId { get; set; }

    [Required(ErrorMessage = "مستوى الصلاحية مطلوب")]
    [Display(Name = "مستوى الصلاحية")]
    public PermissionLevel PermissionLevel { get; set; }

    [Required(ErrorMessage = "الدور مطلوب")]
    [Display(Name = "الدور")]
    public string Role { get; set; } = string.Empty;
}

// تعديل مستخدم
public class EditUserViewModel
{
    public string Id { get; set; } = string.Empty;

    [Display(Name = "اسم المستخدم")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
    [EmailAddress]
    [Display(Name = "البريد الإلكتروني")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "الاسم الكامل مطلوب")]
    [Display(Name = "الاسم الكامل بالعربية")]
    public string FullNameAr { get; set; } = string.Empty;

    [Display(Name = "الاسم بالإنجليزية")]
    public string? FullNameEn { get; set; }

    [Display(Name = "رقم الهاتف")]
    public string? Phone { get; set; }

    [Display(Name = "المسمى الوظيفي")]
    public string? JobTitle { get; set; }

    [Display(Name = "الدائرة الوقفية")]
    public int? WaqfOfficeId { get; set; }

    [Display(Name = "المحافظة")]
    public int? ProvinceId { get; set; }

    [Display(Name = "مستوى الصلاحية")]
    public PermissionLevel PermissionLevel { get; set; }

    [Display(Name = "الدور")]
    public string? Role { get; set; }

    [Display(Name = "نشط")]
    public bool IsActive { get; set; }
}
