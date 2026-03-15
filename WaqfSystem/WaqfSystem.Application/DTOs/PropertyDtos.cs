using System;
using System.Collections.Generic;
using WaqfSystem.Core.Enums;

namespace WaqfSystem.Application.DTOs.Property
{
    public class PropertyListDto
    {
        public int Id { get; set; }
        public string WqfNumber { get; set; } = string.Empty;
        public string? PropertyName { get; set; }
        public PropertyType PropertyType { get; set; }
        public string PropertyTypeDisplay { get; set; } = string.Empty;
        public PropertyCategory PropertyCategory { get; set; }
        public OwnershipType OwnershipType { get; set; }
        public string OwnershipTypeDisplay { get; set; } = string.Empty;
        public decimal OwnershipPercentage { get; set; }
        public PropertyStatus PropertyStatus { get; set; }
        public ApprovalStage ApprovalStage { get; set; }
        public string ApprovalStageDisplay { get; set; } = string.Empty;
        public decimal DqsScore { get; set; }
        public string? GovernorateName { get; set; }
        public int? GovernorateId { get; set; }
        public decimal? TotalAreaSqm { get; set; }
        public decimal? EstimatedValue { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public GisSyncStatus GisSyncStatus { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PropertyDetailDto
    {
        public int Id { get; set; }
        public string WqfNumber { get; set; } = string.Empty;
        public string? PropertyName { get; set; }
        public string? PropertyNameEn { get; set; }
        public PropertyType PropertyType { get; set; }
        public PropertyCategory PropertyCategory { get; set; }
        public WaqfType? WaqfType { get; set; }
        public OwnershipType OwnershipType { get; set; }
        public decimal OwnershipPercentage { get; set; }
        public string? DeedNumber { get; set; }
        public string? CadastralNumber { get; set; }
        public string? TabuNumber { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public string? WaqfOriginStory { get; set; }
        public string? FounderName { get; set; }
        public DateTime? FoundationDate { get; set; }
        public string? EndowmentPurpose { get; set; }
        public short? TotalFloors { get; set; }
        public short? BasementFloors { get; set; }
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
        public decimal? EstimatedValue { get; set; }
        public DateTime? LastValuationDate { get; set; }
        public decimal? AnnualRevenue { get; set; }
        public decimal? AnnualExpenses { get; set; }
        public decimal? InsuranceValue { get; set; }
        public DateTime? InsuranceExpiryDate { get; set; }
        public PropertyStatus PropertyStatus { get; set; }
        public ApprovalStage ApprovalStage { get; set; }
        public decimal DqsScore { get; set; }
        public string? GisFeatureId { get; set; }
        public string? GisPolygon { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? GpsAccuracyMeters { get; set; }
        public GisSyncStatus GisSyncStatus { get; set; }
        public string? Notes { get; set; }
        public int? GovernorateId { get; set; }
        public string? GovernorateName { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Nested details
        public PropertyAddressDto? Address { get; set; }
        public List<FloorDto> Floors { get; set; } = new();
        public List<PropertyPartnershipDto> Partnerships { get; set; } = new();
        public AgriculturalDetailDto? AgriculturalDetail { get; set; }
        public List<DocumentDto> Documents { get; set; } = new();
        public List<PhotoDto> Photos { get; set; } = new();
        public List<WorkflowHistoryDto> WorkflowHistory { get; set; } = new();
    }

    public class PropertyAddressDto
    {
        public int? StreetId { get; set; }
        public string? StreetName { get; set; }
        public string? NeighborhoodName { get; set; }
        public string? DistrictName { get; set; }
        public string? GovernorateName { get; set; }
        public string? BuildingNumber { get; set; }
        public string? PlotNumber { get; set; }
        public string? BlockNumber { get; set; }
        public string? ZoneNumber { get; set; }
        public string? NearestLandmark { get; set; }
        public string? AlternativeAddress { get; set; }
        public string? What3Words { get; set; }
        public string? PlusCodes { get; set; }
    }

    public class FloorDto
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public short FloorNumber { get; set; }
        public string? FloorLabel { get; set; }
        public FloorUsage FloorUsage { get; set; }
        public decimal? TotalAreaSqm { get; set; }
        public decimal? UsableAreaSqm { get; set; }
        public short? CeilingHeightCm { get; set; }
        public StructuralCondition? StructuralCondition { get; set; }
        public bool HasBalcony { get; set; }
        public bool IsOccupied { get; set; }
        public string? Notes { get; set; }
        public List<UnitDto> Units { get; set; } = new();
    }

    public class UnitDto
    {
        public int Id { get; set; }
        public int FloorId { get; set; }
        public int PropertyId { get; set; }
        public string? UnitNumber { get; set; }
        public UnitType UnitType { get; set; }
        public decimal? AreaSqm { get; set; }
        public short? BedroomCount { get; set; }
        public short? BathroomCount { get; set; }
        public OccupancyStatus OccupancyStatus { get; set; }
        public decimal? MarketRentMonthly { get; set; }
        public string? ElectricMeterNo { get; set; }
        public string? WaterMeterNo { get; set; }
        public bool HasAC { get; set; }
        public bool HasKitchen { get; set; }
        public FurnishedStatus Furnished { get; set; }
        public StructuralCondition? Condition { get; set; }
        public string? Notes { get; set; }
        public List<RoomDto> Rooms { get; set; } = new();
    }

    public class RoomDto
    {
        public int Id { get; set; }
        public int UnitId { get; set; }
        public RoomType RoomType { get; set; }
        public decimal? AreaSqm { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public short? WindowsCount { get; set; }
        public StructuralCondition? Condition { get; set; }
        public string? Notes { get; set; }
    }

    public class PropertyPartnershipDto
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public string PartnerName { get; set; } = string.Empty;
        public PartnerType PartnerType { get; set; }
        public string? PartnerNationalId { get; set; }
        public decimal PartnerSharePercent { get; set; }
        public string? PartnerBankIBAN { get; set; }
        public RevenueDistribMethod RevenueDistribMethod { get; set; }
        public string? AgreementDocUrl { get; set; }
        public DateTime? AgreementDate { get; set; }
        public bool IsActive { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
        public string? Notes { get; set; }
    }

    public class AgriculturalDetailDto
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public decimal? TotalAreaDunum { get; set; }
        public decimal? CultivatedAreaDunum { get; set; }
        public decimal? UncultivatedAreaDunum { get; set; }
        public SoilType? SoilType { get; set; }
        public byte? SoilFertilityRating { get; set; }
        public WaterSourceType? WaterSourceType { get; set; }
        public IrrigationMethod? IrrigationMethod { get; set; }
        public string? PrimaryHarvestType { get; set; }
        public string? SecondaryHarvestType { get; set; }
        public SeasonType? SeasonType { get; set; }
        public decimal? AverageYieldTonPerDunum { get; set; }
        public string? FarmerName { get; set; }
        public string? FarmerNationalId { get; set; }
        public FarmingContractType? FarmingContractType { get; set; }
        public decimal? WaqfShareOfHarvest { get; set; }
        public DateTime? FarmingStartDate { get; set; }
        public DateTime? FarmingEndDate { get; set; }
        public bool HasFarmBuilding { get; set; }
        public bool HasStorage { get; set; }
        public bool HasWell { get; set; }
        public bool HasRoadAccess { get; set; }
        public decimal? AnnualRevenueEstimate { get; set; }
        public decimal? LandValuePerDunum { get; set; }
        public DateTime? LastInspectionDate { get; set; }
        public short? LastHarvestYear { get; set; }
    }

    public class DocumentDto
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public DocumentCategory DocumentCategory { get; set; }
        public string? DocumentType { get; set; }
        public string? DocumentNumber { get; set; }
        public DateTime? DocumentDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? IssuingAuthority { get; set; }
        public string FileUrl { get; set; } = string.Empty;
        public FileFormat FileFormat { get; set; }
        public int? FileSizeKB { get; set; }
        public bool IsOriginal { get; set; }
        public bool IsVerified { get; set; }
        public VerificationMethod? VerificationMethod { get; set; }
        public string? VerifiedByName { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public decimal? OcrConfidence { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PhotoDto
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public int? UnitId { get; set; }
        public PhotoType PhotoType { get; set; }
        public string FileUrl { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public int? FileSizeKB { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public DateTime? TakenAt { get; set; }
        public bool IsMain { get; set; }
        public string? Caption { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class WorkflowHistoryDto
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public ApprovalStage FromStage { get; set; }
        public ApprovalStage ToStage { get; set; }
        public string? ActionByName { get; set; }
        public DateTime ActionAt { get; set; }
        public string? Notes { get; set; }
        public decimal? DqsAtAction { get; set; }
    }

    public class CreatePropertyDto
    {
        public string? PropertyName { get; set; }
        public PropertyType PropertyType { get; set; }
        public PropertyCategory PropertyCategory { get; set; }
        public WaqfType? WaqfType { get; set; }
        public OwnershipType OwnershipType { get; set; }
        public decimal OwnershipPercentage { get; set; } = 100.00m;
        public string? DeedNumber { get; set; }
        public string? CadastralNumber { get; set; }
        public string? TabuNumber { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public string? WaqfOriginStory { get; set; }
        public string? FounderName { get; set; }
        public string? EndowmentPurpose { get; set; }
        public short? TotalFloors { get; set; }
        public decimal? TotalAreaSqm { get; set; }
        public decimal? LandAreaSqm { get; set; }
        public short? YearBuilt { get; set; }
        public ConstructionType? ConstructionType { get; set; }
        public StructuralCondition? StructuralCondition { get; set; }
        public decimal? EstimatedValue { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? GpsAccuracyMeters { get; set; }
        public string? GisPolygon { get; set; }
        public int? GovernorateId { get; set; }
        public string? Notes { get; set; }
        public string? LocalId { get; set; }
        public string? DeviceId { get; set; }

        // Address
        public int? StreetId { get; set; }
        public string? BuildingNumber { get; set; }
        public string? PlotNumber { get; set; }
        public string? BlockNumber { get; set; }
        public string? NearestLandmark { get; set; }
    }

    public class UpdatePropertyDto : CreatePropertyDto
    {
        public int Id { get; set; }
    }

    public class PropertyFilterRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int? GovernorateId { get; set; }
        public PropertyType? PropertyType { get; set; }
        public OwnershipType? OwnershipType { get; set; }
        public ApprovalStage? ApprovalStage { get; set; }
        public PropertyStatus? Status { get; set; }
        public string? SearchTerm { get; set; }
    }

    public class PropertyMapPointDto
    {
        public int Id { get; set; }
        public string WqfNumber { get; set; } = string.Empty;
        public string? PropertyName { get; set; }
        public PropertyType PropertyType { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public ApprovalStage ApprovalStage { get; set; }
        public decimal DqsScore { get; set; }
    }

    public class CreateFloorDto
    {
        public int PropertyId { get; set; }
        public short FloorNumber { get; set; }
        public string? FloorLabel { get; set; }
        public FloorUsage FloorUsage { get; set; }
        public decimal? TotalAreaSqm { get; set; }
        public decimal? UsableAreaSqm { get; set; }
        public short? CeilingHeightCm { get; set; }
        public StructuralCondition? StructuralCondition { get; set; }
        public bool HasBalcony { get; set; }
        public string? Notes { get; set; }
    }

    public class CreateUnitDto
    {
        public int FloorId { get; set; }
        public int PropertyId { get; set; }
        public string? UnitNumber { get; set; }
        public UnitType UnitType { get; set; }
        public decimal? AreaSqm { get; set; }
        public short? BedroomCount { get; set; }
        public short? BathroomCount { get; set; }
        public OccupancyStatus OccupancyStatus { get; set; }
        public decimal? MarketRentMonthly { get; set; }
        public string? ElectricMeterNo { get; set; }
        public string? WaterMeterNo { get; set; }
        public bool HasAC { get; set; }
        public bool HasKitchen { get; set; }
        public FurnishedStatus Furnished { get; set; }
        public StructuralCondition? Condition { get; set; }
        public string? Notes { get; set; }
    }

    public class CreateRoomDto
    {
        public int UnitId { get; set; }
        public RoomType RoomType { get; set; }
        public decimal? AreaSqm { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public short? WindowsCount { get; set; }
        public StructuralCondition? Condition { get; set; }
        public string? Notes { get; set; }
    }

    public class CreatePartnershipDto
    {
        public int PropertyId { get; set; }
        public string PartnerName { get; set; } = string.Empty;
        public PartnerType PartnerType { get; set; }
        public string? PartnerNationalId { get; set; }
        public decimal PartnerSharePercent { get; set; }
        public string? PartnerBankIBAN { get; set; }
        public RevenueDistribMethod RevenueDistribMethod { get; set; }
        public DateTime? AgreementDate { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
        public string? Notes { get; set; }
    }

    public class RevenueSplitDto
    {
        public int PropertyId { get; set; }
        public string PropertyName { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public decimal WaqfSharePercent { get; set; }
        public decimal WaqfShareAmount { get; set; }
        public List<PartnerShareDto> PartnerShares { get; set; } = new();
    }

    public class PartnerShareDto
    {
        public string PartnerName { get; set; } = string.Empty;
        public decimal SharePercent { get; set; }
        public decimal ShareAmount { get; set; }
    }

    public class CreateAgriculturalDto
    {
        public int PropertyId { get; set; }
        public decimal? TotalAreaDunum { get; set; }
        public decimal? CultivatedAreaDunum { get; set; }
        public decimal? UncultivatedAreaDunum { get; set; }
        public SoilType? SoilType { get; set; }
        public byte? SoilFertilityRating { get; set; }
        public WaterSourceType? WaterSourceType { get; set; }
        public IrrigationMethod? IrrigationMethod { get; set; }
        public string? PrimaryHarvestType { get; set; }
        public string? SecondaryHarvestType { get; set; }
        public SeasonType? SeasonType { get; set; }
        public decimal? AverageYieldTonPerDunum { get; set; }
        public string? FarmerName { get; set; }
        public string? FarmerNationalId { get; set; }
        public FarmingContractType? FarmingContractType { get; set; }
        public decimal? WaqfShareOfHarvest { get; set; }
        public DateTime? FarmingStartDate { get; set; }
        public DateTime? FarmingEndDate { get; set; }
        public bool HasFarmBuilding { get; set; }
        public bool HasStorage { get; set; }
        public bool HasWell { get; set; }
        public bool HasRoadAccess { get; set; }
        public decimal? AnnualRevenueEstimate { get; set; }
        public decimal? LandValuePerDunum { get; set; }
    }

    public class UploadDocumentDto
    {
        public int PropertyId { get; set; }
        public DocumentCategory DocumentCategory { get; set; }
        public string? DocumentType { get; set; }
        public string? DocumentNumber { get; set; }
        public DateTime? DocumentDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? IssuingAuthority { get; set; }
        public string? IssuingCity { get; set; }
        public FileFormat FileFormat { get; set; }
        public bool IsOriginal { get; set; }
        public string? Notes { get; set; }
    }

    public class WorkflowActionDto
    {
        public int PropertyId { get; set; }
        public ApprovalStage ToStage { get; set; }
        public string? Notes { get; set; }
    }

    public class GisSyncDto
    {
        public int PropertyId { get; set; }
        public string? GisFeatureId { get; set; }
        public string? GisPolygon { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? GpsAccuracy { get; set; }
        public GisSyncStatus SyncStatus { get; set; }
        public DateTime? LastSyncAt { get; set; }
    }

    public class DqsScoreDto
    {
        public int PropertyId { get; set; }
        public decimal TotalScore { get; set; }
        public List<DqsCriterionDto> Criteria { get; set; } = new();
        public bool CanSubmit { get; set; }
        public bool CanApprove { get; set; }
    }

    public class DqsCriterionDto
    {
        public string CriterionName { get; set; } = string.Empty;
        public string CriterionNameAr { get; set; } = string.Empty;
        public decimal Weight { get; set; }
        public bool Achieved { get; set; }
        public decimal Score { get; set; }
    }
}
