using System.ComponentModel.DataAnnotations;

namespace WaqfGIS.Web.Models;

public class ContractViewModel
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

    [Required(ErrorMessage = "رقم العقد مطلوب")]
    [Display(Name = "رقم العقد")]
    public string ContractNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "تاريخ العقد مطلوب")]
    [Display(Name = "تاريخ العقد")]
    public DateTime ContractDate { get; set; }

    [Required(ErrorMessage = "نوع العقد مطلوب")]
    [Display(Name = "نوع العقد")]
    public string ContractType { get; set; } = string.Empty;

    [Display(Name = "الغرض من العقد")]
    public string? ContractPurpose { get; set; }

    [Required(ErrorMessage = "اسم المستثمر مطلوب")]
    [Display(Name = "اسم المستثمر")]
    public string InvestorName { get; set; } = string.Empty;

    [Display(Name = "نوع المستثمر")]
    public string? InvestorType { get; set; }

    [Display(Name = "رقم الهوية")]
    public string? InvestorIdNumber { get; set; }

    [Display(Name = "رقم الهاتف")]
    public string? InvestorPhone { get; set; }

    [Display(Name = "رقم الموبايل")]
    public string? InvestorMobile { get; set; }

    [Display(Name = "البريد الإلكتروني")]
    [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
    public string? InvestorEmail { get; set; }

    [Display(Name = "العنوان")]
    public string? InvestorAddress { get; set; }

    [Required(ErrorMessage = "تاريخ البداية مطلوب")]
    [Display(Name = "تاريخ البداية")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "تاريخ النهاية مطلوب")]
    [Display(Name = "تاريخ النهاية")]
    public DateTime EndDate { get; set; }

    [Display(Name = "قابل للتجديد")]
    public bool IsRenewable { get; set; }

    [Display(Name = "مدة الإشعار بالتجديد (أيام)")]
    public int? RenewalNoticeDays { get; set; }

    [Display(Name = "شروط التجديد")]
    public string? RenewalTerms { get; set; }

    [Required(ErrorMessage = "الإيجار الشهري مطلوب")]
    [Display(Name = "الإيجار الشهري")]
    [Range(0, double.MaxValue, ErrorMessage = "القيمة يجب أن تكون موجبة")]
    public decimal MonthlyRent { get; set; }

    [Display(Name = "العملة")]
    public string Currency { get; set; } = "IQD";

    [Required(ErrorMessage = "طريقة الدفع مطلوبة")]
    [Display(Name = "طريقة الدفع")]
    public string PaymentMethod { get; set; } = string.Empty;

    [Display(Name = "يوم الدفع من الشهر")]
    [Range(1, 31, ErrorMessage = "اليوم يجب أن يكون بين 1 و 31")]
    public int PaymentDayOfMonth { get; set; }

    [Display(Name = "اسم البنك")]
    public string? BankName { get; set; }

    [Display(Name = "رقم الحساب البنكي")]
    public string? BankAccountNumber { get; set; }

    [Display(Name = "التأمين")]
    public decimal? SecurityDeposit { get; set; }

    [Display(Name = "يوجد ضمان بنكي")]
    public bool HasBankGuarantee { get; set; }

    [Display(Name = "نوع الضمان")]
    public string? GuaranteeType { get; set; }

    [Display(Name = "رقم الضمان")]
    public string? GuaranteeNumber { get; set; }

    [Display(Name = "يوجد زيادة سنوية")]
    public bool HasAnnualIncrease { get; set; }

    [Display(Name = "نسبة الزيادة السنوية %")]
    public decimal? AnnualIncreasePercentage { get; set; }

    [Display(Name = "قيمة الزيادة السنوية")]
    public decimal? AnnualIncreaseAmount { get; set; }

    [Display(Name = "مسؤولية الصيانة")]
    public string? MaintenanceResponsibility { get; set; }

    [Display(Name = "مسؤولية المرافق")]
    public string? UtilitiesResponsibility { get; set; }

    [Display(Name = "الاستخدام المسموح")]
    public string? AllowedUsage { get; set; }

    [Display(Name = "الاستخدام الممنوع")]
    public string? ProhibitedUsage { get; set; }

    [Display(Name = "يسمح بالتأجير من الباطن")]
    public bool AllowSubleasing { get; set; }

    [Display(Name = "غرامة التأخير اليومية")]
    public decimal? LatePaymentPenaltyDaily { get; set; }

    [Display(Name = "نسبة غرامة التأخير %")]
    public decimal? LatePaymentPenaltyPercentage { get; set; }

    [Display(Name = "غرامة الإنهاء المبكر")]
    public decimal? EarlyTerminationPenalty { get; set; }

    [Display(Name = "غرامة خرق العقد")]
    public decimal? ContractBreachPenalty { get; set; }

    [Display(Name = "الحالة")]
    public string Status { get; set; } = "نشط";

    [Display(Name = "نشط")]
    public bool IsActive { get; set; }

    [Display(Name = "إشعار قبل 6 أشهر")]
    public bool NotifyBeforeSixMonths { get; set; }

    [Display(Name = "إشعار قبل 3 أشهر")]
    public bool NotifyBeforeThreeMonths { get; set; }

    [Display(Name = "إشعار قبل شهر")]
    public bool NotifyBeforeOneMonth { get; set; }

    [Display(Name = "المسؤول عن العقد")]
    public string? ResponsibleOfficer { get; set; }

    [Display(Name = "هاتف المسؤول")]
    public string? ResponsibleOfficerPhone { get; set; }

    [Display(Name = "ملاحظات")]
    public string? Notes { get; set; }

    [Display(Name = "شروط خاصة")]
    public string? SpecialTerms { get; set; }
}
