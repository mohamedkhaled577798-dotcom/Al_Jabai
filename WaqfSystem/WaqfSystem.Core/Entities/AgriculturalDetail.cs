using System;
using WaqfSystem.Core.Enums;

namespace WaqfSystem.Core.Entities
{
    /// <summary>
    /// تفاصيل زراعية — Agricultural details for farm-type properties.
    /// </summary>
    public class AgriculturalDetail : BaseEntity
    {
        public int PropertyId { get; set; }

        // Land measurements
        public decimal? TotalAreaDunum { get; set; }
        public decimal? CultivatedAreaDunum { get; set; }
        public decimal? UncultivatedAreaDunum { get; set; }

        // Soil
        public SoilType? SoilType { get; set; }
        public byte? SoilFertilityRating { get; set; } // 1-5

        // Water & Irrigation
        public WaterSourceType? WaterSourceType { get; set; }
        public string? WaterRightsDocUrl { get; set; }
        public IrrigationMethod? IrrigationMethod { get; set; }
        public string? WaterAvailabilityMonths { get; set; } // JSON array

        // Harvest
        public string? PrimaryHarvestType { get; set; }
        public string? SecondaryHarvestType { get; set; }
        public SeasonType? SeasonType { get; set; }
        public decimal? AverageYieldTonPerDunum { get; set; }

        // Farmer & Contract
        public string? FarmerName { get; set; }
        public string? FarmerNationalId { get; set; }
        public FarmingContractType? FarmingContractType { get; set; }
        public decimal? WaqfShareOfHarvest { get; set; }
        public DateTime? FarmingStartDate { get; set; }
        public DateTime? FarmingEndDate { get; set; }

        // Facilities
        public bool HasFarmBuilding { get; set; } = false;
        public bool HasStorage { get; set; } = false;
        public bool HasWell { get; set; } = false;
        public bool HasRoadAccess { get; set; } = false;

        // Financial
        public decimal? AnnualRevenueEstimate { get; set; }
        public decimal? LandValuePerDunum { get; set; }

        // Inspection
        public DateTime? LastInspectionDate { get; set; }
        public short? LastHarvestYear { get; set; }

        // Navigation
        public virtual Property Property { get; set; } = null!;
    }
}
