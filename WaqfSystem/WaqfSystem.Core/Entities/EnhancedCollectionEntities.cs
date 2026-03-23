using System;
using System.Collections.Generic;
using WaqfSystem.Core.Enums;

namespace WaqfSystem.Core.Entities
{
    /// <summary>
    /// دفعة جماعية — Groups multiple property revenues together.
    /// </summary>
    public class CollectionBatch : BaseEntity
    {
        public string BatchCode { get; set; } = string.Empty; // BCH-2025-00001
        public string PeriodLabel { get; set; } = string.Empty;
        public int CollectedById { get; set; }
        public decimal TotalAmount { get; set; }
        public int ItemCount { get; set; }
        public DateTime CollectionDate { get; set; }
        public string? PaymentMethod { get; set; }
        public string? Notes { get; set; }

        // Navigation
        public virtual User CollectedBy { get; set; } = null!;
        public virtual ICollection<PropertyRevenue> Revenues { get; set; } = new List<PropertyRevenue>();
    }

    /// <summary>
    /// سجل الاقتراحات الذكية — Tracks whether suggestions were acted upon.
    /// </summary>
    public class CollectionSmartLog : BaseEntity
    {
        public int UserId { get; set; }
        public string SuggestionType { get; set; } = string.Empty; // Overdue, DueToday, DueSoon
        public int? UnitId { get; set; }
        public int? FloorId { get; set; }
        public int PropertyId { get; set; }
        public bool WasActedOn { get; set; } = false;

        // Navigation
        public virtual User User { get; set; } = null!;
        public virtual Property Property { get; set; } = null!;
        public virtual PropertyFloor? Floor { get; set; }
        public virtual PropertyUnit? Unit { get; set; }
    }

    /// <summary>
    /// Extension of PropertyRevenue for enhanced fields.
    /// </summary>
    public partial class PropertyRevenue
    {
        public int? BatchId { get; set; }
        public bool SuggestedBySystem { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public int? VarianceApprovedBy { get; set; }
        public string? VarianceApprovalNote { get; set; }

        // Navigation
        public virtual CollectionBatch? Batch { get; set; }
        public virtual User? VarianceApprover { get; set; }
    }
}
