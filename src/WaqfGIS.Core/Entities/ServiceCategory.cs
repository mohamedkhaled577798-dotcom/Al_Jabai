namespace WaqfGIS.Core.Entities;

/// <summary>
/// تصنيفات وأنواع المرافق الخدمية القابلة للإدارة
/// </summary>
public class ServiceCategory : BaseEntity
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // الأيقونة الافتراضية
    public string? DefaultIconClass { get; set; } = "fas fa-building";
    public string? DefaultIconColor { get; set; } = "#3B82F6";
    
    // ترتيب العرض
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    
    // الأنواع التابعة لهذا التصنيف
    public virtual ICollection<ServiceType> ServiceTypes { get; set; } = new List<ServiceType>();
}

/// <summary>
/// أنواع المرافق ضمن كل تصنيف
/// </summary>
public class ServiceType : BaseEntity
{
    public int ServiceCategoryId { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // الأيقونة الخاصة بهذا النوع
    public int? MapIconId { get; set; }
    public string? CustomIconClass { get; set; }
    public string? CustomIconColor { get; set; }
    
    // ترتيب العرض
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    public virtual ServiceCategory ServiceCategory { get; set; } = null!;
    public virtual MapIcon? MapIcon { get; set; }
}
