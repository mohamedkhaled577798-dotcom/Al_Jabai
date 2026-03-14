using System;

namespace WaqfSystem.Core.Entities
{
    /// <summary>
    /// عنوان العقار — Property address with geographic hierarchy reference.
    /// </summary>
    public class PropertyAddress : BaseEntity
    {
        public int PropertyId { get; set; }
        public int? StreetId { get; set; }
        public string? BuildingNumber { get; set; }
        public string? PlotNumber { get; set; }
        public string? BlockNumber { get; set; }
        public string? ZoneNumber { get; set; }
        public string? NearestLandmark { get; set; }
        public string? AlternativeAddress { get; set; }
        public string? What3Words { get; set; }
        public string? PlusCodes { get; set; }

        // Navigation
        public virtual Property Property { get; set; } = null!;
        public virtual Street? Street { get; set; }
    }
}
