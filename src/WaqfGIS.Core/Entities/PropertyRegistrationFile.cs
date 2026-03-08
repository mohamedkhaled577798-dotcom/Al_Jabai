namespace WaqfGIS.Core.Entities;

/// <summary>
/// ملفات تسجيل العقار (سندات، عقود، وثائق رسمية)
/// </summary>
public class PropertyRegistrationFile : BaseEntity
{
    public int PropertyId { get; set; }

    /// <summary>نوع المستند: سند ملكية، قرار تخصيص، عقد إيجار، إلخ</summary>
    public string DocumentType { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long? FileSize { get; set; }
    public string? MimeType { get; set; }
    public string? Description { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public string? UploadedBy { get; set; }

    // Navigation
    public virtual WaqfProperty Property { get; set; } = null!;
}
