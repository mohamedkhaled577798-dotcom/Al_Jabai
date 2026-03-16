using System;
using WaqfSystem.Core.Enums;

namespace WaqfSystem.Core.Entities
{
    public class PartnershipConditionRule
    {
        public int Id { get; set; }
        public int PartnershipId { get; set; }
        public ConditionRuleType RuleType { get; set; }
        public ConditionApplicationScope Scope { get; set; } = ConditionApplicationScope.Always;
        public string RuleName { get; set; } = string.Empty;
        public decimal? FixedAmount { get; set; }
        public decimal? PercentValue { get; set; }
        public decimal? MinRevenueThreshold { get; set; }
        public decimal? MaxRevenueThreshold { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? DistributionType { get; set; }
        public string? SeasonLabel { get; set; }
        public int PriorityOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? CreatedById { get; set; }

        public virtual PropertyPartnership Partnership { get; set; } = null!;
        public virtual User? CreatedBy { get; set; }
    }

    public class PartnershipExpenseEntry
    {
        public int Id { get; set; }
        public int PartnershipId { get; set; }
        public int PropertyId { get; set; }
        public string PeriodLabel { get; set; } = string.Empty;
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public string ExpenseType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? ReferenceNo { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? CreatedById { get; set; }

        public virtual PropertyPartnership Partnership { get; set; } = null!;
        public virtual Property Property { get; set; } = null!;
        public virtual User? CreatedBy { get; set; }
    }

    public class PartnerRevenueDistribution
    {
        public int Id { get; set; }
        public int PartnershipId { get; set; }
        public int PropertyId { get; set; }
        public string PeriodLabel { get; set; } = string.Empty;
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public string DistributionType { get; set; } = "Revenue";
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetRevenue { get; set; }
        public decimal WaqfAmount { get; set; }
        public decimal PartnerAmount { get; set; }
        public decimal WaqfPercentSnapshot { get; set; }
        public TransferStatus TransferStatus { get; set; } = TransferStatus.Pending;
        public DateTime? TransferDate { get; set; }
        public string? TransferMethod { get; set; }
        public string? TransferReference { get; set; }
        public string? TransferBankName { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? CreatedById { get; set; }

        public virtual PropertyPartnership Partnership { get; set; } = null!;
        public virtual Property Property { get; set; } = null!;
        public virtual User? CreatedBy { get; set; }
        public virtual PartnerRevenueDistribution? LinkedDistribution { get; set; }
    }

    public class PartnerContactLog
    {
        public int Id { get; set; }
        public int PartnershipId { get; set; }
        public ContactType ContactType { get; set; }
        public ContactDirection ContactDirection { get; set; } = ContactDirection.Outgoing;
        public string? Subject { get; set; }
        public string? MessageBody { get; set; }
        public string? RecipientAddress { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public int SentById { get; set; }
        public bool IsAutomatic { get; set; }
        public string? DeliveryStatus { get; set; }
        public string? ExternalMessageId { get; set; }
        public int? LinkedDistributionId { get; set; }
        public string? Notes { get; set; }

        public virtual PropertyPartnership Partnership { get; set; } = null!;
        public virtual User SentBy { get; set; } = null!;
        public virtual PartnerRevenueDistribution? LinkedDistribution { get; set; }
    }

    public class PartnerNotificationSchedule
    {
        public int Id { get; set; }
        public int PartnershipId { get; set; }
        public PartnershipNotificationTrigger TriggerType { get; set; }
        public DateTime TriggerDate { get; set; }
        public string Channels { get; set; } = "[]";
        public bool IsSent { get; set; }
        public DateTime? SentAt { get; set; }
        public string? TemplateKey { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual PropertyPartnership Partnership { get; set; } = null!;
    }
}
