using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WaqfGIS.Web.Models;

/// <summary>
/// نموذج عرض أرض الوقف
/// </summary>
public class WaqfLandViewModel
{
    public int Id { get; set; }
    
    [Display(Name = "الكود")]
    public string? Code { get; set; }
    
    [Required(ErrorMessage = "اسم الأرض بالعربية مطلوب")]
    [Display(Name = "الاسم بالعربية")]
    [StringLength(200)]
    public string NameAr { get; set; } = string.Empty;
    
    [Display(Name = "الاسم بالإنجليزية")]
    [StringLength(200)]
    public string? NameEn { get; set; }
    
    [Display(Name = "الوصف")]
    [StringLength(1000)]
    public string? Description { get; set; }
    
    // Location
    [Required(ErrorMessage = "المحافظة مطلوبة")]
    [Display(Name = "المحافظة")]
    public int ProvinceId { get; set; }
    
    [Display(Name = "القضاء")]
    public int? DistrictId { get; set; }
    
    [Display(Name = "دائرة الوقف")]
    public int? WaqfOfficeId { get; set; }
    
    [Display(Name = "العنوان")]
    [StringLength(500)]
    public string? Address { get; set; }
    
    [Display(Name = "المحلة/الحي")]
    [StringLength(200)]
    public string? Neighborhood { get; set; }
    
    // Classification
    [Display(Name = "نوع الأرض")]
    public string LandType { get; set; } = "Waqf";
    
    [Display(Name = "استخدام الأرض")]
    public string LandUse { get; set; } = "Vacant";
    
    [Display(Name = "كود التنظيم")]
    [StringLength(50)]
    public string? ZoningCode { get; set; }
    
    // Area
    [Display(Name = "المساحة المحسوبة (م²)")]
    public double? CalculatedAreaSqm { get; set; }
    
    [Display(Name = "المساحة القانونية (م²)")]
    [Range(0, 10000000)]
    public decimal? LegalAreaSqm { get; set; }
    
    [Display(Name = "المساحة (دونم)")]
    [Range(0, 100000)]
    public decimal? AreaDonum { get; set; }
    
    [Display(Name = "المحيط (م)")]
    public double? PerimeterMeters { get; set; }
    
    // Legal
    [Display(Name = "رقم السند")]
    [StringLength(100)]
    public string? DeedNumber { get; set; }
    
    [Display(Name = "رقم التسجيل")]
    [StringLength(100)]
    public string? RegistrationNumber { get; set; }
    
    [Display(Name = "تاريخ التسجيل")]
    [DataType(DataType.Date)]
    public DateTime? RegistrationDate { get; set; }
    
    [Display(Name = "حالة الملكية")]
    [StringLength(100)]
    public string? OwnershipStatus { get; set; }
    
    // Financial
    [Display(Name = "القيمة التقديرية")]
    [Range(0, 999999999999)]
    public decimal? EstimatedValue { get; set; }
    
    [Display(Name = "الإيراد السنوي")]
    [Range(0, 999999999)]
    public decimal? AnnualRevenue { get; set; }
    
    // Geometry (GeoJSON)
    [Display(Name = "حدود الأرض")]
    public string? BoundaryGeoJson { get; set; }
    
    [Display(Name = "نقطة المركز")]
    public double? CenterLatitude { get; set; }
    public double? CenterLongitude { get; set; }
    
    [Display(Name = "ملاحظات")]
    [StringLength(2000)]
    public string? Notes { get; set; }

    // ================== الأهلية وشرط الوقف ==================
    [Display(Name = "طبيعة الوقف")]

    [Display(Name = "مستلمة إدارياً")]
    public bool? IsAdminReceived { get; set; }

    [Display(Name = "شرط الوقف")]
    public string WaqfCondition { get; set; } = "WithoutCondition";

    // ================== تجاوزات ==================
    [Display(Name = "يوجد تجاوز")]
    public bool HasEncroachment { get; set; } = false;

    [Display(Name = "ملاحظات التجاوزات")]
    [StringLength(2000)]
    public string? EncroachmentNotes { get; set; }

    // For Display
    public string? ProvinceName { get; set; }
    public string? DistrictName { get; set; }
    public string? WaqfOfficeName { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    
    // Dropdowns
    public SelectList? Provinces { get; set; }
    public SelectList? Districts { get; set; }
    public SelectList? WaqfOffices { get; set; }
    
    // Land Types
    public static List<SelectListItem> LandTypes => new()
    {
        new SelectListItem("وقف", "Waqf"),
        new SelectListItem("مؤجرة", "Leased"),
        new SelectListItem("متنازع عليها", "Disputed"),
        new SelectListItem("محجوزة", "Reserved"),
        new SelectListItem("مستثمرة", "Invested"),
        new SelectListItem("مباعة", "Sold")
    };
    
    // Land Uses
    public static List<SelectListItem> LandUses => new()
    {
        new SelectListItem("شاغرة", "Vacant"),
        new SelectListItem("زراعية", "Agricultural"),
        new SelectListItem("تجارية", "Commercial"),
        new SelectListItem("سكنية", "Residential"),
        new SelectListItem("صناعية", "Industrial"),
        new SelectListItem("مختلطة", "Mixed"),
        new SelectListItem("دينية", "Religious"),
        new SelectListItem("خدمية", "Service")
    };
    
    // Ownership Statuses
    public static List<SelectListItem> OwnershipStatuses => new()
    {
        new SelectListItem("مسجل", "Registered"),
        new SelectListItem("غير مسجل", "Unregistered"),
        new SelectListItem("قيد التسجيل", "Pending"),
        new SelectListItem("متنازع عليه", "Disputed"),
        new SelectListItem("موروث", "Inherited")
    };

    public static List<SelectListItem> WaqfNaturesList => new()
    {
        new SelectListItem("خيري", "Khairi"),
        new SelectListItem("أهلي", "Ahli")
    };

    public static List<SelectListItem> WaqfConditionsList => new()
    {
        new SelectListItem("بدون شرط", "WithoutCondition"),
        new SelectListItem("بشرط", "WithCondition")
    };
}

/// <summary>
/// نموذج البحث في أراضي الوقف
/// </summary>
public class WaqfLandSearchViewModel
{
    public string? SearchTerm { get; set; }
    public int? ProvinceId { get; set; }
    public int? DistrictId { get; set; }
    public int? WaqfOfficeId { get; set; }
    public string? LandType { get; set; }
    public string? LandUse { get; set; }
    public decimal? MinArea { get; set; }
    public decimal? MaxArea { get; set; }
    public bool? HasBoundary { get; set; }
    
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}
