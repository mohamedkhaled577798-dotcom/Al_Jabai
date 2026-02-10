using NetTopologySuite.Geometries;

namespace WaqfGIS.Core.Entities;

/// <summary>
/// النزاعات القانونية على الأملاك الوقفية
/// </summary>
public class LegalDispute : BaseEntity
{
    public Guid Uuid { get; set; } = Guid.NewGuid();

    // ربط النزاع بالعقار
    public string EntityType { get; set; } = string.Empty; // Mosque, WaqfProperty, WaqfLand
    public int EntityId { get; set; }
    public string? EntityName { get; set; }

    // معلومات الدعوى
    public string CaseNumber { get; set; } = string.Empty;
    public string CourtName { get; set; } = string.Empty;
    public string CourtType { get; set; } = string.Empty; // محكمة ابتدائية، استئناف، تمييز
    public DateTime CaseDate { get; set; }

    // أطراف الدعوى
    public string PlaintiffName { get; set; } = string.Empty; // المدعي
    public string? PlaintiffPhone { get; set; }
    public string? PlaintiffAddress { get; set; }
    public string DefendantName { get; set; } = string.Empty; // المدعى عليه
    public string? DefendantPhone { get; set; }
    public string? DefendantAddress { get; set; }

    // تفاصيل النزاع
    public string DisputeType { get; set; } = string.Empty; // ملكية، حدود، استثمار، إزالة تجاوز
    public string DisputeSubject { get; set; } = string.Empty; // موضوع النزاع
    public string? DisputeDescription { get; set; }

    // المطالبة
    public decimal? ClaimAmount { get; set; }
    public decimal? ClaimValue { get; set; }

    // حالة الدعوى
    public string CaseStatus { get; set; } = "جارية"; // جارية، متوقفة، منتهية
    public string CurrentStage { get; set; } = string.Empty; // ابتدائية، استئنافية، تمييزية، تنفيذية
    public DateTime? LastHearingDate { get; set; }
    public DateTime? NextHearingDate { get; set; }
    public string? LastProcedure { get; set; } // آخر إجراء

    // الحكم
    public bool HasVerdict { get; set; } = false;
    public DateTime? VerdictDate { get; set; }
    public string? VerdictSummary { get; set; }
    public string? VerdictResult { get; set; } // لصالح الوقف، ضد الوقف، جزئي

    // الاستئناف
    public bool IsAppealed { get; set; } = false;
    public DateTime? AppealDate { get; set; }
    public string? AppealStage { get; set; }

    // التنفيذ
    public bool IsExecuted { get; set; } = false;
    public DateTime? ExecutionDate { get; set; }
    public string? ExecutionDetails { get; set; }

    // المحامي
    public string? LawyerName { get; set; }
    public string? LawyerPhone { get; set; }
    public string? LawyerLicenseNumber { get; set; }

    // الأثر المالي
    public decimal? EstimatedLoss { get; set; }
    public decimal? ActualLoss { get; set; }
    public decimal? LegalCosts { get; set; }

    // الملاحظات
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }

    // المستندات
    public virtual ICollection<DisputeDocument> Documents { get; set; } = new List<DisputeDocument>();
}

/// <summary>
/// مستندات النزاع القانوني
/// </summary>
public class DisputeDocument : BaseEntity
{
    public int DisputeId { get; set; }
    public string DocumentType { get; set; } = string.Empty; // لائحة دعوى، محضر، حكم، مذكرة
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? FileExtension { get; set; }
    public long FileSize { get; set; }
    public DateTime DocumentDate { get; set; }
    public string? Description { get; set; }

    public virtual LegalDispute Dispute { get; set; } = null!;
}
