using System;
using System.Collections.Generic;
using WaqfSystem.Core.Enums;

namespace WaqfSystem.Core.Entities
{
    /// <summary>
    /// شراكة العقار — Partnership details for shared-ownership properties.
    /// </summary>
    public class PropertyPartnership : BaseEntity
    {
        public int PropertyId { get; set; }
        public PartnershipType PartnershipType { get; set; } = PartnershipType.RevenuePercent;

        // Core share model
        public decimal WaqfSharePercent { get; set; }
        public decimal PartnerSharePercent { get; set; }

        // Type-specific fields
        public string? OwnedFloorNumbers { get; set; }
        public string? OwnedUnitIds { get; set; }
        public DateTime? UsufructStartDate { get; set; }
        public DateTime? UsufructEndDate { get; set; }
        public int? UsufructTermYears { get; set; }
        public decimal? UsufructAnnualFeePerYear { get; set; }
        public DateTime? PartnershipStartDate { get; set; }
        public DateTime? PartnershipEndDate { get; set; }
        public decimal? LandSharePercentWaqf { get; set; }
        public decimal? LandTotalDunum { get; set; }
        public decimal? WaqfLandDunum { get; set; }
        public decimal? WaqfHarvestPercent { get; set; }
        public string? FarmerName { get; set; }
        public string? FarmerNationalId { get; set; }
        public string? HarvestContractType { get; set; }
        public string? CustomPartnershipName { get; set; }
        public string? CustomCalculationFormula { get; set; }

        // Partner identity
        public string PartnerName { get; set; } = string.Empty;
        public string? PartnerNameEn { get; set; }
        public PartnerType PartnerType { get; set; } = PartnerType.Individual;
        public string? PartnerNationalId { get; set; }
        public string? PartnerRegistrationNo { get; set; }
        public string PartnerPhone { get; set; } = string.Empty;
        public string? PartnerPhone2 { get; set; }
        public string? PartnerEmail { get; set; }
        public string? PartnerWhatsApp { get; set; }
        public string? PartnerAddress { get; set; }
        public string? PartnerBankName { get; set; }
        public string? PartnerBankIBAN { get; set; }
        public string? PartnerBankAccountNo { get; set; }
        public string? PartnerBankBranch { get; set; }

        // Agreement
        public DateTime? AgreementDate { get; set; }
        public string? AgreementNotaryName { get; set; }
        public string? AgreementCourt { get; set; }
        public string? AgreementReferenceNo { get; set; }
        public string? AgreementDocUrl { get; set; }

        // Distribution schedule
        public RevenueDistribMethod RevenueDistribMethod { get; set; } = RevenueDistribMethod.Monthly;
        public ExpenseBearingMethod ExpenseBearingMethod { get; set; } = ExpenseBearingMethod.BeforeDistribution;
        public int? RevenueDistribDay { get; set; }
        public DateTime? LastDistribDate { get; set; }
        public DateTime? NextDistribDate { get; set; }

        // Lifecycle
        public bool IsActive { get; set; } = true;
        public string? DeactivationReason { get; set; }
        public DateTime? DeactivatedAt { get; set; }

        // Legacy compatibility
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
        public string? Notes { get; set; }

        // Navigation
        public virtual Property Property { get; set; } = null!;
        public override User? CreatedBy { get; set; }
        public virtual ICollection<PartnerRevenueDistribution> RevenueDistributions { get; set; } = new List<PartnerRevenueDistribution>();
        public virtual ICollection<PartnershipConditionRule> ConditionRules { get; set; } = new List<PartnershipConditionRule>();
        public virtual ICollection<PartnershipExpenseEntry> ExpenseEntries { get; set; } = new List<PartnershipExpenseEntry>();
        public virtual ICollection<PartnerContactLog> ContactLogs { get; set; } = new List<PartnerContactLog>();
        public virtual ICollection<PartnerNotificationSchedule> NotificationSchedules { get; set; } = new List<PartnerNotificationSchedule>();
    }
}
