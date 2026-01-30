using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WaqfGIS.Web.Models;

public class RoadViewModel
{
    public int Id { get; set; }

    [Display(Name = "الكود")]
    public string? Code { get; set; }

    [Required(ErrorMessage = "الاسم بالعربية مطلوب")]
    [Display(Name = "الاسم بالعربية")]
    public string NameAr { get; set; } = string.Empty;

    [Display(Name = "الاسم بالإنجليزية")]
    public string? NameEn { get; set; }

    [Display(Name = "نوع الطريق")]
    public string RoadType { get; set; } = "Local";

    [Display(Name = "عرض الطريق (م)")]
    public double? WidthMeters { get; set; }

    [Display(Name = "عدد المسارات")]
    public int? LanesCount { get; set; }

    [Display(Name = "نوع السطح")]
    public string? SurfaceType { get; set; }

    [Display(Name = "المحافظة")]
    public int? ProvinceId { get; set; }

    [Display(Name = "القضاء")]
    public int? DistrictId { get; set; }

    [Display(Name = "ملاحظات")]
    public string? Notes { get; set; }

    // Geometry
    public string? GeometryGeoJson { get; set; }
    public double? LengthMeters { get; set; }

    // Navigation names
    public string? ProvinceName { get; set; }
    public string? DistrictName { get; set; }

    // Static Lists
    public static List<SelectListItem> RoadTypes => new()
    {
        new SelectListItem { Value = "Highway", Text = "طريق سريع" },
        new SelectListItem { Value = "Main", Text = "طريق رئيسي" },
        new SelectListItem { Value = "Secondary", Text = "طريق ثانوي" },
        new SelectListItem { Value = "Local", Text = "طريق محلي" },
        new SelectListItem { Value = "Alley", Text = "زقاق/درب" }
    };

    public static List<SelectListItem> SurfaceTypes => new()
    {
        new SelectListItem { Value = "Asphalt", Text = "أسفلت" },
        new SelectListItem { Value = "Concrete", Text = "كونكريت" },
        new SelectListItem { Value = "Gravel", Text = "حصى" },
        new SelectListItem { Value = "Unpaved", Text = "غير مبلط" }
    };
}

public class RoadSearchViewModel
{
    public string? SearchTerm { get; set; }
    public int? ProvinceId { get; set; }
    public string? RoadType { get; set; }
}
