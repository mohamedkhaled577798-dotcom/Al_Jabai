using System.ComponentModel.DataAnnotations;

namespace WaqfGIS.Web.Models;

public class RoadViewModel
{
    public int Id { get; set; }
    
    public string? Code { get; set; }
    
    [Required(ErrorMessage = "الاسم بالعربية مطلوب")]
    [Display(Name = "الاسم بالعربية")]
    public string NameAr { get; set; } = string.Empty;
    
    [Display(Name = "الاسم بالإنجليزية")]
    public string? NameEn { get; set; }
    
    [Display(Name = "المحافظة")]
    public int? ProvinceId { get; set; }
    
    [Display(Name = "القضاء")]
    public int? DistrictId { get; set; }
    
    [Display(Name = "نوع الطريق")]
    public string? RoadType { get; set; }
    
    [Display(Name = "العرض (متر)")]
    public double? WidthMeters { get; set; }
    
    [Display(Name = "عدد المسارات")]
    public int? LanesCount { get; set; }
    
    [Display(Name = "نوع السطح")]
    public string? SurfaceType { get; set; }
    
    [Display(Name = "الطول (متر)")]
    public double? LengthMeters { get; set; }
    
    [Display(Name = "ملاحظات")]
    public string? Notes { get; set; }
    
    public string? GeometryGeoJson { get; set; }
}
