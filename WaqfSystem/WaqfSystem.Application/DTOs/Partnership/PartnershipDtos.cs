using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using WaqfSystem.Application.DTOs.Common;
using WaqfSystem.Core.Enums;

namespace WaqfSystem.Application.DTOs.Partnership
{
    public class CreatePartnershipDto
    {
        [Required]
        public int PropertyId { get; set; }

        [Required]
        public PartnershipType PartnershipType { get; set; }

        [Required]
        [Range(1, 99)]
        public decimal WaqfSharePercent { get; set; }

        [Required]
        public string PartnerName { get; set; } = string.Empty;

        public string? PartnerNameEn { get; set; }

        [Required]
        public PartnerType PartnerType { get; set; }

        public string? PartnerNationalId { get; set; }
        public string? PartnerRegistrationNo { get; set; }

        [Required]
        public string PartnerPhone { get; set; } = string.Empty;

        public string? PartnerPhone2 { get; set; }
        public string? PartnerEmail { get; set; }
        public string? PartnerWhatsApp { get; set; }
        public string? PartnerAddress { get; set; }
        public string? PartnerBankName { get; set; }
        public string? PartnerBankIBAN { get; set; }
        public string? PartnerBankAccountNo { get; set; }
        public string? PartnerBankBranch { get; set; }

        public List<int>? OwnedFloorNumbers { get; set; }
        public List<long>? OwnedUnitIds { get; set; }
        public DateTime? UsufructStartDate { get; set; }
        public DateTime? UsufructEndDate { get; set; }
        public decimal? UsufructAnnualFeePerYear { get; set; }
        public DateTime? PartnershipStartDate { get; set; }
        public DateTime? PartnershipEndDate { get; set; }
        public decimal? LandSharePercentWaqf { get; set; }
        public decimal? LandTotalDunum { get; set; }
        public decimal? WaqfHarvestPercent { get; set; }
        public string? FarmerName { get; set; }
        public string? FarmerNationalId { get; set; }
        public string? HarvestContractType { get; set; }
        public string? CustomPartnershipName { get; set; }
        public string? CustomCalculationFormula { get; set; }

        public DateTime? AgreementDate { get; set; }
        public string? AgreementNotaryName { get; set; }
        public string? AgreementCourt { get; set; }
        public string? AgreementReferenceNo { get; set; }
        public IFormFile? AgreementFile { get; set; }

        [Required]
        public RevenueDistribMethod RevenueDistribMethod { get; set; }

        [Required]
        public ExpenseBearingMethod ExpenseBearingMethod { get; set; } = ExpenseBearingMethod.BeforeDistribution;

        [Range(1, 28)]
        public int? RevenueDistribDay { get; set; }

        public List<PartnershipConditionRuleDto> ConditionRules { get; set; } = new();

        public string? Notes { get; set; }
    }

    public class UpdatePartnershipDto
    {
        [Required]
        public int Id { get; set; }

        public int? PropertyId { get; set; }
        public PartnershipType? PartnershipType { get; set; }
        [Range(1, 99)]
        public decimal? WaqfSharePercent { get; set; }

        public string? PartnerName { get; set; }
        public string? PartnerNameEn { get; set; }
        public PartnerType? PartnerType { get; set; }
        public string? PartnerNationalId { get; set; }
        public string? PartnerRegistrationNo { get; set; }
        public string? PartnerPhone { get; set; }
        public string? PartnerPhone2 { get; set; }
        public string? PartnerEmail { get; set; }
        public string? PartnerWhatsApp { get; set; }
        public string? PartnerAddress { get; set; }
        public string? PartnerBankName { get; set; }
        public string? PartnerBankIBAN { get; set; }
        public string? PartnerBankAccountNo { get; set; }
        public string? PartnerBankBranch { get; set; }

        public List<int>? OwnedFloorNumbers { get; set; }
        public List<long>? OwnedUnitIds { get; set; }
        public DateTime? UsufructStartDate { get; set; }
        public DateTime? UsufructEndDate { get; set; }
        public decimal? UsufructAnnualFeePerYear { get; set; }
        public DateTime? PartnershipStartDate { get; set; }
        public DateTime? PartnershipEndDate { get; set; }
        public decimal? LandSharePercentWaqf { get; set; }
        public decimal? LandTotalDunum { get; set; }
        public decimal? WaqfHarvestPercent { get; set; }
        public string? FarmerName { get; set; }
        public string? FarmerNationalId { get; set; }
        public string? HarvestContractType { get; set; }
        public string? CustomPartnershipName { get; set; }
        public string? CustomCalculationFormula { get; set; }

        public DateTime? AgreementDate { get; set; }
        public string? AgreementNotaryName { get; set; }
        public string? AgreementCourt { get; set; }
        public string? AgreementReferenceNo { get; set; }
        public IFormFile? AgreementFile { get; set; }

        public RevenueDistribMethod? RevenueDistribMethod { get; set; }
        public ExpenseBearingMethod? ExpenseBearingMethod { get; set; }
        [Range(1, 28)]
        public int? RevenueDistribDay { get; set; }

        public List<PartnershipConditionRuleDto>? ConditionRules { get; set; }

        public bool? IsActive { get; set; }
        public string? Notes { get; set; }
    }

    public class PartnershipDetailDto
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public string PropertyNameAr { get; set; } = string.Empty;
        public string PropertyWqfNumber { get; set; } = string.Empty;
        public PartnershipType PartnershipType { get; set; }
        public decimal WaqfSharePercent { get; set; }
        public decimal PartnerSharePercent { get; set; }
        public string PartnerName { get; set; } = string.Empty;
        public string? PartnerNameEn { get; set; }
        public PartnerType PartnerType { get; set; }
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
        public DateTime? AgreementDate { get; set; }
        public string? AgreementNotaryName { get; set; }
        public string? AgreementCourt { get; set; }
        public string? AgreementReferenceNo { get; set; }
        public string? AgreementDocUrl { get; set; }

        public DateTime? UsufructStartDate { get; set; }
        public DateTime? UsufructEndDate { get; set; }
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

        public RevenueDistribMethod RevenueDistribMethod { get; set; }
        public ExpenseBearingMethod ExpenseBearingMethod { get; set; }
        public int? RevenueDistribDay { get; set; }
        public DateTime? LastDistribDate { get; set; }
        public DateTime? NextDistribDate { get; set; }

        public List<int> OwnedFloorNumbersList { get; set; } = new();
        public List<long> OwnedUnitsList { get; set; } = new();

        public int? DaysUntilExpiry { get; set; }
        public bool IsExpiringSoon { get; set; }
        public bool IsExpired { get; set; }

        public decimal TotalDistributed { get; set; }
        public decimal TotalWaqfReceived { get; set; }
        public decimal TotalPartnerReceived { get; set; }
        public decimal PendingTransferAmount { get; set; }
        public DateTime? LastContactDate { get; set; }
        public string? LastContactType { get; set; }
        public int ConditionRulesCount { get; set; }
        public bool IsActive { get; set; }
        public string? Notes { get; set; }
    }

    public class PartnershipListItemDto
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public string PropertyNameAr { get; set; } = string.Empty;
        public string PropertyWqfNumber { get; set; } = string.Empty;
        public string PartnerName { get; set; } = string.Empty;
        public PartnerType PartnerType { get; set; }
        public string PartnerPhone { get; set; } = string.Empty;
        public string? PartnerEmail { get; set; }
        public string? PartnerNationalId { get; set; }
        public string? PartnerAddress { get; set; }
        public PartnershipType PartnershipType { get; set; }
        public decimal WaqfSharePercent { get; set; }
        public decimal PartnerSharePercent { get; set; }
        public DateTime? PartnershipEndDate { get; set; }
        public int? DaysUntilExpiry { get; set; }
        public bool IsActive { get; set; }
        public bool IsExpiringSoon { get; set; }
        public bool IsExpired { get; set; }
        public decimal TotalDistributed { get; set; }
        public decimal PendingTransferAmount { get; set; }
        public RevenueDistribMethod RevenueDistribMethod { get; set; }
        public ExpenseBearingMethod ExpenseBearingMethod { get; set; }
        public DateTime? NextDistribDate { get; set; }
    }

    public class RevenueDistributionCreateDto
    {
        [Required]
        public int PartnershipId { get; set; }

        [Required]
        public string PeriodLabel { get; set; } = string.Empty;

        [Required]
        public DateTime PeriodStartDate { get; set; }

        [Required]
        public DateTime PeriodEndDate { get; set; }

        [Required]
        [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
        public decimal TotalRevenue { get; set; }

        [Range(typeof(decimal), "0", "79228162514264337593543950335")]
        public decimal TotalExpenses { get; set; }

        public string DistributionType { get; set; } = "Revenue";
        public string? SeasonLabel { get; set; }
        public string? TransferMethod { get; set; }
        public string? Notes { get; set; }
    }

    public class RevenueDistributionDto
    {
        public int Id { get; set; }
        public int PartnershipId { get; set; }
        public int PropertyId { get; set; }
        public string PartnerName { get; set; } = string.Empty;
        public string PropertyNameAr { get; set; } = string.Empty;
        public string PeriodLabel { get; set; } = string.Empty;
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public string DistributionType { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetRevenue { get; set; }
        public decimal WaqfAmount { get; set; }
        public decimal PartnerAmount { get; set; }
        public decimal WaqfPercentSnapshot { get; set; }
        public TransferStatus TransferStatus { get; set; }
        public DateTime? TransferDate { get; set; }
        public string? TransferMethod { get; set; }
        public string? TransferReference { get; set; }
        public string? TransferBankName { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class RevenueCalculationResultDto
    {
        public decimal WaqfAmount { get; set; }
        public decimal PartnerAmount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetRevenue { get; set; }
        public decimal WaqfPercent { get; set; }
        public string CalculationMethod { get; set; } = string.Empty;
        public string CalculationDetail { get; set; } = string.Empty;
        public string? AppliedRuleName { get; set; }
    }

    public class PartnershipConditionRuleDto
    {
        public int? Id { get; set; }
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
    }

    public class PartnershipExpenseEntryDto
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
        public DateTime CreatedAt { get; set; }
    }

    public class CreatePartnershipExpenseDto
    {
        [Required]
        public int PartnershipId { get; set; }

        [Required]
        public string PeriodLabel { get; set; } = string.Empty;

        [Required]
        public DateTime PeriodStartDate { get; set; }

        [Required]
        public DateTime PeriodEndDate { get; set; }

        [Required]
        public string ExpenseType { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
        public decimal Amount { get; set; }

        public string? ReferenceNo { get; set; }
        public string? Notes { get; set; }
    }

    public class PartnerContactDto
    {
        public int Id { get; set; }
        public int PartnershipId { get; set; }
        public ContactType ContactType { get; set; }
        public ContactDirection ContactDirection { get; set; }
        public string? Subject { get; set; }
        public string? MessageBody { get; set; }
        public string? RecipientAddress { get; set; }
        public DateTime SentAt { get; set; }
        public int SentById { get; set; }
        public string PartnerName { get; set; } = string.Empty;
        public bool IsAutomatic { get; set; }
        public string? DeliveryStatus { get; set; }
        public string? ExternalMessageId { get; set; }
        public int? LinkedDistributionId { get; set; }
        public string? Notes { get; set; }
    }

    public class SendCommunicationDto
    {
        [Required]
        public int PartnershipId { get; set; }

        [Required]
        public ContactType ContactType { get; set; }

        public string? Subject { get; set; }

        [Required]
        public string MessageBody { get; set; } = string.Empty;

        public int? LinkedDistributionId { get; set; }
    }

    public class PartnerStatementDto
    {
        public string PartnerName { get; set; } = string.Empty;
        public string PropertyNameAr { get; set; } = string.Empty;
        public string WqfNumber { get; set; } = string.Empty;
        public PartnershipType PartnershipType { get; set; }
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalWaqfAmount { get; set; }
        public decimal TotalPartnerAmount { get; set; }
        public List<RevenueDistributionDto> Distributions { get; set; } = new();
        public List<RevenueDistributionDto> TransferHistory { get; set; } = new();
        public decimal PendingAmount { get; set; }
    }

    public class RevenuePreviewDto : RevenueCalculationResultDto
    {
        public List<string> Warnings { get; set; } = new();
    }

    public class PartnershipFilterRequest
    {
        public int? GovernorateId { get; set; }
        public int? DistrictId { get; set; }
        public PartnershipType? PartnershipType { get; set; }
        public PartnerType? PartnerType { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? ExpiryDateFrom { get; set; }
        public DateTime? ExpiryDateTo { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class PartnershipStatsDto
    {
        public int TotalCount { get; set; }
        public int ActiveCount { get; set; }
        public decimal TotalWaqfRevenueThisMonth { get; set; }
        public decimal TotalPendingTransfers { get; set; }
        public int ExpiringIn30Days { get; set; }
        public int ExpiringIn90Days { get; set; }
        public int ExpiredCount { get; set; }
        public Dictionary<string, int> ByPartnershipType { get; set; } = new();
    }

    public class PartnerSummaryDto
    {
        public string Name { get; set; } = string.Empty;
        public PartnerType Type { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? NationalId { get; set; }
        public string? Address { get; set; }
        public int PartnershipCount { get; set; }
        public decimal TotalWaqfRevenue { get; set; }
        public decimal TotalPartnerShare { get; set; }
        public DateTime? LastPartnershipDate { get; set; }
    }

    public class CreatePartnerDto
    {
        public string PartnerName { get; set; } = string.Empty;
        public string? PartnerNameEn { get; set; }
        public PartnerType PartnerType { get; set; }
        public string? PartnerNationalId { get; set; }
        public string? PartnerRegistrationNo { get; set; }
        public string PartnerPhone { get; set; } = string.Empty;
        public string? PartnerEmail { get; set; }
        public string? PartnerWhatsApp { get; set; }
        public string? PartnerAddress { get; set; }
    }
}
