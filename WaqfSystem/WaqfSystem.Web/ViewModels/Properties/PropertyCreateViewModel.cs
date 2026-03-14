using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WaqfSystem.Web.ViewModels.Properties
{
    // ─────────────────────────────────────────────────────────────────────────
    // OUTER CONTAINER
    // ─────────────────────────────────────────────────────────────────────────
    public class PropertyCreateViewModel
    {
        public int CurrentStep { get; set; } = 1;
        public int TotalSteps { get; set; } = 5;
        public string? DraftKey { get; set; }
        public int DqsScore { get; set; }
        public List<DqsCriterionViewModel> DqsCriteria { get; set; } = new();

        public BasicInfoViewModel BasicInfo { get; set; } = new();
        public GeographicViewModel Geographic { get; set; } = new();
        public MapLocationViewModel MapLocation { get; set; } = new();
        public BuildingDetailsViewModel BuildingDetails { get; set; } = new();
        public DocumentsViewModel Documents { get; set; } = new();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STEP 1 — البيانات الأساسية
    // ─────────────────────────────────────────────────────────────────────────
    public class BasicInfoViewModel
    {
        [Required(ErrorMessage = "اسم البناية مطلوب")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "يجب أن يكون الاسم بين 5 و200 حرف")]
        [Display(Name = "اسم البناية")]
        public string? NameAr { get; set; }

        [Required(ErrorMessage = "نوع الملكية مطلوب")]
        [Display(Name = "نوع الملكية")]
        public string? OwnershipType { get; set; }

        [Required(ErrorMessage = "تصنيف العقار مطلوب")]
        [Display(Name = "تصنيف العقار")]
        public int PropertyTypeId { get; set; }

        [Required(ErrorMessage = "نوع الوقف مطلوب")]
        [Display(Name = "نوع الوقف")]
        public string? WaqfType { get; set; }

        [Display(Name = "اسم الواقف")]
        public string? FounderName { get; set; }

        [Range(1800, 2025, ErrorMessage = "سنة التأسيس يجب أن تكون بين 1800 و2025")]
        [Display(Name = "سنة التأسيس")]
        public int? FoundingYear { get; set; }

        [Required(ErrorMessage = "الحالة الإنشائية مطلوبة")]
        [Display(Name = "الحالة الإنشائية")]
        public string? StructuralCondition { get; set; }

        [Required(ErrorMessage = "القيمة التقديرية مطلوبة")]
        [Range(1, double.MaxValue, ErrorMessage = "القيمة التقديرية يجب أن تكون أكبر من صفر")]
        [Display(Name = "القيمة التقديرية (دينار)")]
        public decimal? EstimatedValue { get; set; }

        [Required(ErrorMessage = "المساحة الكلية مطلوبة")]
        [Range(1, 999999, ErrorMessage = "المساحة يجب أن تكون بين 1 و999999 م²")]
        [Display(Name = "المساحة الكلية (م²)")]
        public decimal? TotalAreaSqm { get; set; }

        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }

        // ── Partnership fields ──────────────────────────────────────────────
        [Range(1, 99, ErrorMessage = "نسبة الوقف يجب أن تكون بين 1% و99%")]
        [Display(Name = "نسبة الوقف (%)")]
        public decimal? WaqfSharePercent { get; set; }

        [Display(Name = "اسم الشريك")]
        public string? PartnerName { get; set; }

        [Display(Name = "نوع الشريك")]
        public string? PartnerType { get; set; }

        // ── Agricultural fields ─────────────────────────────────────────────
        [Display(Name = "المساحة الكلية (دونم)")]
        public decimal? TotalAreaDunum { get; set; }

        [Display(Name = "المساحة المزروعة (دونم)")]
        public decimal? CultivatedAreaDunum { get; set; }

        [Display(Name = "نوع التربة")]
        public string? SoilType { get; set; }

        [Display(Name = "مصدر المياه")]
        public string? WaterSourceType { get; set; }

        [Display(Name = "طريقة الري")]
        public string? IrrigationMethod { get; set; }

        [Display(Name = "نوع المحصول الرئيسي")]
        public string? PrimaryHarvestType { get; set; }

        [Display(Name = "الموسم الزراعي")]
        public string? SeasonType { get; set; }

        [Display(Name = "نوع عقد الزراعة")]
        public string? FarmingContractType { get; set; }

        [Range(0, 100, ErrorMessage = "نسبة الوقف من الحصاد يجب بين 0 و100")]
        [Display(Name = "نسبة الوقف من الحصاد (%)")]
        public decimal? WaqfShareOfHarvest { get; set; }

        [Display(Name = "اسم المزارع")]
        public string? FarmerName { get; set; }

        [Display(Name = "رقم هوية المزارع")]
        public string? FarmerNationalId { get; set; }

        // ── SelectLists ─────────────────────────────────────────────────────
        public List<SelectListItem> PropertyTypeOptions { get; set; } = new();
        public List<SelectListItem> WaqfTypeOptions { get; set; } = new()
        {
            new SelectListItem("-- اختر نوع الوقف --", ""),
            new SelectListItem("وقف خيري", "Charitable"),
            new SelectListItem("وقف أهلي", "Family"),
            new SelectListItem("وقف مشترك", "Mixed")
        };
        public List<SelectListItem> StructuralConditionOptions { get; set; } = new()
        {
            new SelectListItem("-- اختر الحالة الإنشائية --", ""),
            new SelectListItem("ممتازة", "Excellent"),
            new SelectListItem("جيدة جداً", "VeryGood"),
            new SelectListItem("جيدة", "Good"),
            new SelectListItem("متوسطة", "Average"),
            new SelectListItem("ضعيفة", "Poor"),
            new SelectListItem("لا تصلح للسكن", "Uninhabitable")
        };
        public List<SelectListItem> PartnerTypeOptions { get; set; } = new()
        {
            new SelectListItem("-- اختر نوع الشريك --", ""),
            new SelectListItem("فرد", "Individual"),
            new SelectListItem("شركة", "Company"),
            new SelectListItem("ورثة", "Heirs"),
            new SelectListItem("جهة حكومية", "Government")
        };
        public List<SelectListItem> SoilTypeOptions { get; set; } = new()
        {
            new SelectListItem("-- اختر نوع التربة --", ""),
            new SelectListItem("طينية", "Clay"),
            new SelectListItem("رملية", "Sandy"),
            new SelectListItem("طينية رملية", "ClaySandy"),
            new SelectListItem("مالحة", "Saline"),
            new SelectListItem("سبخة", "Sabkha")
        };
        public List<SelectListItem> WaterSourceOptions { get; set; } = new()
        {
            new SelectListItem("-- اختر مصدر المياه --", ""),
            new SelectListItem("نهر", "River"),
            new SelectListItem("قناة", "Canal"),
            new SelectListItem("بئر ارتوازية", "ArtesianWell"),
            new SelectListItem("مطر", "Rain"),
            new SelectListItem("مياه مطرية محجوزة", "Rainwater")
        };
        public List<SelectListItem> IrrigationMethodOptions { get; set; } = new()
        {
            new SelectListItem("-- اختر طريقة الري --", ""),
            new SelectListItem("غمر", "Flood"),
            new SelectListItem("رش", "Sprinkler"),
            new SelectListItem("تنقيط", "Drip"),
            new SelectListItem("تقليدي", "Traditional")
        };
        public List<SelectListItem> SeasonTypeOptions { get; set; } = new()
        {
            new SelectListItem("-- اختر الموسم --", ""),
            new SelectListItem("شتوي", "Winter"),
            new SelectListItem("صيفي", "Summer"),
            new SelectListItem("موسمان", "TwoSeasons"),
            new SelectListItem("دائم", "Permanent")
        };
        public List<SelectListItem> FarmingContractOptions { get; set; } = new()
        {
            new SelectListItem("-- اختر نوع العقد --", ""),
            new SelectListItem("مزارعة", "Muzaraa"),
            new SelectListItem("مساقاة", "Musaqa"),
            new SelectListItem("إيجار نقدي", "CashRent"),
            new SelectListItem("إدارة ذاتية", "SelfFarmed")
        };
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STEP 2 — الموقع الجغرافي
    // ─────────────────────────────────────────────────────────────────────────
    public class GeographicViewModel
    {
        [Required(ErrorMessage = "المحافظة مطلوبة")]
        [Display(Name = "المحافظة")]
        public int GovernorateId { get; set; }

        [Required(ErrorMessage = "القضاء مطلوب")]
        [Display(Name = "القضاء")]
        public int DistrictId { get; set; }

        [Required(ErrorMessage = "الناحية مطلوبة")]
        [Display(Name = "الناحية")]
        public int SubDistrictId { get; set; }

        [Display(Name = "الحي / القرية")]
        public int? NeighborhoodId { get; set; }

        [Display(Name = "الشارع")]
        public int? StreetId { get; set; }

        [Display(Name = "رقم البناية")]
        public string? BuildingNumber { get; set; }

        [Display(Name = "رقم القطعة")]
        public string? PlotNumber { get; set; }

        [Display(Name = "رقم المحلة / المقاطعة")]
        public string? BlockNumber { get; set; }

        [Display(Name = "رقم المنطقة")]
        public string? ZoneNumber { get; set; }

        [StringLength(500, ErrorMessage = "لا يجوز أن يتجاوز 500 حرف")]
        [Display(Name = "أقرب معلم")]
        public string? NearestLandmark { get; set; }

        [Display(Name = "عنوان بديل")]
        public string? AlternativeAddress { get; set; }

        [Display(Name = "What3Words")]
        public string? What3Words { get; set; }

        // ── SelectLists ─────────────────────────────────────────────────────
        public List<SelectListItem> Governorates { get; set; } = new() { new SelectListItem("-- اختر المحافظة --", "0") };
        public List<SelectListItem> Districts { get; set; } = new() { new SelectListItem("-- اختر القضاء --", "0") };
        public List<SelectListItem> SubDistricts { get; set; } = new() { new SelectListItem("-- اختر الناحية --", "0") };
        public List<SelectListItem> Neighborhoods { get; set; } = new() { new SelectListItem("-- اختر الحي --", "") };
        public List<SelectListItem> Streets { get; set; } = new() { new SelectListItem("-- اختر الشارع --", "") };

        // ── Computed ────────────────────────────────────────────────────────
        public string AssembledAddress
        {
            get
            {
                var parts = new List<string>();
                if (!string.IsNullOrWhiteSpace(BuildingNumber)) parts.Add($"بناية {BuildingNumber}");
                if (!string.IsNullOrWhiteSpace(PlotNumber)) parts.Add($"قطعة {PlotNumber}");
                if (!string.IsNullOrWhiteSpace(BlockNumber)) parts.Add($"محلة {BlockNumber}");
                if (!string.IsNullOrWhiteSpace(NearestLandmark)) parts.Add($"بالقرب من {NearestLandmark}");
                return string.Join("، ", parts);
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STEP 3 — الخريطة والإحداثيات
    // ─────────────────────────────────────────────────────────────────────────
    public class MapLocationViewModel
    {
        [Range(-90, 90, ErrorMessage = "خط العرض يجب أن يكون بين -90 و90")]
        [Display(Name = "خط العرض (Latitude)")]
        public decimal? Latitude { get; set; }

        [Range(-180, 180, ErrorMessage = "خط الطول يجب أن يكون بين -180 و180")]
        [Display(Name = "خط الطول (Longitude)")]
        public decimal? Longitude { get; set; }

        [Display(Name = "دقة GPS (م)")]
        public decimal? GpsAccuracyMeters { get; set; }

        [Display(Name = "حدود المضلع (GeoJSON)")]
        public string? GisPolygon { get; set; }

        [Display(Name = "المساحة المحسوبة (م²)")]
        public decimal? CalculatedAreaSqm { get; set; }

        [Display(Name = "اسم الطبقة في GIS")]
        public string? GisLayerName { get; set; }

        public string MapMode { get; set; } = "Pin";
        public string GisBaseUrl { get; set; } = string.Empty;
        public string DefaultMapCenter { get; set; } = "[33.3152, 44.3661]";
        public int DefaultZoom { get; set; } = 13;

        public bool HasCoordinates => Latitude.HasValue && Longitude.HasValue;
        public bool HasPolygon => !string.IsNullOrWhiteSpace(GisPolygon);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STEP 4 — تفاصيل البناية
    // ─────────────────────────────────────────────────────────────────────────
    public class BuildingDetailsViewModel
    {
        [Required(ErrorMessage = "عدد الطوابق مطلوب")]
        [Range(1, 100, ErrorMessage = "عدد الطوابق بين 1 و100")]
        [Display(Name = "عدد الطوابق (فوق الأرض)")]
        public int FloorCount { get; set; } = 1;

        [Range(0, 10, ErrorMessage = "عدد البدرومات بين 0 و10")]
        [Display(Name = "عدد البدرومات")]
        public int BasementCount { get; set; } = 0;

        [Display(Name = "سنة البناء")]
        public int? ConstructionYear { get; set; }

        [Display(Name = "مساحة الطابق (م²)")]
        public decimal? FloorAreaSqm { get; set; }

        [Display(Name = "ارتفاع السقف (سم)")]
        public int? CeilingHeightCm { get; set; }

        [Display(Name = "الاستخدام الرئيسي")]
        public string? PrimaryUsage { get; set; }

        // ── Facilities ───────────────────────────────────────────────────────
        [Display(Name = "مصعد")]
        public bool HasElevator { get; set; }

        [Display(Name = "مواقف سيارات")]
        public bool HasParking { get; set; }

        [Display(Name = "مولد كهربائي")]
        public bool HasGenerator { get; set; }

        [Display(Name = "تكييف مركزي")]
        public bool HasCentralAC { get; set; }

        [Display(Name = "سطح / روف")]
        public bool HasRooftop { get; set; }

        [Display(Name = "خزان مياه")]
        public bool HasWaterTank { get; set; }

        [Display(Name = "كاميرات أمن")]
        public bool HasSecurityCameras { get; set; }

        [Display(Name = "مصلى")]
        public bool HasPrayerRoom { get; set; }

        // ── Floors ───────────────────────────────────────────────────────────
        public List<FloorInputViewModel> Floors { get; set; } = new();

        // ── SelectLists ──────────────────────────────────────────────────────
        public List<SelectListItem> UsageOptions { get; set; } = new()
        {
            new SelectListItem("-- اختر الاستخدام --", ""),
            new SelectListItem("سكني", "Residential"),
            new SelectListItem("تجاري", "Commercial"),
            new SelectListItem("مكتبي", "Office"),
            new SelectListItem("ديني", "Religious"),
            new SelectListItem("مختلط", "Mixed"),
            new SelectListItem("تخزين", "Storage"),
            new SelectListItem("مواقف سيارات", "Parking")
        };

        public List<SelectListItem> FloorUsageOptions { get; set; } = new()
        {
            new SelectListItem("-- اختر الاستخدام --", ""),
            new SelectListItem("سكني", "Residential"),
            new SelectListItem("تجاري", "Commercial"),
            new SelectListItem("مكتبي", "Office"),
            new SelectListItem("ديني", "Religious"),
            new SelectListItem("مختلط", "Mixed"),
            new SelectListItem("تخزين", "Storage"),
            new SelectListItem("مواقف سيارات", "Parking")
        };

        public List<SelectListItem> StructuralConditionOptions { get; set; } = new()
        {
            new SelectListItem("-- اختر الحالة --", ""),
            new SelectListItem("ممتازة", "Excellent"),
            new SelectListItem("جيدة جداً", "VeryGood"),
            new SelectListItem("جيدة", "Good"),
            new SelectListItem("متوسطة", "Average"),
            new SelectListItem("ضعيفة", "Poor"),
            new SelectListItem("لا تصلح للسكن", "Uninhabitable")
        };

        public List<SelectListItem> UnitTypeOptions { get; set; } = new()
        {
            new SelectListItem("-- نوع الوحدة --", ""),
            new SelectListItem("شقة", "Apartment"),
            new SelectListItem("محل تجاري", "Shop"),
            new SelectListItem("مكتب", "Office"),
            new SelectListItem("مستودع", "Warehouse"),
            new SelectListItem("استوديو", "Studio"),
            new SelectListItem("وحدة فندقية", "HotelUnit")
        };

        public List<SelectListItem> OccupancyOptions { get; set; } = new()
        {
            new SelectListItem("-- حالة الإشغال --", ""),
            new SelectListItem("شاغر", "Vacant"),
            new SelectListItem("مؤجر", "Rented"),
            new SelectListItem("مغلق", "Closed"),
            new SelectListItem("تحت الصيانة", "UnderMaintenance")
        };
    }

    // ─────────────────────────────────────────────────────────────────────────
    // FLOOR / UNIT
    // ─────────────────────────────────────────────────────────────────────────
    public class FloorInputViewModel
    {
        public int FloorNumber { get; set; }
        public string FloorLabel { get; set; } = string.Empty;
        public string FloorUsage { get; set; } = "Residential";
        public decimal? AreaSqm { get; set; }
        public string? StructuralCondition { get; set; }
        public int? CeilingHeightCm { get; set; }
        public bool HasBalcony { get; set; }
        public bool IsOccupied { get; set; }
        public string? Notes { get; set; }
        public List<UnitInputViewModel> Units { get; set; } = new();

        public string FloorDisplayName
        {
            get
            {
                if (FloorNumber == 0) return "الطابق الأرضي";
                if (FloorNumber < 0)
                {
                    var abs = Math.Abs(FloorNumber);
                    return abs == 1 ? "البدروم الأول" :
                           abs == 2 ? "البدروم الثاني" :
                           abs == 3 ? "البدروم الثالث" :
                           $"البدروم {abs}";
                }
                return $"الطابق {FloorNumber}";
            }
        }
    }

    public class UnitInputViewModel
    {
        public string UnitNumber { get; set; } = string.Empty;
        public string UnitType { get; set; } = "Apartment";
        public decimal? AreaSqm { get; set; }
        public int BedroomCount { get; set; }
        public string OccupancyStatus { get; set; } = "Vacant";
        public decimal? MarketRentMonthly { get; set; }
        public string? ElectricMeterNo { get; set; }
        public string? WaterMeterNo { get; set; }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // STEP 5 — المستندات
    // ─────────────────────────────────────────────────────────────────────────
    public class DocumentsViewModel
    {
        [Display(Name = "رقم الصك / الحجة")]
        public string? DeedNumber { get; set; }

        [Display(Name = "تاريخ الصك")]
        public DateTime? DeedDate { get; set; }

        [Display(Name = "محكمة الصك")]
        public string? DeedCourt { get; set; }

        [Display(Name = "ملف الصك")]
        public IFormFile? DeedFile { get; set; }
        public bool DeedUploaded { get; set; }

        [Display(Name = "رقم الكاداسترو")]
        public string? CadastralNumber { get; set; }

        [Display(Name = "رقم الطابو")]
        public string? TabuNumber { get; set; }

        [Display(Name = "ملف الكاداسترو / الطابو")]
        public IFormFile? CadastralFile { get; set; }

        [Display(Name = "رقم رخصة البناء")]
        public string? BuildingPermitNumber { get; set; }

        [Display(Name = "تاريخ رخصة البناء")]
        public DateTime? BuildingPermitDate { get; set; }

        [Display(Name = "ملف رخصة البناء")]
        public IFormFile? BuildingPermitFile { get; set; }

        [Display(Name = "رقم شهادة الإنجاز")]
        public string? CompletionCertNumber { get; set; }

        [Display(Name = "ملف شهادة الإنجاز")]
        public IFormFile? CompletionCertFile { get; set; }

        [Display(Name = "صورة الواجهة الأمامية")]
        public IFormFile? PhotoFront { get; set; }

        [Display(Name = "صورة الجانب الأيمن")]
        public IFormFile? PhotoRight { get; set; }

        [Display(Name = "صورة الجانب الأيسر")]
        public IFormFile? PhotoLeft { get; set; }

        [Display(Name = "صورة الجانب الخلفي")]
        public IFormFile? PhotoBack { get; set; }

        [Display(Name = "صورة داخلية")]
        public IFormFile? PhotoInside { get; set; }

        public int UploadedPhotoCount =>
            (PhotoFront != null ? 1 : 0) +
            (PhotoRight != null ? 1 : 0) +
            (PhotoLeft != null ? 1 : 0) +
            (PhotoBack != null ? 1 : 0) +
            (PhotoInside != null ? 1 : 0);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // DQS Criterion
    // ─────────────────────────────────────────────────────────────────────────
    public class DqsCriterionViewModel
    {
        public string Label { get; set; } = string.Empty;
        public int Weight { get; set; }
        public bool Achieved { get; set; }
        public int Score => Achieved ? Weight : 0;
    }
}
