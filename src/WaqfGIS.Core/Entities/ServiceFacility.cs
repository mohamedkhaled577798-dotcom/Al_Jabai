using NetTopologySuite.Geometries;

namespace WaqfGIS.Core.Entities;

/// <summary>
/// الخدمات المحيطة بالأملاك الوقفية
/// </summary>
public class ServiceFacility : BaseEntity
{
    public Guid Uuid { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? NameEn { get; set; }

    // التصنيف
    public string ServiceCategory { get; set; } = string.Empty; 
    // المرافق: كهرباء، ماء، غاز، صرف صحي
    // الخدمات العامة: مستشفى، مدرسة، جامعة، مركز شرطة، إطفاء
    // الخدمات التجارية: سوق، مركز تجاري، بنك، صيدلية
    // المواصلات: محطة وقود، موقف حافلات، موقف سيارات
    // الترفيه: حديقة، ملعب، مركز شباب

    public string ServiceType { get; set; } = string.Empty;
    // أمثلة: محطة كهرباء، محطة ماء، محطة غاز، محطة بنزين، مستشفى عام، مستشفى خاص، مدرسة ابتدائية، إلخ

    // الموقع الجغرافي
    public Point Location { get; set; } = null!;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public Polygon? Boundary { get; set; }

    // العنوان
    public int? ProvinceId { get; set; }
    public int? DistrictId { get; set; }
    public int? SubDistrictId { get; set; }
    public string? Address { get; set; }
    public string? Neighborhood { get; set; }

    // معلومات المرفق
    public string? OwnerName { get; set; }
    public string? OwnerType { get; set; } // حكومي، خاص، مختلط
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public DateTime? EstablishedDate { get; set; }

    // الرمز على الخريطة
    public int? MapIconId { get; set; }
    public string? CustomIconClass { get; set; } // fas fa-hospital
    public string? CustomIconColor { get; set; } // #ff0000
    
    // المواصفات (حسب النوع)
    public decimal? Capacity { get; set; } // سعة المحطة، عدد الأسرة، عدد الطلاب، إلخ
    public string? CapacityUnit { get; set; }
    public decimal? AreaSqm { get; set; }
    public string? ServiceRadius { get; set; } // نطاق الخدمة (بالكيلومتر)

    // ساعات العمل
    public string? WorkingHours { get; set; }
    public bool Is24Hours { get; set; } = false;
    public string? WorkingDays { get; set; }

    // التقييم
    public decimal? QualityRating { get; set; } // من 1 إلى 5
    public string? ServiceQuality { get; set; } // ممتاز، جيد، متوسط، ضعيف
    public bool IsOperational { get; set; } = true;
    public string? Status { get; set; } // نشط، متوقف، قيد الصيانة، قيد الإنشاء

    // معلومات إضافية
    public string? Description { get; set; }
    public string? Facilities { get; set; } // المرافق المتوفرة
    public string? Notes { get; set; }

    // Navigation Properties
    public virtual Province? Province { get; set; }
    public virtual District? District { get; set; }
    public virtual SubDistrict? SubDistrict { get; set; }
    public virtual MapIcon? MapIcon { get; set; }
    public virtual ICollection<ServiceImage> Images { get; set; } = new List<ServiceImage>();
}

/// <summary>
/// صور المرافق الخدمية
/// </summary>
public class ServiceImage : BaseEntity
{
    public int ServiceFacilityId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? FileExtension { get; set; }
    public long FileSize { get; set; }
    public bool IsPrimary { get; set; } = false;
    public string? Caption { get; set; }
    public int DisplayOrder { get; set; }

    public virtual ServiceFacility ServiceFacility { get; set; } = null!;
}
