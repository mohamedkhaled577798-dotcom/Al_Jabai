namespace WaqfGIS.Core.Entities;

/// <summary>
/// صورة المسجد
/// </summary>
public class MosqueImage : BaseEntity
{
    public int MosqueId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string? ImageType { get; set; } // main, exterior, interior, minaret
    public string? Description { get; set; }
    public bool IsMain { get; set; } = false;
    public int SortOrder { get; set; } = 0;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public string? UploadedBy { get; set; }

    // Navigation Properties
    public virtual Mosque Mosque { get; set; } = null!;
}
