using NetTopologySuite.Geometries;

namespace WaqfGIS.Core.Entities;

/// <summary>
/// القضاء
/// </summary>
public class District : BaseEntity
{
    public int ProvinceId { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string Code { get; set; } = string.Empty;
    public Geometry? Boundary { get; set; }
    public Point? Centroid { get; set; }
    public decimal? AreaSqKm { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public virtual Province Province { get; set; } = null!;
    public virtual ICollection<SubDistrict> SubDistricts { get; set; } = new List<SubDistrict>();
    public virtual ICollection<Mosque> Mosques { get; set; } = new List<Mosque>();
    public virtual ICollection<WaqfProperty> WaqfProperties { get; set; } = new List<WaqfProperty>();
}
