namespace WaqfGIS.Core.Entities;

/// <summary>
/// صورة المسجد
/// </summary>
public class MosqueImage : BaseEntity
{
    public int MosqueId { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public string? ImageType { get; set; } // main, exterior, interior, minaret
    public string? Caption { get; set; }
    public bool IsPrimary { get; set; } = false;
    public int SortOrder { get; set; } = 0;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public string? UploadedBy { get; set; }

    // Navigation Properties
    public virtual Mosque Mosque { get; set; } = null!;
}
