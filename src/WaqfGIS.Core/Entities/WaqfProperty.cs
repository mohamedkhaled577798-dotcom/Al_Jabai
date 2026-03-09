using NetTopologySuite.Geometries;

namespace WaqfGIS.Core.Entities;

/// <summary>
/// العقار الوقفي
/// </summary>
public class WaqfProperty : BaseEntity
{
    public Guid Uuid { get; set; } = Guid.NewGuid();
    public int WaqfOfficeId { get; set; }
    public int PropertyTypeId { get; set; }
    public int? UsageTypeId { get; set; }
    public int ProvinceId { get; set; }
    public int? DistrictId { get; set; }
    public int? SubDistrictId { get; set; }

    // البيانات الأساسية
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string? Description { get; set; }

    // الموقع الجغرافي
    public Point Location { get; set; } = null!;
    public string? Address { get; set; }
    public string? Neighborhood { get; set; }

    // المساحة والقيمة
    public decimal? AreaSqm { get; set; }
    public decimal? BuiltAreaSqm { get; set; }
    public decimal? TotalArea { get; set; } // المساحة المحسوبة من الحدود
    public int? FloorsCount { get; set; }
    public int? RoomsCount { get; set; }
    public decimal? EstimatedValue { get; set; }
    public decimal? PricePerSqm { get; set; } // سعر المتر المربع
    public string Currency { get; set; } = "IQD";

    // الملكية والتسجيل
    public string? OwnershipType { get; set; }
    public string? DeedNumber { get; set; }
    public DateTime? DeedDate { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? RealEstateUnitNumber { get; set; }
    public DateTime? AcquisitionDate { get; set; }
    public string? AcquisitionType { get; set; }

    // الإيجار والاستثمار
    public string? RentalStatus { get; set; }
    public decimal? MonthlyRent { get; set; }
    public decimal? AnnualRent { get; set; }
    public string? TenantName { get; set; }
    public string? TenantPhone { get; set; }
    public DateTime? LeaseStartDate { get; set; }
    public DateTime? LeaseEndDate { get; set; }

    // الحالة
    public string? ConditionStatus { get; set; }
    public string Status { get; set; } = "Active";
    public string? Notes { get; set; }

    // ============ حقول الأهلية والوقف ============
    /// <summary>أهلية الوقف: أهلي أم خيري</summary>
    public string WaqfNature { get; set; } = "Khairi";

    /// <summary>هل تمت استلامها إدارياً؟ (فقط للأهلي)</summary>
    public bool? IsAdminReceived { get; set; }

    /// <summary>شرط الوقف: بشرط أم بدون شرط</summary>
    public string WaqfCondition { get; set; } = "WithoutCondition";

    // ============ بيانات الواقف الأصلي ============
    /// <summary>اسم الواقف</summary>
    public string? WaqifName { get; set; }

    /// <summary>تاريخ وثيقة الوقف الأصلية</summary>
    public DateTime? WaqfDocumentDate { get; set; }

    /// <summary>رقم وثيقة الوقف</summary>
    public string? WaqfDocumentNumber { get; set; }

    /// <summary>نص شرط الواقف كاملاً</summary>
    public string? WaqfConditionText { get; set; }

    // ============ تجاوزات وتعديات ============
    /// <summary>ملاحظات التجاوزات على العقار</summary>
    public string? EncroachmentNotes { get; set; }

    /// <summary>هل يوجد تجاوز على العقار؟</summary>
    public bool HasEncroachment { get; set; } = false;

    // ============ بيانات الإشغال السكني ============
    /// <summary>هل العقار مشغول سكنياً؟</summary>
    public bool IsResidentialOccupancy { get; set; } = false;

    /// <summary>اسم الموظف الشاغل للسكن</summary>
    public string? OccupantEmployeeName { get; set; }

    /// <summary>رقم الهوية الوطنية للموظف</summary>
    public string? OccupantNationalId { get; set; }

    /// <summary>هاتف الموظف الشاغل</summary>
    public string? OccupantPhone { get; set; }

    /// <summary>الوزارة أو الجهة التي يتبع لها الموظف</summary>
    public string? OccupantMinistry { get; set; }

    /// <summary>تاريخ بدء الإشغال</summary>
    public DateTime? OccupantStartDate { get; set; }

    /// <summary>هل الموظف متقاعد؟</summary>
    public bool? IsOccupantRetired { get; set; }

    /// <summary>ملاحظات الإشغال السكني</summary>
    public string? OccupancyNotes { get; set; }

    // النزاعات القانونية
    public bool IsDisputed { get; set; } = false;
    public bool HasLegalCase { get; set; } = false;

    // الشوارع المحيطة (تم تحديدها تلقائياً من الخريطة)
    public string? NorthStreet { get; set; }
    public string? SouthStreet { get; set; }
    public string? EastStreet { get; set; }
    public string? WestStreet { get; set; }
    public string? MainAccessRoad { get; set; }


    // التحقق
    public bool IsVerified { get; set; } = false;
    public DateTime? VerifiedAt { get; set; }
    public string? VerifiedBy { get; set; }

    // Navigation Properties
    public virtual WaqfOffice WaqfOffice { get; set; } = null!;
    public virtual PropertyType PropertyType { get; set; } = null!;
    public virtual UsageType? UsageType { get; set; }
    public virtual Province Province { get; set; } = null!;
    public virtual District? District { get; set; }
    public virtual SubDistrict? SubDistrict { get; set; }
    public virtual ICollection<PropertyDocument> Documents { get; set; } = new List<PropertyDocument>();
    public virtual ICollection<PropertyRegistrationFile> RegistrationFiles { get; set; } = new List<PropertyRegistrationFile>();
    public virtual ICollection<PropertyImage> Images { get; set; } = new List<PropertyImage>();
    public virtual ICollection<InvestmentContract> Contracts { get; set; } = new List<InvestmentContract>();
    public virtual ICollection<LegalDispute> Disputes { get; set; } = new List<LegalDispute>();
}
