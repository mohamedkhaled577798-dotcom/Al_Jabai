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
    public double Latitude { get; set; } = 33.3;

    [Required(ErrorMessage = "خط الطول مطلوب")]
    [Display(Name = "خط الطول")]
    [Range(-180, 180)]
    public double Longitude { get; set; } = 44.4;

    [Display(Name = "العنوان")]
    [MaxLength(500)]
    public string? Address { get; set; }

    [Display(Name = "الحي/المنطقة")]
    [MaxLength(100)]
    public string? Neighborhood { get; set; }

    [Display(Name = "أقرب معلم")]
    [MaxLength(200)]
    public string? NearestLandmark { get; set; }

    // ================== المواصفات ==================
    [Display(Name = "السعة (مصلي)")]
    [Range(0, 100000)]
    public int? Capacity { get; set; }

    [Display(Name = "المساحة (م²)")]
    public decimal? AreaSqm { get; set; }

    [Display(Name = "سنة التأسيس")]
    public int? EstablishedYear { get; set; }

    [Display(Name = "سنة آخر ترميم")]
    public int? LastRenovationYear { get; set; }

    // ================== المرافق ==================
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

    [Display(Name = "تكييف")]
    public bool HasAirConditioning { get; set; }

    [Display(Name = "مرافق وضوء")]
    public bool HasWuduArea { get; set; } = true;

    [Display(Name = "مكتبة")]
    public bool HasLibrary { get; set; }

    // ================== بيانات الإمام ==================
    [Display(Name = "اسم الإمام")]
    [MaxLength(100)]
    public string? ImamName { get; set; }

    [Display(Name = "هاتف الإمام")]
    [MaxLength(20)]
    public string? ImamPhone { get; set; }

    // ================== بيانات المؤذن ==================
    [Display(Name = "اسم المؤذن")]
    [MaxLength(100)]
    public string? MuezzinName { get; set; }

    [Display(Name = "هاتف المؤذن")]
    [MaxLength(20)]
    public string? MuezzinPhone { get; set; }

    // ================== ملاحظات ==================
    [Display(Name = "ملاحظات")]
    public string? Notes { get; set; }
}
