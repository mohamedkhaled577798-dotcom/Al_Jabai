using System;
using WaqfSystem.Core.Enums;

namespace WaqfSystem.Core.Entities
{
    /// <summary>
    /// مرفق العقار — Property facility (elevator, generator, parking, etc.).
    /// </summary>
    public class PropertyFacility : BaseEntity
    {
        public int PropertyId { get; set; }
        public FacilityType FacilityType { get; set; } = FacilityType.Parking;
        public string? Details { get; set; }
        public int? Capacity { get; set; }
        public DateTime? LastMaintenanceDate { get; set; }
        public bool IsOperational { get; set; } = true;

        // Navigation
        public virtual Property Property { get; set; } = null!;
    }
}
