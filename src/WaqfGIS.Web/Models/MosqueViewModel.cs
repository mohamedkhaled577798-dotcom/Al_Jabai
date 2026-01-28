using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace WaqfGIS.Web.Models;

public class MosqueViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "الاسم بالعربية مطلوب")]
    [Display(Name = "الاسم بالعربية")]
    [MaxLength(200)]
    public string NameAr { get; set; } = string.Empty;

    [Display(Name = "الاسم بالإنجليزية")]
    [MaxLength(200)]
    public string? NameEn { get; set; }

    [Required(ErrorMessage = "الدائرة الوقفية مطلوبة")]
    [Display(Name = "الدائرة الوقفية")]
    public int WaqfOfficeId { get; set; }

    [Required(ErrorMessage = "نوع المسجد مطلوب")]
    [Display(Name = "نوع المسجد")]
    public int MosqueTypeId { get; set; }

    [Display(Name = "الحالة")]
    public int? MosqueStatusId { get; set; }

    [Required(ErrorMessage = "المحافظة مطلوبة")]
    [Display(Name = "المحافظة")]
    public int ProvinceId { get; set; }

    [Display(Name = "القضاء")]
    public int? DistrictId { get; set; }

    [Required(ErrorMessage = "خط العرض مطلوب")]
    [Display(Name = "خط العرض")]
    [Range(-90, 90)]
    public double Latitude { get; set; }

    [Required(ErrorMessage = "خط الطول مطلوب")]
    [Display(Name = "خط الطول")]
    [Range(-180, 180)]
    public double Longitude { get; set; }

    [Display(Name = "العنوان")]
    [MaxLength(500)]
    public string? Address { get; set; }

    [Display(Name = "الحي/المنطقة")]
    [MaxLength(100)]
    public string? Neighborhood { get; set; }

    [Display(Name = "أقرب معلم")]
    [MaxLength(200)]
    public string? NearestLandmark { get; set; }

    [Display(Name = "السعة")]
    [Range(0, 100000)]
    public int? Capacity { get; set; }

    [Display(Name = "المساحة (م²)")]
    public decimal? AreaSqm { get; set; }

    [Display(Name = "صلاة الجمعة")]
    public bool HasFridayPrayer { get; set; }

    [Display(Name = "مئذنة")]
    public bool HasMinaret { get; set; } = true;

    [Display(Name = "قبة")]
    public bool HasDome { get; set; } = true;

    [Display(Name = "موقف سيارات")]
    public bool HasParking { get; set; }

    [Display(Name = "قسم نسائي")]
    public bool HasWomenSection { get; set; }

    [Display(Name = "اسم الإمام")]
    [MaxLength(100)]
    public string? ImamName { get; set; }

    [Display(Name = "هاتف الإمام")]
    [MaxLength(20)]
    public string? ImamPhone { get; set; }

    [Display(Name = "سنة التأسيس")]
    public int? EstablishedYear { get; set; }

    [Display(Name = "ملاحظات")]
    public string? Notes { get; set; }
    // ================== تعريف وتسجيل ==================
    [Display(Name = "كود المسجد")]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Display(Name = "رقم السند")]
    [MaxLength(100)]
    public string? DeedNumber { get; set; }

    [Display(Name = "رقم التسجيل")]
    [MaxLength(100)]
    public string? RegistrationNumber { get; set; }

    [Display(Name = "تاريخ التسجيل")]
    public DateTime? RegistrationDate { get; set; }

    // ================== مواصفات البناية ==================
    [Display(Name = "عدد الطوابق")]
    [Range(0, 50)]
    public int FloorsCount { get; set; } = 1;

    [Display(Name = "سنة آخر ترميم")]
    public int? LastRenovationYear { get; set; }

    [Display(Name = "مكتبة")]
    public bool HasLibrary { get; set; }

    [Display(Name = "مرافق وضوء")]
    public bool HasAblutionFacility { get; set; }

    // ================== بيانات إدارية ==================
    [Display(Name = "اسم المؤذن")]
    [MaxLength(100)]
    public string? MuezzinName { get; set; }

    // ================== تقسيم إداري ==================
    [Display(Name = "الناحية")]
    public int? SubDistrictId { get; set; }

    // ================== القوائم ==================
    public IEnumerable<SelectListItem> MosqueTypes { get; set; } = Enumerable.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> MosqueStatuses { get; set; } = Enumerable.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> WaqfOffices { get; set; } = Enumerable.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> Provinces { get; set; } = Enumerable.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> Districts { get; set; } = Enumerable.Empty<SelectListItem>();

}
