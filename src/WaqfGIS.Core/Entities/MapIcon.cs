namespace WaqfGIS.Core.Entities;

/// <summary>
/// رموز الخرائط القابلة للتخصيص
/// </summary>
public class MapIcon : BaseEntity
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    
    // التصنيف
    public string Category { get; set; } = string.Empty; // مساجد، عقارات، خدمات، مرافق
    
    // رمز الأيقونة
    public string IconClass { get; set; } = string.Empty; // fas fa-mosque
    public string IconUrl { get; set; } = string.Empty; // URL للصورة
    public string IconColor { get; set; } = "#3388ff"; // اللون
    public string IconShape { get; set; } = "circle"; // circle, square, star, marker
    
    // الحجم
    public int IconSize { get; set; } = 32;
    
    // للاستخدام في
    public string UsedFor { get; set; } = string.Empty; // Mosque, WaqfProperty, ServiceFacility, etc.
    
    // هل افتراضي؟
    public bool IsDefault { get; set; } = false;
    public bool IsSystemIcon { get; set; } = false; // لا يمكن حذفه
    public bool IsActive { get; set; } = true;
    
    // SVG مخصص (اختياري)
    public string? CustomSvg { get; set; }
    
    public string? Description { get; set; }
}
