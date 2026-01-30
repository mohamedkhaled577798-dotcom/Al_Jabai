using NetTopologySuite.Geometries;

namespace WaqfGIS.Core.Entities;

/// <summary>
/// المسجد / الجامع
/// </summary>
public class Mosque : BaseEntity
{
    public Guid Uuid { get; set; } = Guid.NewGuid();
    public int WaqfOfficeId { get; set; }
    public int MosqueTypeId { get; set; }
    public int? MosqueStatusId { get; set; }
    public int ProvinceId { get; set; }
    public int? DistrictId { get; set; }
    public int? SubDistrictId { get; set; }

    // البيانات الأساسية
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? NameEn { get; set; }

    // الموقع الجغرافي
    public Point Location { get; set; } = null!;
    public string? Address { get; set; }
    public string? Neighborhood { get; set; }
    public string? NearestLandmark { get; set; }

    // المواصفات
    public int? Capacity { get; set; }
    public decimal? AreaSqm { get; set; }
    public int FloorsCount { get; set; } = 1;
    public bool HasFridayPrayer { get; set; } = false;
    public bool HasMinaret { get; set; } = true;
    public bool HasDome { get; set; } = true;
    public bool HasParking { get; set; } = false;
    public bool HasWomenSection { get; set; } = false;
    public bool HasLibrary { get; set; } = false;
    public bool HasAblutionFacility { get; set; } = true;
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    // بيانات إدارية
    public string? ImamName { get; set; }
    public string? ImamPhone { get; set; }
    public string? MuezzinName { get; set; }
    public string? MuezzinPhone { get; set; }
    public int? EstablishedYear { get; set; }
    public int? LastRenovationYear { get; set; }
    public bool HasAirConditioning { get; set; } = false;

    // الملكية والتسجيل
    public string? DeedNumber { get; set; }
    public string? RegistrationNumber { get; set; }
    public DateTime? RegistrationDate { get; set; }

    // ملاحظات
    public string? Notes { get; set; }
    public bool IsVerified { get; set; } = false;
    public DateTime? VerifiedAt { get; set; }
    public string? VerifiedBy { get; set; }


    // Navigation Properties
    public virtual WaqfOffice WaqfOffice { get; set; } = null!;
    public virtual MosqueType MosqueType { get; set; } = null!;
    public virtual MosqueStatus? MosqueStatus { get; set; }
    public virtual Province Province { get; set; } = null!;
    public virtual District? District { get; set; }
    public virtual SubDistrict? SubDistrict { get; set; }
    public virtual ICollection<MosqueDocument> Documents { get; set; } = new List<MosqueDocument>();
    public virtual ICollection<MosqueImage> Images { get; set; } = new List<MosqueImage>();
    public virtual ICollection<MosqueBoundary> Boundaries { get; set; } = new List<MosqueBoundary>();
}
