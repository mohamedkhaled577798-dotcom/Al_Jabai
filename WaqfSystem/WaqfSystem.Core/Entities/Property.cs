using System;
using System.Collections.Generic;
using WaqfSystem.Core.Enums;

namespace WaqfSystem.Core.Entities
{
    /// <summary>
    /// العقار — Main property entity with 65+ fields.
    /// </summary>
    public class Property : BaseEntity
    {
        // Identity
        public string WqfNumber { get; set; } = string.Empty;
        public string? PropertyName { get; set; }
        public string? PropertyNameEn { get; set; }
        public PropertyType PropertyType { get; set; }
        public PropertyCategory PropertyCategory { get; set; } = PropertyCategory.Building;
        public WaqfType? WaqfType { get; set; }
        public OwnershipType OwnershipType { get; set; } = OwnershipType.FullWaqf;
        public decimal OwnershipPercentage { get; set; } = 100.00m;
        public string? DeedNumber { get; set; }
        public string? CadastralNumber { get; set; }
        public string? TabuNumber { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public string? WaqfOriginStory { get; set; }
        public string? FounderName { get; set; }
        public DateTime? FoundationDate { get; set; }
        public string? EndowmentPurpose { get; set; }

        // Building info
        public short? TotalFloors { get; set; }
        public short? BasementFloors { get; set; } = 0;
        public int? TotalUnits { get; set; }
        public decimal? TotalAreaSqm { get; set; }
        public decimal? BuiltUpAreaSqm { get; set; }
        public decimal? LandAreaSqm { get; set; }
        public short? YearBuilt { get; set; }
        public short? LastRenovationYear { get; set; }
        public ConstructionType? ConstructionType { get; set; }
        public StructuralCondition? StructuralCondition { get; set; }
        public string? FacadeType { get; set; }
        public string? RoofType { get; set; }

        // Financial
        public decimal? EstimatedValue { get; set; }
        public DateTime? LastValuationDate { get; set; }
        public decimal? AnnualRevenue { get; set; }
        public decimal? AnnualExpenses { get; set; }
        public decimal? InsuranceValue { get; set; }
        public DateTime? InsuranceExpiryDate { get; set; }

        // Status & Workflow
        public PropertyStatus PropertyStatus { get; set; } = PropertyStatus.Active;
        public ApprovalStage ApprovalStage { get; set; } = ApprovalStage.Draft;
        public decimal DqsScore { get; set; } = 0;

        // GIS
        public string? GisFeatureId { get; set; }
        public string? GisPolygonId { get; set; }
        public string? GisLayerName { get; set; }
        public string? GisPolygon { get; set; } // GeoJSON
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? GpsAccuracyMeters { get; set; }
        public string? SatelliteImageUrl { get; set; }
        public DateTime? LastGisSyncAt { get; set; }
        public GisSyncStatus GisSyncStatus { get; set; } = GisSyncStatus.Pending;

        // Audit
        public string? Notes { get; set; }
        public string? LocalId { get; set; }
        public string? DeviceId { get; set; }
        public int? UpdatedById { get; set; }
        public int? GovernorateId { get; set; }

        // Navigation properties
        public virtual User? UpdatedBy { get; set; }
        public virtual Governorate? Governorate { get; set; }
        public virtual PropertyAddress? Address { get; set; }
        public virtual AgriculturalDetail? AgriculturalDetail { get; set; }
        public virtual ICollection<PropertyFloor> Floors { get; set; } = new List<PropertyFloor>();
        public virtual ICollection<PropertyUnit> Units { get; set; } = new List<PropertyUnit>();
        public virtual ICollection<PropertyFacility> Facilities { get; set; } = new List<PropertyFacility>();
        public virtual ICollection<PropertyPartnership> Partnerships { get; set; } = new List<PropertyPartnership>();
        public virtual ICollection<PropertyDocument> Documents { get; set; } = new List<PropertyDocument>();
        public virtual ICollection<PropertyPhoto> Photos { get; set; } = new List<PropertyPhoto>();
        public virtual ICollection<PropertyWorkflowHistory> WorkflowHistory { get; set; } = new List<PropertyWorkflowHistory>();
        public virtual ICollection<GisSyncLog> GisSyncLogs { get; set; } = new List<GisSyncLog>();
    }
}
