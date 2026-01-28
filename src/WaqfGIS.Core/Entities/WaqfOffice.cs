using NetTopologySuite.Geometries;

namespace WaqfGIS.Core.Entities;

/// <summary>
/// الدائرة / المكتب الوقفي
/// </summary>
public class WaqfOffice : BaseEntity
{
    public Guid Uuid { get; set; } = Guid.NewGuid();
    public int? ParentOfficeId { get; set; }
    public int OfficeTypeId { get; set; }
    public int? ProvinceId { get; set; }
    public int? DistrictId { get; set; }

    // البيانات الأساسية
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? NameEn { get; set; }

    // الموقع الجغرافي
    public Point? Location { get; set; }
    public string? Address { get; set; }

    // بيانات التواصل
    public string? Phone { get; set; }
    public string? PhoneSecondary { get; set; }
    public string? Fax { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }

    // بيانات إدارية
    public string? ManagerName { get; set; }
    public string? ManagerPhone { get; set; }
    public DateTime? EstablishedDate { get; set; }

    // الحالة
    public string Status { get; set; } = "Active";
    public string? Notes { get; set; }

    // Navigation Properties
    public virtual WaqfOffice? ParentOffice { get; set; }
    public virtual ICollection<WaqfOffice> ChildOffices { get; set; } = new List<WaqfOffice>();
    public virtual OfficeType OfficeType { get; set; } = null!;
    public virtual Province? Province { get; set; }
    public virtual District? District { get; set; }
    public virtual ICollection<Mosque> Mosques { get; set; } = new List<Mosque>();
    public virtual ICollection<WaqfProperty> WaqfProperties { get; set; } = new List<WaqfProperty>();
    public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
}
