namespace WaqfGIS.Core.Entities;

/// <summary>
/// صور العقارات
/// </summary>
public class PropertyImage : BaseEntity
{
    public int WaqfPropertyId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsMain { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public string? UploadedBy { get; set; }

    // Navigation
    public virtual WaqfProperty WaqfProperty { get; set; } = null!;
}

/// <summary>
/// صور الدوائر الوقفية
/// </summary>
public class OfficeImage : BaseEntity
{
    public int WaqfOfficeId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsMain { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public string? UploadedBy { get; set; }

    // Navigation
    public virtual WaqfOffice WaqfOffice { get; set; } = null!;
}
