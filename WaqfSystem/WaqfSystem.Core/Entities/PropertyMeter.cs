using System;
using WaqfSystem.Core.Enums;

namespace WaqfSystem.Core.Entities
{
    /// <summary>
    /// عداد — Utility meter for a property unit.
    /// </summary>
    public class PropertyMeter : BaseEntity
    {
        public int UnitId { get; set; }
        public MeterType MeterType { get; set; } = MeterType.Electric;
        public string? MeterNumber { get; set; }
        public string? SubscriberNumber { get; set; }
        public string? IssuingAuthority { get; set; }

        // Navigation
        public virtual PropertyUnit Unit { get; set; } = null!;
    }
}
