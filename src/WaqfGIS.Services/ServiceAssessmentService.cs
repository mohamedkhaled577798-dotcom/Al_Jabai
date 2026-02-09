using NetTopologySuite.Geometries;
using WaqfGIS.Core.Entities;
using WaqfGIS.Core.Interfaces;
using WaqfGIS.Services.GIS;

namespace WaqfGIS.Services;

/// <summary>
/// خدمة تقييم المرافق الخدمية للعقارات
/// </summary>
public class ServiceAssessmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly GeometryService _geometryService;

    public ServiceAssessmentService(IUnitOfWork unitOfWork, GeometryService geometryService)
    {
        _unitOfWork = unitOfWork;
        _geometryService = geometryService;
    }

    /// <summary>
    /// تقييم شامل لتوفر الخدمات حول عقار
    /// </summary>
    public async Task<PropertyServiceAssessment> AssessPropertyServicesAsync(string entityType, int entityId)
    {
        Point? location = await GetEntityLocationAsync(entityType, entityId);
        if (location == null)
        {
            throw new ArgumentException("الموقع غير موجود");
        }

        var assessment = new PropertyServiceAssessment
        {
            EntityType = entityType,
            EntityId = entityId,
            AssessmentDate = DateTime.UtcNow
        };

        var allFacilities = await _unitOfWork.Repository<ServiceFacility>().GetAllAsync();

        // تقييم المرافق (كهرباء، ماء، غاز)
        await AssessUtilitiesAsync(assessment, location, allFacilities);

        // تقييم الخدمات الصحية
        await AssessHealthServicesAsync(assessment, location, allFacilities);

        // تقييم الخدمات التعليمية
        await AssessEducationServicesAsync(assessment, location, allFacilities);

        // تقييم الأمن والسلامة
        await AssessSafetyServicesAsync(assessment, location, allFacilities);

        // تقييم المواصلات والوقود
        await AssessTransportServicesAsync(assessment, location, allFacilities);

        // تقييم الخدمات التجارية
        await AssessCommercialServicesAsync(assessment, location, allFacilities);

        // حساب التقييم الإجمالي
        CalculateOverallRating(assessment);

        return assessment;
    }

    private async Task<Point?> GetEntityLocationAsync(string entityType, int entityId)
    {
        return entityType switch
        {
            "WaqfProperty" => (await _unitOfWork.Repository<WaqfProperty>().GetByIdAsync(entityId))?.Location,
            "Mosque" => (await _unitOfWork.Repository<Mosque>().GetByIdAsync(entityId))?.Location,
            "WaqfLand" => (await _unitOfWork.Repository<WaqfLand>().GetByIdAsync(entityId))?.CenterPoint,
            _ => null
        };
    }

    private async Task AssessUtilitiesAsync(PropertyServiceAssessment assessment, Point location, IEnumerable<ServiceFacility> facilities)
    {
        // كهرباء
        var electricityStations = facilities.Where(f => f.ServiceCategory == "المرافق" && f.ServiceType.Contains("كهرباء"));
        if (electricityStations.Any())
        {
            var nearest = FindNearestFacility(location, electricityStations);
            assessment.HasElectricity = nearest.Distance <= 5000; // 5 كم
            assessment.ElectricityDistance = (decimal)nearest.Distance;
            assessment.ElectricityScore = CalculateScore(nearest.Distance, 1000, 5000);
        }

        // ماء
        var waterStations = facilities.Where(f => f.ServiceCategory == "المرافق" && f.ServiceType.Contains("ماء"));
        if (waterStations.Any())
        {
            var nearest = FindNearestFacility(location, waterStations);
            assessment.HasWater = nearest.Distance <= 3000;
            assessment.WaterDistance = (decimal)nearest.Distance;
            assessment.WaterScore = CalculateScore(nearest.Distance, 500, 3000);
        }

        // غاز
        var gasStations = facilities.Where(f => f.ServiceCategory == "المرافق" && f.ServiceType.Contains("غاز"));
        if (gasStations.Any())
        {
            var nearest = FindNearestFacility(location, gasStations);
            assessment.HasGas = nearest.Distance <= 5000;
            assessment.GasDistance = (decimal)nearest.Distance;
            assessment.GasScore = CalculateScore(nearest.Distance, 1000, 5000);
        }
    }

    private async Task AssessHealthServicesAsync(PropertyServiceAssessment assessment, Point location, IEnumerable<ServiceFacility> facilities)
    {
        var hospitals = facilities.Where(f => f.ServiceCategory == "الخدمات الصحية").ToList();
        var nearby = hospitals.Select(h => new
        {
            Facility = h,
            Distance = _geometryService.CalculateDistance(location, h.Location)
        }).Where(x => x.Distance <= 10000).OrderBy(x => x.Distance).ToList();

        assessment.NearbyHospitalsCount = nearby.Count;
        if (nearby.Any())
        {
            assessment.NearestHospitalDistance = (decimal)nearby.First().Distance;
            assessment.HealthServicesScore = CalculateScore(nearby.First().Distance, 1000, 10000);
        }
    }

    private async Task AssessEducationServicesAsync(PropertyServiceAssessment assessment, Point location, IEnumerable<ServiceFacility> facilities)
    {
        var schools = facilities.Where(f => f.ServiceCategory == "الخدمات التعليمية").ToList();
        var nearby = schools.Select(s => new
        {
            Facility = s,
            Distance = _geometryService.CalculateDistance(location, s.Location)
        }).Where(x => x.Distance <= 5000).OrderBy(x => x.Distance).ToList();

        assessment.NearbySchoolsCount = nearby.Count;
        if (nearby.Any())
        {
            assessment.NearestSchoolDistance = (decimal)nearby.First().Distance;
            assessment.EducationServicesScore = CalculateScore(nearby.First().Distance, 500, 5000);
        }
    }

    private async Task AssessSafetyServicesAsync(PropertyServiceAssessment assessment, Point location, IEnumerable<ServiceFacility> facilities)
    {
        var police = facilities.Where(f => f.ServiceType.Contains("شرطة")).ToList();
        var fire = facilities.Where(f => f.ServiceType.Contains("إطفاء")).ToList();

        var nearbyPolice = police.Select(p => new
        {
            Facility = p,
            Distance = _geometryService.CalculateDistance(location, p.Location)
        }).Where(x => x.Distance <= 10000).OrderBy(x => x.Distance).ToList();

        var nearbyFire = fire.Select(f => new
        {
            Facility = f,
            Distance = _geometryService.CalculateDistance(location, f.Location)
        }).Where(x => x.Distance <= 10000).OrderBy(x => x.Distance).ToList();

        assessment.NearbyPoliceStationsCount = nearbyPolice.Count;
        if (nearbyPolice.Any())
        {
            assessment.NearestPoliceStationDistance = (decimal)nearbyPolice.First().Distance;
        }

        assessment.NearbyFireStationsCount = nearbyFire.Count;
        if (nearbyFire.Any())
        {
            assessment.NearestFireStationDistance = (decimal)nearbyFire.First().Distance;
        }

        var avgDistance = (nearbyPolice.Any() ? nearbyPolice.First().Distance : 10000) +
                          (nearbyFire.Any() ? nearbyFire.First().Distance : 10000);
        assessment.SafetyServicesScore = CalculateScore(avgDistance / 2, 2000, 10000);
    }

    private async Task AssessTransportServicesAsync(PropertyServiceAssessment assessment, Point location, IEnumerable<ServiceFacility> facilities)
    {
        var fuelStations = facilities.Where(f => f.ServiceType.Contains("وقود") || f.ServiceType.Contains("بنزين")).ToList();
        var nearby = fuelStations.Select(f => new
        {
            Facility = f,
            Distance = _geometryService.CalculateDistance(location, f.Location)
        }).Where(x => x.Distance <= 5000).OrderBy(x => x.Distance).ToList();

        assessment.NearbyFuelStationsCount = nearby.Count;
        if (nearby.Any())
        {
            assessment.NearestFuelStationDistance = (decimal)nearby.First().Distance;
            assessment.TransportServicesScore = CalculateScore(nearby.First().Distance, 1000, 5000);
        }
    }

    private async Task AssessCommercialServicesAsync(PropertyServiceAssessment assessment, Point location, IEnumerable<ServiceFacility> facilities)
    {
        var markets = facilities.Where(f => f.ServiceType.Contains("سوق") || f.ServiceType.Contains("مركز تجاري")).ToList();
        var banks = facilities.Where(f => f.ServiceType.Contains("بنك") || f.ServiceType.Contains("مصرف")).ToList();

        var nearbyMarkets = markets.Select(m => new
        {
            Facility = m,
            Distance = _geometryService.CalculateDistance(location, m.Location)
        }).Where(x => x.Distance <= 3000).OrderBy(x => x.Distance).ToList();

        var nearbyBanks = banks.Select(b => new
        {
            Facility = b,
            Distance = _geometryService.CalculateDistance(location, b.Location)
        }).Where(x => x.Distance <= 5000).OrderBy(x => x.Distance).ToList();

        assessment.NearbyMarketsCount = nearbyMarkets.Count;
        if (nearbyMarkets.Any())
        {
            assessment.NearestMarketDistance = (decimal)nearbyMarkets.First().Distance;
        }

        assessment.NearbyBanksCount = nearbyBanks.Count;
        if (nearbyBanks.Any())
        {
            assessment.NearestBankDistance = (decimal)nearbyBanks.First().Distance;
        }

        var avgDistance = (nearbyMarkets.Any() ? nearbyMarkets.First().Distance : 3000) +
                          (nearbyBanks.Any() ? nearbyBanks.First().Distance : 5000);
        assessment.CommercialServicesScore = CalculateScore(avgDistance / 2, 500, 4000);
    }

    private (ServiceFacility Facility, double Distance) FindNearestFacility(Point location, IEnumerable<ServiceFacility> facilities)
    {
        var nearest = facilities
            .Select(f => new { Facility = f, Distance = _geometryService.CalculateDistance(location, f.Location) })
            .OrderBy(x => x.Distance)
            .First();

        return (nearest.Facility, nearest.Distance);
    }

    private int CalculateScore(double distance, double excellent, double poor)
    {
        if (distance <= excellent) return 10;
        if (distance >= poor) return 1;
        
        // حساب النسبة بين excellent و poor
        var ratio = (poor - distance) / (poor - excellent);
        return (int)Math.Ceiling(ratio * 9) + 1; // من 1 إلى 10
    }

    private void CalculateOverallRating(PropertyServiceAssessment assessment)
    {
        var scores = new List<int?> 
        {
            assessment.ElectricityScore,
            assessment.WaterScore,
            assessment.GasScore,
            assessment.HealthServicesScore,
            assessment.EducationServicesScore,
            assessment.SafetyServicesScore,
            assessment.TransportServicesScore,
            assessment.CommercialServicesScore
        };

        var validScores = scores.Where(s => s.HasValue).Select(s => s!.Value).ToList();
        
        if (validScores.Any())
        {
            assessment.OverallScore = (int)Math.Round(validScores.Average() * 10); // من 0 إلى 100
            
            assessment.OverallRating = assessment.OverallScore switch
            {
                >= 80 => "ممتاز",
                >= 60 => "جيد",
                >= 40 => "متوسط",
                _ => "ضعيف"
            };
        }
        else
        {
            assessment.OverallScore = 0;
            assessment.OverallRating = "غير محدد";
        }
    }

    /// <summary>
    /// حفظ التقييم في قاعدة البيانات
    /// </summary>
    public async Task<PropertyServiceAssessment> SaveAssessmentAsync(PropertyServiceAssessment assessment)
    {
        await _unitOfWork.Repository<PropertyServiceAssessment>().AddAsync(assessment);
        await _unitOfWork.SaveChangesAsync();
        return assessment;
    }

    /// <summary>
    /// الحصول على آخر تقييم لعقار
    /// </summary>
    public async Task<PropertyServiceAssessment?> GetLatestAssessmentAsync(string entityType, int entityId)
    {
        var assessments = await _unitOfWork.Repository<PropertyServiceAssessment>()
            .FindAsync(a => a.EntityType == entityType && a.EntityId == entityId);
        
        return assessments.OrderByDescending(a => a.AssessmentDate).FirstOrDefault();
    }
}
