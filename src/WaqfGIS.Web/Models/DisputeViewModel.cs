using System.ComponentModel.DataAnnotations;

namespace WaqfGIS.Web.Models;

public class DisputeViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "نوع الكيان مطلوب")]
    [Display(Name = "نوع الكيان")]
    public string EntityType { get; set; } = string.Empty;

    [Required(ErrorMessage = "معرف الكيان مطلوب")]
    [Display(Name = "معرف الكيان")]
    public int EntityId { get; set; }

    [Display(Name = "اسم الكيان")]
    public string? EntityName { get; set; }

    [Required(ErrorMessage = "رقم الدعوى مطلوب")]
    [Display(Name = "رقم الدعوى")]
    public string CaseNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "اسم المحكمة مطلوب")]
    [Display(Name = "اسم المحكمة")]
    public string CourtName { get; set; } = string.Empty;

    [Required(ErrorMessage = "نوع المحكمة مطلوب")]
    [Display(Name = "نوع المحكمة")]
    public string CourtType { get; set; } = string.Empty;

    [Required(ErrorMessage = "تاريخ الدعوى مطلوب")]
    [Display(Name = "تاريخ الدعوى")]
    public DateTime CaseDate { get; set; }

    [Required(ErrorMessage = "اسم المدعي مطلوب")]
    [Display(Name = "المدعي")]
    public string PlaintiffName { get; set; } = string.Empty;

    [Display(Name = "هاتف المدعي")]
    public string? PlaintiffPhone { get; set; }

    [Display(Name = "عنوان المدعي")]
    public string? PlaintiffAddress { get; set; }

    [Required(ErrorMessage = "اسم المدعى عليه مطلوب")]
    [Display(Name = "المدعى عليه")]
    public string DefendantName { get; set; } = string.Empty;

    [Display(Name = "هاتف المدعى عليه")]
    public string? DefendantPhone { get; set; }

    [Display(Name = "عنوان المدعى عليه")]
    public string? DefendantAddress { get; set; }

    [Required(ErrorMessage = "نوع النزاع مطلوب")]
    [Display(Name = "نوع النزاع")]
    public string DisputeType { get; set; } = string.Empty;

    [Required(ErrorMessage = "موضوع النزاع مطلوب")]
    [Display(Name = "موضوع النزاع")]
    public string DisputeSubject { get; set; } = string.Empty;

    [Display(Name = "وصف النزاع")]
    public string? DisputeDescription { get; set; }

    [Display(Name = "قيمة المطالبة")]
    public decimal? ClaimValue { get; set; }

    [Required(ErrorMessage = "حالة الدعوى مطلوبة")]
    [Display(Name = "حالة الدعوى")]
    public string CaseStatus { get; set; } = "جارية";

    [Required(ErrorMessage = "المرحلة الحالية مطلوبة")]
    [Display(Name = "المرحلة الحالية")]
    public string CurrentStage { get; set; } = string.Empty;

    [Display(Name = "تاريخ آخر جلسة")]
    public DateTime? LastHearingDate { get; set; }

    [Display(Name = "تاريخ الجلسة القادمة")]
    public DateTime? NextHearingDate { get; set; }

    [Display(Name = "آخر إجراء")]
    public string? LastProcedure { get; set; }

    [Display(Name = "يوجد حكم")]
    public bool HasVerdict { get; set; }

    [Display(Name = "تاريخ الحكم")]
    public DateTime? VerdictDate { get; set; }

    [Display(Name = "ملخص الحكم")]
    public string? VerdictSummary { get; set; }

    [Display(Name = "نتيجة الحكم")]
    public string? VerdictResult { get; set; }

    [Display(Name = "مستأنف")]
    public bool IsAppealed { get; set; }

    [Display(Name = "اسم المحامي")]
    public string? LawyerName { get; set; }

    [Display(Name = "هاتف المحامي")]
    public string? LawyerPhone { get; set; }

    [Display(Name = "رقم إجازة المحامي")]
    public string? LawyerLicenseNumber { get; set; }

    [Display(Name = "التكاليف القانونية")]
    public decimal? LegalCosts { get; set; }

    [Display(Name = "ملاحظات")]
    public string? Notes { get; set; }
}
