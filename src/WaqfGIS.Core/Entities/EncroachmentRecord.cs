using NetTopologySuite.Geometries;

namespace WaqfGIS.Core.Entities;

/// <summary>
/// سجل التجاوزات والتعديات على الأملاك الوقفية
/// </summary>
public class EncroachmentRecord : BaseEntity
{
    public Guid Uuid { get; set; } = Guid.NewGuid();

    // ربط التجاوز بالعقار أو الأرض أو المسجد
    public string EntityType { get; set; } = string.Empty; // WaqfProperty, WaqfLand, Mosque
    public int EntityId { get; set; }
    public string? EntityName { get; set; }
    public int? ProvinceId { get; set; }

    // نوع التجاوز وتصنيفه
    public string EncroachmentType { get; set; } = string.Empty;
    // بناء غير مرخص، شق طريق، وضع يد، استغلال تجاري، سكني، زراعي، إشغال حكومي، أخرى

    public string Severity { get; set; } = "متوسط"; // بسيط، متوسط، خطير، بالغ الخطورة

    // وصف التجاوز
    public string Description { get; set; } = string.Empty;
    public decimal? EncroachmentAreaSqm { get; set; } // مساحة التجاوز بالمتر المربع

    // المتجاوز
    public string? EncroachwrName { get; set; }   // اسم المتجاوز
    public string? EncroachwrPhone { get; set; }   // هاتفه
    public string? EncroachwrNationalId { get; set; } // هويته

    // الموقع الجغرافي الدقيق للتجاوز (نقطة على الخريطة)
    public Point? Location { get; set; }
    public string? LocationDescription { get; set; } // وصف نصي للموقع

    // التواريخ
    public DateTime DiscoveryDate { get; set; } = DateTime.Today;
    public DateTime? ReportDate { get; set; }
    public DateTime? RemovalDate { get; set; }   // تاريخ إزالة التجاوز

    // الحالة
    public string Status { get; set; } = "قائم"; // قائم، قيد المعالجة، أُزيل، مرفوع للقضاء

    // الإجراءات المتخذة
    public string? ActionTaken { get; set; }      // وصف الإجراءات المتخذة
    public bool IsReportedToAuthorities { get; set; } = false;
    public string? ReportReferenceNumber { get; set; } // رقم الكتاب الرسمي
    public DateTime? AuthorityReportDate { get; set; }

    // ربط بنزاع قانوني (اختياري)
    public int? LegalDisputeId { get; set; }
    public bool HasLegalCase { get; set; } = false;

    // ملاحظات
    public string? Notes { get; set; }

    // Navigation Properties
    public virtual Province? Province { get; set; }
    public virtual LegalDispute? LegalDispute { get; set; }
    public virtual ICollection<EncroachmentPhoto> Photos { get; set; } = new List<EncroachmentPhoto>();
}

/// <summary>
/// صور التجاوزات
/// </summary>
public class EncroachmentPhoto : BaseEntity
{
    public int EncroachmentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public long FileSize { get; set; }
    public string? MimeType { get; set; }

    public virtual EncroachmentRecord Encroachment { get; set; } = null!;
}
