using System;
using System.Collections.Generic;
using WaqfSystem.Core.Enums;

namespace WaqfSystem.Core.Entities
{
    /// <summary>
    /// عقد إيجار — Rent contract entity connecting a unit to a tenant.
    /// </summary>
    public class RentContract : BaseEntity
    {
        public int PropertyId { get; set; }
        public int? FloorId { get; set; }
        public int? UnitId { get; set; }
        
        public string ContractNumber { get; set; } = string.Empty;
        public string TenantNameAr { get; set; } = string.Empty;
        public string? TenantNameEn { get; set; }
        public string? TenantNationalId { get; set; }
        public string? TenantPhone { get; set; }
        
        public decimal RentAmount { get; set; }
        public decimal? InsuranceAmount { get; set; }
        public CollectionPeriodType PeriodType { get; set; } = CollectionPeriodType.Monthly;
        public string? ContractType { get; set; } // e.g. "Residential", "Commercial"
        
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? ActualTerminationDate { get; set; }
        public DateTime? NextDueDate { get; set; }
        
        public byte GracePeriodDays { get; set; } = 5;
        public decimal? PenaltyPerDay { get; set; }
        public bool AllowsPartialPayments { get; set; } = true;
        
        public ContractStatus Status { get; set; } = ContractStatus.Active;
        public string? ContractFileUrl { get; set; }
        public string? Notes { get; set; }

        // Navigation
        public virtual Property Property { get; set; } = null!;
        public virtual PropertyFloor? Floor { get; set; }
        public virtual PropertyUnit? Unit { get; set; }
        public virtual ICollection<PropertyRevenue> Revenues { get; set; } = new List<PropertyRevenue>();
        public virtual ICollection<RentPaymentSchedule> PaymentSchedules { get; set; } = new List<RentPaymentSchedule>();
    }

    /// <summary>
    /// إيراد — Revenue record for a building, floor, or unit.
    /// </summary>
    public partial class PropertyRevenue : BaseEntity
    {
        public int PropertyId { get; set; }
        public int? FloorId { get; set; }
        public int? UnitId { get; set; }
        public int? ContractId { get; set; }
        public int CollectedById { get; set; }
        
        public decimal Amount { get; set; }
        public decimal? ExpectedAmount { get; set; }
        public string PeriodLabel { get; set; } = string.Empty; // e.g. "2025-01"
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        
        public DateTime CollectionDate { get; set; }
        public string? PaymentMethod { get; set; }
        public string? ReceiptNumber { get; set; }
        public string? PayerNameAr { get; set; }
        
        public CollectionLevel CollectionLevel { get; set; } = CollectionLevel.Unit;
        public string? RevenueCode { get; set; } // RVN-2025-XXXXX
        public string? Notes { get; set; }
        
        // Navigation (Partial properties in EnhancedCollectionEntities.cs)
        public virtual Property Property { get; set; } = null!;
        public virtual PropertyFloor? Floor { get; set; }
        public virtual PropertyUnit? Unit { get; set; }
        public virtual RentContract? Contract { get; set; }
        public virtual User CollectedBy { get; set; } = null!;
    }

    /// <summary>
    /// قفل الفترة — Prevents double collection for the same period.
    /// </summary>
    public class RevenuePeriodLock : BaseEntity
    {
        public int PropertyId { get; set; }
        public int? FloorId { get; set; }
        public int? UnitId { get; set; }
        public string PeriodLabel { get; set; } = string.Empty;
        public CollectionLevel LockedLevel { get; set; }
        public string? ReasonAr { get; set; }
        public string? LockedByRevenueCode { get; set; }
        public DateTime LockedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation
        public virtual Property Property { get; set; } = null!;
    }

    /// <summary>
    /// جدول دفعات الإيجار — Auto-generated schedule for a contract.
    /// </summary>
    public class RentPaymentSchedule : BaseEntity
    {
        public int ContractId { get; set; }
        public DateTime DueDate { get; set; }
        public decimal ExpectedAmount { get; set; }
        public string PeriodLabel { get; set; } = string.Empty;
        public bool IsPaid { get; set; } = false;
        public decimal? AmountPaid { get; set; }
        public DateTime? LastPaymentDate { get; set; }
        
        // Navigation
        public virtual RentContract Contract { get; set; } = null!;
    }
}
