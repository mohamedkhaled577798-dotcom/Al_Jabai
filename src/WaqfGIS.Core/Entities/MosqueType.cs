namespace WaqfGIS.Core.Entities;

/// <summary>
/// نوع المسجد
/// </summary>
public class MosqueType : BaseEntity
{
    public string NameAr { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public string? IconName { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public virtual ICollection<Mosque> Mosques { get; set; } = new List<Mosque>();
}
