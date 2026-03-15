using System;
using System.Collections.Generic;
using WaqfSystem.Core.Enums;

namespace WaqfSystem.Core.Entities
{
    /// <summary>
    /// وحدة العقار — Property unit entity (apartment, shop, office, etc.).
    /// </summary>
    public class PropertyUnit : BaseEntity
    {
        public int FloorId { get; set; }
        public int PropertyId { get; set; }
        public string? UnitNumber { get; set; }
        public UnitType UnitType { get; set; } = UnitType.Apartment;
        public decimal? AreaSqm { get; set; }
        public short? BedroomCount { get; set; } = 0;
        public short? BathroomCount { get; set; } = 0;
        public OccupancyStatus OccupancyStatus { get; set; } = OccupancyStatus.Vacant;
        public int? CurrentContractId { get; set; }
        public decimal? MarketRentMonthly { get; set; }
        public string? ElectricMeterNo { get; set; }
        public string? WaterMeterNo { get; set; }
        public bool HasAC { get; set; } = false;
        public bool HasKitchen { get; set; } = false;
        public FurnishedStatus Furnished { get; set; } = FurnishedStatus.Unfurnished;
        public StructuralCondition? Condition { get; set; }
        public string? PhotoUrls { get; set; } // JSON array
        public string? UnitFloorPlanUrl { get; set; }
        public string? Notes { get; set; }

        // Navigation
        public virtual PropertyFloor Floor { get; set; } = null!;
        public virtual Property Property { get; set; } = null!;
        public virtual ICollection<PropertyRoom> Rooms { get; set; } = new List<PropertyRoom>();
        public virtual ICollection<PropertyMeter> Meters { get; set; } = new List<PropertyMeter>();
        public virtual ICollection<PropertyPhoto> Photos { get; set; } = new List<PropertyPhoto>();
    }
}
