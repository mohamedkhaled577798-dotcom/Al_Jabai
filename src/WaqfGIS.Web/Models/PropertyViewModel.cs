using System.ComponentModel.DataAnnotations;

namespace WaqfGIS.Web.Models;

public class PropertyViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "الاسم بالعربية مطلوب")]
    [Display(Name = "الاسم بالعربية")]
    public string NameAr { get; set; } = string.Empty;

    [Display(Name = "الاسم بالإنجليزية")]
    public string? NameEn { get; set; }

    [Required(ErrorMessage = "الدائرة الوقفية مطلوبة")]
    [Display(Name = "الدائرة الوقفية")]
    public int WaqfOfficeId { get; set; }

    [Required(ErrorMessage = "نوع العقار مطلوب")]
    [Display(Name = "نوع العقار")]
    public int PropertyTypeId { get; set; }

    [Display(Name = "نوع الاستخدام")]
    public int? UsageTypeId { get; set; }

    [Required(ErrorMessage = "المحافظة مطلوبة")]
    [Display(Name = "المحافظة")]
    public int ProvinceId { get; set; }

    [Display(Name = "القضاء")]
    public int? DistrictId { get; set; }

    [Required(ErrorMessage = "خط العرض مطلوب")]
    [Display(Name = "خط العرض")]
    public double Latitude { get; set; }

    [Required(ErrorMessage = "خط الطول مطلوب")]
    [Display(Name = "خط الطول")]
    public double Longitude { get; set; }

    [Display(Name = "العنوان")]
    public string? Address { get; set; }

    [Display(Name = "الحي/المنطقة")]
    public string? Neighborhood { get; set; }

    [Display(Name = "المساحة الكلية (م²)")]
    public decimal? AreaSqm { get; set; }

    [Display(Name = "المساحة المبنية (م²)")]
    public decimal? BuiltAreaSqm { get; set; }

    [Display(Name = "عدد الطوابق")]
    public int? FloorsCount { get; set; }

    [Display(Name = "عدد الغرف")]
    public int? RoomsCount { get; set; }

    [Display(Name = "القيمة التقديرية")]
    public decimal? EstimatedValue { get; set; }

    [Display(Name = "رقم الصك")]
    public string? DeedNumber { get; set; }

    [Display(Name = "نوع الملكية")]
    public string? OwnershipType { get; set; }

    [Display(Name = "حالة الإيجار")]
    public string? RentalStatus { get; set; }

    [Display(Name = "الإيجار الشهري")]
    public decimal? MonthlyRent { get; set; }

    [Display(Name = "اسم المستأجر")]
    public string? TenantName { get; set; }

    [Display(Name = "هاتف المستأجر")]
    public string? TenantPhone { get; set; }

    [Display(Name = "حالة العقار")]
    public string? ConditionStatus { get; set; }

    [Display(Name = "ملاحظات")]
    public string? Notes { get; set; }

    // ================== الأهلية وشرط الوقف ==================
    [Display(Name = "طبيعة الوقف")]
    public string WaqfNature { get; set; } = "Khairi";

    [Display(Name = "مستلمة إدارياً")]
    public bool? IsAdminReceived { get; set; }

    [Display(Name = "شرط الوقف")]
    public string WaqfCondition { get; set; } = "WithoutCondition";

    // ================== بيانات الواقف الأصلي ==================
    [Display(Name = "اسم الواقف")]
    public string? WaqifName { get; set; }

    [Display(Name = "تاريخ وثيقة الوقف")]
    [DataType(DataType.Date)]
    public DateTime? WaqfDocumentDate { get; set; }

    [Display(Name = "رقم وثيقة الوقف")]
    public string? WaqfDocumentNumber { get; set; }

    [Display(Name = "نص شرط الواقف")]
    public string? WaqfConditionText { get; set; }

    // ================== تجاوزات ==================
    [Display(Name = "يوجد تجاوز")]
    public bool HasEncroachment { get; set; } = false;

    [Display(Name = "ملاحظات التجاوزات")]
    public string? EncroachmentNotes { get; set; }

    // ================== نظام الإشغال السكني ==================
    [Display(Name = "مشغول سكنياً")]
    public bool IsResidentialOccupancy { get; set; } = false;

    [Display(Name = "اسم الموظف الشاغل للسكن")]
    public string? OccupantEmployeeName { get; set; }

    [Display(Name = "رقم الهوية الوطنية")]
    public string? OccupantNationalId { get; set; }

    [Display(Name = "هاتف الموظف")]
    public string? OccupantPhone { get; set; }

    [Display(Name = "الوزارة / الجهة")]
    public string? OccupantMinistry { get; set; }

    [Display(Name = "تاريخ بدء الإشغال")]
    [DataType(DataType.Date)]
    public DateTime? OccupantStartDate { get; set; }

    [Display(Name = "الموظف متقاعد")]
    public bool? IsOccupantRetired { get; set; }

    [Display(Name = "ملاحظات الإشغال")]
    public string? OccupancyNotes { get; set; }

    // قوائم منسدلة
    public static List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> WaqfNaturesList => new()
    {
        new() { Text = "خيري", Value = "Khairi" },
        new() { Text = "أهلي", Value = "Ahli" }
    };

    public static List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> WaqfConditionsList => new()
    {
        new() { Text = "بدون شرط", Value = "WithoutCondition" },
        new() { Text = "بشرط", Value = "WithCondition" }
    };
}
