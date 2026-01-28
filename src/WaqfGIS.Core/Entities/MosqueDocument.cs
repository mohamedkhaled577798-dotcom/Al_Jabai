namespace WaqfGIS.Core.Entities;

/// <summary>
/// وثيقة المسجد
/// </summary>
public class MosqueDocument : BaseEntity
{
    public int MosqueId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long? FileSize { get; set; }
    public string? MimeType { get; set; }
    public string? Description { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public string? UploadedBy { get; set; }

    // Navigation Properties
    public virtual Mosque Mosque { get; set; } = null!;
}
