using NetTopologySuite.Geometries;

namespace WaqfGIS.Core.Entities;

/// <summary>
/// الناحية
/// </summary>
public class SubDistrict : BaseEntity
{
    public int DistrictId { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string? Code { get; set; }
    public Geometry? Boundary { get; set; }
    public Point? Centroid { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public virtual District District { get; set; } = null!;
    public virtual ICollection<Mosque> Mosques { get; set; } = new List<Mosque>();
    public virtual ICollection<WaqfProperty> WaqfProperties { get; set; } = new List<WaqfProperty>();
}
