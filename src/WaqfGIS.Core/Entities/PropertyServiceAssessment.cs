namespace WaqfGIS.Core.Entities;

/// <summary>
/// تقييم مدى توفر الخدمات للعقار
/// </summary>
public class PropertyServiceAssessment : BaseEntity
{
    public Guid Uuid { get; set; } = Guid.NewGuid();

    // ربط بالعقار
    public string EntityType { get; set; } = string.Empty; // Mosque, WaqfProperty, WaqfLand
    public int EntityId { get; set; }
    public string? EntityName { get; set; }

    // التقييم الإجمالي
    public string OverallRating { get; set; } = string.Empty; // ممتاز، جيد، متوسط، ضعيف
    public int OverallScore { get; set; } // من 0 إلى 100
    public DateTime AssessmentDate { get; set; } = DateTime.UtcNow;

    // المرافق (كهرباء، ماء، غاز، صرف صحي)
    public bool HasElectricity { get; set; } = false;
    public decimal? ElectricityDistance { get; set; } // بالمتر
    public int? ElectricityScore { get; set; } // من 0 إلى 10

    public bool HasWater { get; set; } = false;
    public decimal? WaterDistance { get; set; }
    public int? WaterScore { get; set; }

    public bool HasGas { get; set; } = false;
    public decimal? GasDistance { get; set; }
    public int? GasScore { get; set; }

    public bool HasSewage { get; set; } = false;
    public decimal? SewageDistance { get; set; }
    public int? SewageScore { get; set; }

    // الخدمات الصحية
    public int NearbyHospitalsCount { get; set; } = 0;
    public decimal? NearestHospitalDistance { get; set; }
    public int? HealthServicesScore { get; set; }

    // الخدمات التعليمية
    public int NearbySchoolsCount { get; set; } = 0;
    public decimal? NearestSchoolDistance { get; set; }
    public int? EducationServicesScore { get; set; }

    // خدمات الأمن والسلامة
    public int NearbyPoliceStationsCount { get; set; } = 0;
    public decimal? NearestPoliceStationDistance { get; set; }
    public int NearbyFireStationsCount { get; set; } = 0;
    public decimal? NearestFireStationDistance { get; set; }
    public int? SafetyServicesScore { get; set; }

    // المواصلات والوقود
    public int NearbyFuelStationsCount { get; set; } = 0;
    public decimal? NearestFuelStationDistance { get; set; }
    public int? TransportServicesScore { get; set; }

    // الخدمات التجارية
    public int NearbyMarketsCount { get; set; } = 0;
    public decimal? NearestMarketDistance { get; set; }
    public int NearbyBanksCount { get; set; } = 0;
    public decimal? NearestBankDistance { get; set; }
    public int? CommercialServicesScore { get; set; }

    // الطرق والوصول
    public bool OnMainRoad { get; set; } = false;
    public decimal? DistanceToMainRoad { get; set; }
    public string? RoadCondition { get; set; } // ممتاز، جيد، متوسط، ضعيف
    public int? AccessibilityScore { get; set; }

    // الملاحظات
    public string? Strengths { get; set; } // نقاط القوة
    public string? Weaknesses { get; set; } // نقاط الضعف
    public string? Recommendations { get; set; }
    public string? Notes { get; set; }

    // التحقق
    public bool IsVerified { get; set; } = false;
    public DateTime? VerifiedAt { get; set; }
    public string? VerifiedBy { get; set; }

    // العلاقات التفصيلية
    public virtual ICollection<ServiceProximity> NearbyServices { get; set; } = new List<ServiceProximity>();
}

/// <summary>
/// قرب الخدمات من العقار
/// </summary>
public class ServiceProximity : BaseEntity
{
    public int AssessmentId { get; set; }
    public int ServiceFacilityId { get; set; }
    
    public decimal Distance { get; set; } // بالمتر
    public int TravelTimeMinutes { get; set; }
    public string AccessMethod { get; set; } = "مشي"; // مشي، سيارة، باص
    
    public virtual PropertyServiceAssessment Assessment { get; set; } = null!;
    public virtual ServiceFacility ServiceFacility { get; set; } = null!;
}
