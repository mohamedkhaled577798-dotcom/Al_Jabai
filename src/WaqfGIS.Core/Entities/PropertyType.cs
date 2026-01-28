namespace WaqfGIS.Core.Entities;

/// <summary>
/// نوع العقار
/// </summary>
public class PropertyType : BaseEntity
{
    public string NameAr { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string? IconName { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public virtual ICollection<WaqfProperty> WaqfProperties { get; set; } = new List<WaqfProperty>();
}
