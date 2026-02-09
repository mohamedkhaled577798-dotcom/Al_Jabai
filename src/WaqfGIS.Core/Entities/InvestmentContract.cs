namespace WaqfGIS.Core.Entities;

/// <summary>
/// العقود الاستثمارية للأملاك الوقفية
/// </summary>
public class InvestmentContract : BaseEntity
{
    public Guid Uuid { get; set; } = Guid.NewGuid();

    // ربط العقد بالعقار
    public string EntityType { get; set; } = string.Empty; // Mosque, WaqfProperty, WaqfLand
    public int EntityId { get; set; }
    public string? EntityName { get; set; }

    // معلومات العقد
    public string ContractNumber { get; set; } = string.Empty;
    public DateTime ContractDate { get; set; }
    public string ContractType { get; set; } = string.Empty; // إيجار، استثمار، تطوير، شراكة
    public string? ContractPurpose { get; set; }

    // الطرف المستثمر
    public string InvestorName { get; set; } = string.Empty;
    public string? InvestorType { get; set; } // فرد، شركة، مؤسسة
    public string? InvestorIdNumber { get; set; }
    public string? InvestorPhone { get; set; }
    public string? InvestorMobile { get; set; }
    public string? InvestorEmail { get; set; }
    public string? InvestorAddress { get; set; }

    // مدة العقد
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int DurationMonths { get; set; }
    public int DurationYears { get; set; }

    // شروط التجديد
    public bool IsRenewable { get; set; } = true;
    public int? RenewalNoticeDays { get; set; } = 90;
    public string? RenewalTerms { get; set; }

    // القيمة المالية
    public decimal MonthlyRent { get; set; }
    public decimal AnnualRent { get; set; }
    public decimal TotalContractValue { get; set; }
    public string Currency { get; set; } = "IQD";

    // طريقة الدفع
    public string PaymentMethod { get; set; } = string.Empty; // شهري، ربع سنوي، نصف سنوي، سنوي
    public int PaymentDayOfMonth { get; set; } = 1;
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }

    // التأمينات
    public decimal? SecurityDeposit { get; set; }
    public string? GuaranteeType { get; set; } // شيك، كفالة بنكية، سند لأمر
    public string? GuaranteeNumber { get; set; }

    // الزيادة السنوية
    public bool HasAnnualIncrease { get; set; } = false;
    public decimal? AnnualIncreasePercentage { get; set; }
    public decimal? AnnualIncreaseAmount { get; set; }

    // الشروط والأحكام
    public string? MaintenanceResponsibility { get; set; } // المستثمر، الوقف، مشترك
    public string? UtilitiesResponsibility { get; set; }
    public string? AllowedUsage { get; set; }
    public string? ProhibitedUsage { get; set; }
    public bool AllowSubleasing { get; set; } = false;

    // الغرامات
    public decimal? LatePaymentPenaltyDaily { get; set; }
    public decimal? LatePaymentPenaltyPercentage { get; set; }
    public decimal? EarlyTerminationPenalty { get; set; }
    public decimal? ContractBreachPenalty { get; set; }

    // حالة العقد
    public string Status { get; set; } = "نشط"; // نشط، منتهي، ملغي، متوقف، قيد التجديد
    public bool IsActive { get; set; } = true;
    public DateTime? TerminationDate { get; set; }
    public string? TerminationReason { get; set; }

    // التنبيهات
    public bool NotifyBeforeSixMonths { get; set; } = true;
    public bool NotifyBeforeThreeMonths { get; set; } = true;
    public bool NotifyBeforeOneMonth { get; set; } = true;
    public DateTime? LastNotificationDate { get; set; }

    // الإيرادات المحققة
    public decimal TotalPaidAmount { get; set; } = 0;
    public decimal TotalOutstandingAmount { get; set; } = 0;
    public int PaymentsCount { get; set; } = 0;
    public DateTime? LastPaymentDate { get; set; }

    // مسؤول العقد
    public string? ResponsibleOfficer { get; set; }
    public string? ResponsibleOfficerPhone { get; set; }

    // الملاحظات
    public string? Notes { get; set; }
    public string? SpecialTerms { get; set; }

    // المستندات والصور
    public virtual ICollection<ContractDocument> Documents { get; set; } = new List<ContractDocument>();
    public virtual ICollection<ContractPayment> Payments { get; set; } = new List<ContractPayment>();
}

/// <summary>
/// مستندات العقد الاستثماري
/// </summary>
public class ContractDocument : BaseEntity
{
    public int ContractId { get; set; }
    public string DocumentType { get; set; } = string.Empty; // عقد، ملحق، كفالة، شيك، سند
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? FileExtension { get; set; }
    public long FileSize { get; set; }
    public DateTime DocumentDate { get; set; }
    public string? Description { get; set; }

    public virtual InvestmentContract Contract { get; set; } = null!;
}

/// <summary>
/// دفعات العقد الاستثماري
/// </summary>
public class ContractPayment : BaseEntity
{
    public int ContractId { get; set; }
    public int PaymentNumber { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public decimal AmountDue { get; set; }
    public decimal AmountPaid { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? ReceiptNumber { get; set; }
    public string Status { get; set; } = "معلق"; // معلق، مدفوع، متأخر، ملغي
    public int? DaysLate { get; set; }
    public decimal? LateFee { get; set; }
    public string? Notes { get; set; }

    public virtual InvestmentContract Contract { get; set; } = null!;
}
