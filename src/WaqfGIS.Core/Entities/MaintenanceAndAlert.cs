namespace WaqfGIS.Core.Entities;

/// <summary>
/// سجل الصيانة للمساجد والعقارات والأراضي
/// </summary>
public class MaintenanceRecord : BaseEntity
{
    public string EntityType { get; set; } = string.Empty; // Mosque, WaqfProperty, WaqfLand
    public int    EntityId   { get; set; }
    public string? EntityName { get; set; }
    public int?   ProvinceId { get; set; }

    // نوع الصيانة
    public string MaintenanceType    { get; set; } = string.Empty; // كهرباء، سباكة، هيكل، تشطيب، ديكور، طوارئ، دورية، شاملة
    public string Priority           { get; set; } = "عادية";      // عاجلة، عالية، عادية، منخفضة
    public string Status             { get; set; } = "مجدولة";     // مجدولة، جارية، مكتملة، ملغاة، متأخرة

    // التوصيف
    public string  Title       { get; set; } = string.Empty;
    public string  Description { get; set; } = string.Empty;
    public string? Location    { get; set; } // المكان داخل العقار (الطابق الأول، السطح...)

    // التواريخ
    public DateTime  ScheduledDate   { get; set; }
    public DateTime? StartDate        { get; set; }
    public DateTime? CompletionDate   { get; set; }
    public DateTime? NextScheduledDate{ get; set; } // للصيانة الدورية

    // المقاول
    public string? ContractorName    { get; set; }
    public string? ContractorPhone   { get; set; }
    public string? ContractorCompany { get; set; }
    public string? ContractorLicense { get; set; }

    // التكلفة
    public decimal? EstimatedCost { get; set; }
    public decimal? ActualCost    { get; set; }
    public string?  InvoiceNumber { get; set; }

    // الدورية
    public bool   IsRecurring         { get; set; } = false;
    public string? RecurrenceInterval { get; set; } // شهري، ربع سنوي، نصف سنوي، سنوي

    // نتيجة الصيانة
    public string? WorkDone   { get; set; }
    public string? Notes      { get; set; }
    public bool    IsWarranty { get; set; } = false;
    public int?    WarrantyMonths { get; set; }

    // العلاقات
    public virtual Province? Province { get; set; }
    public virtual ICollection<MaintenancePhoto> Photos { get; set; } = new List<MaintenancePhoto>();
}

/// <summary>
/// صور سجل الصيانة
/// </summary>
public class MaintenancePhoto : BaseEntity
{
    public int    MaintenanceId { get; set; }
    public string FileName      { get; set; } = string.Empty;
    public string FilePath      { get; set; } = string.Empty;
    public long   FileSize      { get; set; }
    public string? MimeType     { get; set; }
    public string PhotoType     { get; set; } = "قبل"; // قبل، أثناء، بعد

    public virtual MaintenanceRecord Maintenance { get; set; } = null!;
}

/// <summary>
/// سجل التنبيهات والإشعارات
/// </summary>
public class AlertNotification : BaseEntity
{
    public string AlertType  { get; set; } = string.Empty;
    // ContractExpiry | ContractExpirySoon | LatePayment | LegalDeadline |
    // MaintenanceDue | EncroachmentUnresolved | Custom

    public string Severity   { get; set; } = "تنبيه"; // حرج، تحذير، تنبيه، معلومات
    public string Title      { get; set; } = string.Empty;
    public string Message    { get; set; } = string.Empty;

    // المرجع
    public string? EntityType { get; set; } // InvestmentContract, LegalDispute, MaintenanceRecord...
    public int?   EntityId   { get; set; }
    public string? EntityName { get; set; }
    public string? ActionUrl  { get; set; } // رابط لصفحة الإجراء

    // الحالة
    public bool      IsRead      { get; set; } = false;
    public bool      IsDismissed { get; set; } = false;
    public DateTime? ReadAt      { get; set; }
    public string?   ReadBy      { get; set; }

    // الاستحقاق
    public DateTime? DueDate   { get; set; } // تاريخ الاستحقاق الفعلي
    public int?      DaysLeft  { get; set; } // أيام متبقية (سالبة = متأخر)
    public int?      ProvinceId { get; set; }

    public virtual Province? Province { get; set; }
}
