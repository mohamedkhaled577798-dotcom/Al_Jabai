namespace WaqfGIS.Core.Entities;

/// <summary>
/// حالة المسجد
/// </summary>
public class MosqueStatus : BaseEntity
{
    public string NameAr { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string? ColorCode { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public virtual ICollection<Mosque> Mosques { get; set; } = new List<Mosque>();
}
