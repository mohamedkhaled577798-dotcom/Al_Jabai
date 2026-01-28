using NetTopologySuite.Geometries;

namespace WaqfGIS.Core.Entities;

/// <summary>
/// المحافظة
/// </summary>
public class Province : BaseEntity
{
    public string NameAr { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string Code { get; set; } = string.Empty;
    public Geometry? Boundary { get; set; }
    public Point? Centroid { get; set; }
    public decimal? AreaSqKm { get; set; }
    public int? Population { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public virtual ICollection<District> Districts { get; set; } = new List<District>();
    public virtual ICollection<WaqfOffice> WaqfOffices { get; set; } = new List<WaqfOffice>();
    public virtual ICollection<Mosque> Mosques { get; set; } = new List<Mosque>();
    public virtual ICollection<WaqfProperty> WaqfProperties { get; set; } = new List<WaqfProperty>();
}
