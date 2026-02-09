namespace WaqfGIS.Core.Entities;

/// <summary>
/// أسعار العقارات للمتر المربع حسب المنطقة
/// </summary>
public class PropertyPricing : BaseEntity
{
    public Guid Uuid { get; set; } = Guid.NewGuid();

    // المنطقة
    public int ProvinceId { get; set; }
    public int? DistrictId { get; set; }
    public int? SubDistrictId { get; set; }
    public string? Neighborhood { get; set; }

    // نوع العقار
    public int PropertyTypeId { get; set; }
    public string? PropertySubType { get; set; }

    // السعر
    public decimal PricePerSqm { get; set; }
    public string Currency { get; set; } = "IQD";
    public DateTime PriceDate { get; set; }
    public string PriceSource { get; set; } = string.Empty; // تقييم رسمي، سوق، عقد بيع، تقدير

    // النطاق السعري
    public decimal? MinPricePerSqm { get; set; }
    public decimal? MaxPricePerSqm { get; set; }
    public decimal? AvgPricePerSqm { get; set; }

    // عوامل التأثير
    public string? LocationQuality { get; set; } // ممتاز، جيد، متوسط، ضعيف
    public int? ProximityToMainRoad { get; set; } // بالمتر
    public bool NearPublicServices { get; set; } = false;
    public string? DemandLevel { get; set; } // عالي، متوسط، منخفض

    // معلومات إضافية
    public string? Notes { get; set; }
    public string? MarketTrend { get; set; } // صاعد، ثابت، نازل

    // التحقق
    public bool IsVerified { get; set; } = false;
    public DateTime? VerifiedAt { get; set; }
    public string? VerifiedBy { get; set; }

    // Navigation Properties
    public virtual Province Province { get; set; } = null!;
    public virtual District? District { get; set; }
    public virtual SubDistrict? SubDistrict { get; set; }
    public virtual PropertyType PropertyType { get; set; } = null!;
}

/// <summary>
/// مقارنات الأسعار بين العقارات
/// </summary>
public class PropertyComparison : BaseEntity
{
    public Guid Uuid { get; set; } = Guid.NewGuid();
    public string ComparisonName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ComparisonDate { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;

    // العقارات المقارنة
    public virtual ICollection<PropertyComparisonItem> Items { get; set; } = new List<PropertyComparisonItem>();
}

/// <summary>
/// عناصر المقارنة
/// </summary>
public class PropertyComparisonItem : BaseEntity
{
    public int ComparisonId { get; set; }
    
    // ربط بالعقار
    public string EntityType { get; set; } = string.Empty; // WaqfProperty, WaqfLand
    public int EntityId { get; set; }
    public string EntityName { get; set; } = string.Empty;

    // بيانات للمقارنة
    public decimal AreaSqm { get; set; }
    public decimal PricePerSqm { get; set; }
    public decimal TotalPrice { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? PropertyType { get; set; }
    
    // التقييم
    public int? LocationScore { get; set; } // من 1 إلى 10
    public int? ServicesScore { get; set; } // من 1 إلى 10
    public int? AccessibilityScore { get; set; } // من 1 إلى 10
    public int? OverallScore { get; set; } // من 1 إلى 10

    public string? Notes { get; set; }

    public virtual PropertyComparison Comparison { get; set; } = null!;
}
