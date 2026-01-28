namespace WaqfGIS.Core.Entities;

/// <summary>
/// نوع المكتب الوقفي
/// </summary>
public class OfficeType : BaseEntity
{
    public string NameAr { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public int Level { get; set; } // 1: ديوان, 2: مديرية, 3: دائرة, 4: شعبة
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public virtual ICollection<WaqfOffice> WaqfOffices { get; set; } = new List<WaqfOffice>();
}
