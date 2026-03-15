using System;
using System.Collections.Generic;
using WaqfSystem.Core.Enums;

namespace WaqfSystem.Core.Entities
{
    /// <summary>
    /// طابق العقار — Property floor entity.
    /// </summary>
    public class PropertyFloor : BaseEntity
    {
        public int PropertyId { get; set; }
        public short FloorNumber { get; set; }
        public string? FloorLabel { get; set; }
        public FloorUsage FloorUsage { get; set; } = FloorUsage.Residential;
        public decimal? TotalAreaSqm { get; set; }
        public decimal? UsableAreaSqm { get; set; }
        public short? CeilingHeightCm { get; set; }
        public StructuralCondition? StructuralCondition { get; set; }
        public bool HasBalcony { get; set; } = false;
        public string? FloorPlanUrl { get; set; }
        public bool IsOccupied { get; set; } = false;
        public string? Notes { get; set; }

        // Navigation
        public virtual Property Property { get; set; } = null!;
        public virtual ICollection<PropertyUnit> Units { get; set; } = new List<PropertyUnit>();
    }
}
