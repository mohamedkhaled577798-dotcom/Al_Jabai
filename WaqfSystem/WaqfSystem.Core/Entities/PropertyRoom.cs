using System;
using WaqfSystem.Core.Enums;

namespace WaqfSystem.Core.Entities
{
    /// <summary>
    /// غرفة — Property room entity within a unit.
    /// </summary>
    public class PropertyRoom : BaseEntity
    {
        public int UnitId { get; set; }
        public RoomType RoomType { get; set; } = RoomType.Bedroom;
        public decimal? AreaSqm { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public short? WindowsCount { get; set; } = 0;
        public StructuralCondition? Condition { get; set; }
        public string? Notes { get; set; }

        // Navigation
        public virtual PropertyUnit Unit { get; set; } = null!;
    }
}
