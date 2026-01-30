using NetTopologySuite.Geometries;

namespace WaqfGIS.Core.Entities;

/// <summary>
/// طبقة GIS - تعريف الطبقات المختلفة
/// </summary>
public class GisLayer : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string LayerType { get; set; } = string.Empty; // Point, Polygon, LineString
    public string? Description { get; set; }
    
    // Styling
    public string? FillColor { get; set; } = "#3388ff";
    public double FillOpacity { get; set; } = 0.3;
    public string? StrokeColor { get; set; } = "#3388ff";
    public double StrokeWidth { get; set; } = 2;
    public string? IconUrl { get; set; }
    
    // Visibility & Order
    public bool IsVisible { get; set; } = true;
    public bool IsEditable { get; set; } = true;
    public int DisplayOrder { get; set; }
    public int? MinZoom { get; set; }
    public int? MaxZoom { get; set; }
    
    // Source
    public string? SourceTable { get; set; }
    public string? GeometryColumn { get; set; } = "Boundary";
}

/// <summary>
/// حدود المسجد (مضلع)
/// </summary>
public class MosqueBoundary : BaseEntity
{
    public int MosqueId { get; set; }
    
    /// <summary>
    /// حدود المسجد كمضلع
    /// </summary>
    public Polygon? Boundary { get; set; }
    
    /// <summary>
    /// المساحة المحسوبة بالمتر المربع
    /// </summary>
    public double? CalculatedAreaSqm { get; set; }
    
    /// <summary>
    /// محيط المبنى بالمتر
    /// </summary>
    public double? PerimeterMeters { get; set; }
    
    /// <summary>
    /// نوع الحدود (مبنى، أرض، سور)
    /// </summary>
    public string BoundaryType { get; set; } = "Building"; // Building, Land, Fence
    
    public string? Notes { get; set; }
    
    // Navigation
    public virtual Mosque Mosque { get; set; } = null!;
}

/// <summary>
/// أراضي الوقف
/// </summary>
public class WaqfLand : BaseEntity
{
    public Guid Uuid { get; set; } = Guid.NewGuid();
    public int? WaqfOfficeId { get; set; }
    public int ProvinceId { get; set; }
    public int? DistrictId { get; set; }
    
    // Basic Info
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    
    // Geometry
    public Point? CenterPoint { get; set; }
    public Geometry? Boundary { get; set; } // Polygon or MultiPolygon
    
    // Calculated from Geometry
    public double? CalculatedAreaSqm { get; set; }
    public double? PerimeterMeters { get; set; }
    
    // Manual/Legal Area
    public decimal? LegalAreaSqm { get; set; }
    public decimal? AreaDonum { get; set; } // الدونم
    
    // Classification
    public string LandType { get; set; } = "Waqf"; // Waqf, Leased, Disputed, Reserved
    public string LandUse { get; set; } = "Vacant"; // Vacant, Agricultural, Commercial, Residential
    public string? ZoningCode { get; set; }
    
    // Legal
    public string? DeedNumber { get; set; }
    public string? RegistrationNumber { get; set; }
    public DateTime? RegistrationDate { get; set; }
    public string? OwnershipStatus { get; set; }
    
    // Financial
    public decimal? EstimatedValue { get; set; }
    public decimal? AnnualRevenue { get; set; }
    
    // Address
    public string? Address { get; set; }
    public string? Neighborhood { get; set; }
    
    public string? Notes { get; set; }
    
    // Navigation
    public virtual WaqfOffice? WaqfOffice { get; set; }
    public virtual Province Province { get; set; } = null!;
    public virtual District? District { get; set; }
}

/// <summary>
/// الطرق والشوارع
/// </summary>
public class Road : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    
    // Geometry
    public LineString? Geometry { get; set; }
    
    // Calculated
    public double? LengthMeters { get; set; }
    
    // Classification
    public string RoadType { get; set; } = "Local"; // Highway, Main, Secondary, Local, Alley
    public double? WidthMeters { get; set; }
    public int? LanesCount { get; set; }
    public string? SurfaceType { get; set; } // Asphalt, Concrete, Gravel, Unpaved
    
    // Location
    public int? ProvinceId { get; set; }
    public int? DistrictId { get; set; }
    
    public string? Notes { get; set; }
    
    // Navigation
    public virtual Province? Province { get; set; }
    public virtual District? District { get; set; }
}

/// <summary>
/// المشاريع المجاورة
/// </summary>
public class NearbyProject : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    
    // Geometry
    public Point? Location { get; set; }
    public Geometry? Boundary { get; set; }
    
    // Calculated
    public double? CalculatedAreaSqm { get; set; }
    
    // Classification
    public string ProjectType { get; set; } = string.Empty; // School, Hospital, Park, Commercial, Government
    public string? Status { get; set; } // Planned, UnderConstruction, Completed
    
    // Details
    public string? OwnerName { get; set; }
    public string? DeveloperName { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public decimal? ProjectValue { get; set; }
    
    // Location
    public int? ProvinceId { get; set; }
    public int? DistrictId { get; set; }
    public string? Address { get; set; }
    
    public string? Notes { get; set; }
    
    // Navigation
    public virtual Province? Province { get; set; }
    public virtual District? District { get; set; }
}

/// <summary>
/// سجل تغييرات الهندسة
/// </summary>
public class GeometryAuditLog : BaseEntity
{
    public string EntityType { get; set; } = string.Empty; // Mosque, WaqfLand, Road
    public int EntityId { get; set; }
    public string? EntityName { get; set; }
    
    public string Action { get; set; } = string.Empty; // Created, Modified, Deleted
    
    // Geometry snapshots (WKT format for readability)
    public string? OldGeometryWkt { get; set; }
    public string? NewGeometryWkt { get; set; }
    
    // Area changes
    public double? OldAreaSqm { get; set; }
    public double? NewAreaSqm { get; set; }
    
    // User info
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? IpAddress { get; set; }
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
}
