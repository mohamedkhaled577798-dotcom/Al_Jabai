using System.ComponentModel.DataAnnotations;

namespace WaqfGIS.Web.Models;

public class EncroachmentViewModel
{
    public int Id { get; set; }

    // ربط بالعقار/الأرض/المسجد
    [Required(ErrorMessage = "نوع الكيان مطلوب")]
    [Display(Name = "نوع العنصر")]
    public string EntityType { get; set; } = "WaqfProperty";

    [Required(ErrorMessage = "المعرف مطلوب")]
    [Display(Name = "رقم العنصر")]
    public int EntityId { get; set; }

    [Display(Name = "اسم العنصر")]
    public string? EntityName { get; set; }

    [Display(Name = "المحافظة")]
    public int? ProvinceId { get; set; }

    // التصنيف
    [Required(ErrorMessage = "نوع التجاوز مطلوب")]
    [Display(Name = "نوع التجاوز")]
    public string EncroachmentType { get; set; } = string.Empty;

    [Display(Name = "درجة الخطورة")]
    public string Severity { get; set; } = "متوسط";

    [Required(ErrorMessage = "وصف التجاوز مطلوب")]
    [Display(Name = "وصف التجاوز")]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "مساحة التجاوز (م²)")]
    public decimal? EncroachmentAreaSqm { get; set; }

    // المتجاوز
    [Display(Name = "اسم المتجاوز")]
    public string? EncroachwrName { get; set; }

    [Display(Name = "هاتف المتجاوز")]
    public string? EncroachwrPhone { get; set; }

    [Display(Name = "هوية المتجاوز")]
    public string? EncroachwrNationalId { get; set; }

    // الموقع
    [Display(Name = "خط العرض")]
    public double? Latitude { get; set; }

    [Display(Name = "خط الطول")]
    public double? Longitude { get; set; }

    [Display(Name = "وصف الموقع")]
    public string? LocationDescription { get; set; }

    // التواريخ
    [Required(ErrorMessage = "تاريخ الاكتشاف مطلوب")]
    [Display(Name = "تاريخ الاكتشاف")]
    [DataType(DataType.Date)]
    public DateTime DiscoveryDate { get; set; } = DateTime.Today;

    [Display(Name = "تاريخ التبليغ")]
    [DataType(DataType.Date)]
    public DateTime? ReportDate { get; set; }

    [Display(Name = "تاريخ الإزالة")]
    [DataType(DataType.Date)]
    public DateTime? RemovalDate { get; set; }

    // الحالة والإجراءات
    [Display(Name = "الحالة")]
    public string Status { get; set; } = "قائم";

    [Display(Name = "الإجراءات المتخذة")]
    public string? ActionTaken { get; set; }

    [Display(Name = "أُبلِّغت الجهات المعنية")]
    public bool IsReportedToAuthorities { get; set; } = false;

    [Display(Name = "رقم الكتاب الرسمي")]
    public string? ReportReferenceNumber { get; set; }

    [Display(Name = "تاريخ التبليغ الرسمي")]
    [DataType(DataType.Date)]
    public DateTime? AuthorityReportDate { get; set; }

    // ربط بنزاع قانوني
    [Display(Name = "له قضية قانونية")]
    public bool HasLegalCase { get; set; } = false;

    [Display(Name = "رقم النزاع القانوني")]
    public int? LegalDisputeId { get; set; }

    [Display(Name = "ملاحظات")]
    public string? Notes { get; set; }

    // للعرض
    public string? ProvinceName { get; set; }
    public string? EntityTypeLabel => EntityType switch {
        "WaqfProperty" => "عقار وقفي",
        "WaqfLand" => "أرض وقفية",
        "Mosque" => "مسجد",
        _ => EntityType
    };

    // قوائم ثابتة
    public static List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> EncroachmentTypesList => new()
    {
        new() { Text = "بناء غير مرخص", Value = "بناء غير مرخص" },
        new() { Text = "شق طريق أو ممر", Value = "شق طريق أو ممر" },
        new() { Text = "وضع يد", Value = "وضع يد" },
        new() { Text = "استغلال تجاري", Value = "استغلال تجاري" },
        new() { Text = "استغلال سكني", Value = "استغلال سكني" },
        new() { Text = "استغلال زراعي", Value = "استغلال زراعي" },
        new() { Text = "إشغال حكومي", Value = "إشغال حكومي" },
        new() { Text = "تلوث أو إلقاء مخلفات", Value = "تلوث أو إلقاء مخلفات" },
        new() { Text = "أخرى", Value = "أخرى" },
    };

    public static List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> SeverityList => new()
    {
        new() { Text = "بسيط", Value = "بسيط" },
        new() { Text = "متوسط", Value = "متوسط" },
        new() { Text = "خطير", Value = "خطير" },
        new() { Text = "بالغ الخطورة", Value = "بالغ الخطورة" },
    };

    public static List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> StatusList => new()
    {
        new() { Text = "قائم", Value = "قائم" },
        new() { Text = "قيد المعالجة", Value = "قيد المعالجة" },
        new() { Text = "مرفوع للقضاء", Value = "مرفوع للقضاء" },
        new() { Text = "أُزيل", Value = "أُزيل" },
    };
}
