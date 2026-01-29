using System.ComponentModel.DataAnnotations;

namespace WaqfGIS.Web.Models;

public class OfficeViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "الاسم بالعربية مطلوب")]
    [Display(Name = "الاسم بالعربية")]
    public string NameAr { get; set; } = string.Empty;

    [Display(Name = "الاسم بالإنجليزية")]
    public string? NameEn { get; set; }

    [Required(ErrorMessage = "نوع المكتب مطلوب")]
    [Display(Name = "نوع المكتب")]
    public int OfficeTypeId { get; set; }

    [Display(Name = "المكتب الأعلى")]
    public int? ParentOfficeId { get; set; }

    [Display(Name = "المحافظة")]
    public int? ProvinceId { get; set; }

    [Display(Name = "القضاء")]
    public int? DistrictId { get; set; }

    [Display(Name = "خط العرض")]
    public double Latitude { get; set; }

    [Display(Name = "خط الطول")]
    public double Longitude { get; set; }

    [Display(Name = "العنوان")]
    public string? Address { get; set; }

    [Display(Name = "رقم الهاتف")]
    public string? Phone { get; set; }

    [Display(Name = "البريد الإلكتروني")]
    [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
    public string? Email { get; set; }

    [Display(Name = "اسم المدير")]
    public string? ManagerName { get; set; }

    [Display(Name = "هاتف المدير")]
    public string? ManagerPhone { get; set; }

    [Display(Name = "ملاحظات")]
    public string? Notes { get; set; }
}
