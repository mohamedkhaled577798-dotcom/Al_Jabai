using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WaqfSystem.Core.Enums;

namespace WaqfSystem.Application.DTOs.Revenue
{
    public class PagedRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class SmartSuggestionDto
    {
        public string SuggestionId { get; set; } = string.Empty;
        public SuggestionType SuggestionType { get; set; }
        public int Priority { get; set; }
        
        public int PropertyId { get; set; }
        public string PropertyNameAr { get; set; } = string.Empty;
        public int? FloorId { get; set; }
        public string? FloorLabel { get; set; }
        public int? UnitId { get; set; }
        public string? UnitNumber { get; set; }
        public string? UnitType { get; set; }
        
        public CollectionLevel CollectionLevel { get; set; }
        public string? TenantNameAr { get; set; }
        public string? TenantPhone { get; set; }
        public int? ContractId { get; set; }
        
        public decimal ExpectedAmount { get; set; }
        public string? ContractType { get; set; }
        public DateTime? DueDate { get; set; }
        public int OverdueDays { get; set; }
        public string PeriodLabel { get; set; } = string.Empty;
        
        public List<AmountChipDto> SuggestedAmountChips { get; set; } = new();
        public bool IsLocked { get; set; }
        public string? LockReason { get; set; }
        public string UrgencyColor { get; set; } = "#6b7280"; // Gray default
    }

    public class AmountChipDto
    {
        public string Label { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public bool IsDefault { get; set; }
        public ChipType ChipType { get; set; }
    }

    public class QuickCollectDto
    {
        [Required]
        public int PropertyId { get; set; }
        
        [Required]
        public CollectionLevel CollectionLevel { get; set; }
        
        public int? FloorId { get; set; }
        public int? UnitId { get; set; }
        public int? ContractId { get; set; }
        
        [Required]
        public string PeriodLabel { get; set; } = string.Empty;
        
        public DateTime? PeriodStartDate { get; set; }
        public DateTime? PeriodEndDate { get; set; }
        
        [Required, Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
        
        public decimal? ExpectedAmount { get; set; }
        
        [Required]
        public DateTime CollectionDate { get; set; } = DateTime.Today;
        
        public string? PaymentMethod { get; set; }
        public string? ReceiptNumber { get; set; }
        public string? PayerNameAr { get; set; }
        public string? Notes { get; set; }
        public string? SuggestionId { get; set; }
        public string? VarianceApprovalNote { get; set; }
    }

    public class BatchCollectDto
    {
        [Required]
        public string PeriodLabel { get; set; } = string.Empty;
        
        [Required]
        public DateTime CollectionDate { get; set; } = DateTime.Today;
        
        public string? PaymentMethod { get; set; }
        public string? Notes { get; set; }
        
        [Required]
        public List<BatchCollectItemDto> Items { get; set; } = new();
    }

    public class BatchCollectItemDto
    {
        [Required]
        public int PropertyId { get; set; }
        
        [Required]
        public CollectionLevel CollectionLevel { get; set; }
        
        public int? FloorId { get; set; }
        public int? UnitId { get; set; }
        public int? ContractId { get; set; }
        
        [Required, Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
        
        public decimal? ExpectedAmount { get; set; }
        public string? PayerNameAr { get; set; }
        public string? ReceiptNumber { get; set; }
    }

    public class BatchCollectResultDto
    {
        public string BatchCode { get; set; } = string.Empty;
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public decimal TotalAmount { get; set; }
        public List<BatchItemResultDto> Results { get; set; } = new();
    }

    public class BatchItemResultDto
    {
        public string UnitLabel { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public bool Success { get; set; }
        public string? RevenueCode { get; set; }
        public string? Error { get; set; }
    }

    public class TodayDashboardDto
    {
        public decimal CollectedToday { get; set; }
        public int CollectedTodayCount { get; set; }
        public decimal MonthCollected { get; set; }
        public decimal MonthExpected { get; set; }
        public decimal MonthCollectionRate { get; set; }
        public int OverdueCount { get; set; }
        public decimal OverdueAmount { get; set; }
        public int PendingCount { get; set; }
        
        public List<SmartSuggestionDto> SmartSuggestions { get; set; } = new();
        public List<DayProgressDto> MonthProgress { get; set; } = new();
        public List<FloorCollectionDto> CollectionByFloor { get; set; } = new();
    }

    public class DayProgressDto
    {
        public DateTime Date { get; set; }
        public decimal Collected { get; set; }
        public decimal Expected { get; set; }
        public bool IsToday { get; set; }
    }

    public class FloorCollectionDto
    {
        public string FloorLabel { get; set; } = string.Empty;
        public decimal Collected { get; set; }
        public decimal Expected { get; set; }
        public decimal Rate { get; set; }
        public int UncollectedUnits { get; set; }
    }

    public class SearchResultDto
    {
        public int UnitId { get; set; }
        public string UnitNumber { get; set; } = string.Empty;
        public string UnitType { get; set; } = string.Empty;
        public string FloorLabel { get; set; } = string.Empty;
        public string PropertyNameAr { get; set; } = string.Empty;
        public string WqfNumber { get; set; } = string.Empty;
        public string? TenantNameAr { get; set; }
        public string? TenantPhone { get; set; }
        public decimal ActiveContractRent { get; set; }
        public decimal ExpectedAmount { get; set; }
        public string CollectionStatusThisMonth { get; set; } = "Pending";
        public int OverdueDays { get; set; }
        public DateTime? LastCollectionDate { get; set; }
        public CollectionLevel CollectionLevel { get; set; } = CollectionLevel.Unit;
        public bool IsLocked { get; set; }
        public string? LockReason { get; set; }
        public int PropertyId { get; set; }
        public int FloorId { get; set; }
        public int? ContractId { get; set; }
        public string PeriodLabel { get; set; } = string.Empty;
    }

    public class VarianceAlertDto
    {
        public decimal ExpectedAmount { get; set; }
        public decimal ActualAmount { get; set; }
        public decimal Variance { get; set; }
        public decimal VariancePercent { get; set; }
        public AlertLevel AlertLevel { get; set; }
        public string AlertMessage { get; set; } = string.Empty;
        public bool RequiresApproval { get; set; }
    }

    public class RevenueFilterRequest : PagedRequest
    {
        public string? PeriodLabel { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Search { get; set; }
        public CollectionLevel? Level { get; set; }
    }

    public class ContractFilterRequest : PagedRequest
    {
        public int? PropertyId { get; set; }
        public ContractStatus? Status { get; set; }
        public string? Search { get; set; }
        public bool ActiveOnly { get; set; }
    }

    public class CollectRevenueDto
    {
        public int PropertyId { get; set; }
        public int? FloorId { get; set; }
        public int? UnitId { get; set; }
        public int? ContractId { get; set; }
        public CollectionLevel CollectionLevel { get; set; }
        public string PeriodLabel { get; set; } = string.Empty;
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public DateTime CollectionDate { get; set; }
        public decimal Amount { get; set; }
        public decimal? ExpectedAmount { get; set; }
        public string? PaymentMethod { get; set; }
        public string? ReceiptNumber { get; set; }
        public string? PayerNameAr { get; set; }
        public string? Notes { get; set; }
    }

    public class CreateContractDto
    {
        public int PropertyId { get; set; }
        public int? FloorId { get; set; }
        public int UnitId { get; set; }
        public string TenantNameAr { get; set; } = string.Empty;
        public string? TenantPhone { get; set; }
        public decimal RentAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? ContractType { get; set; }
        public int GracePeriodDays { get; set; }
        public bool AllowsPartialPayments { get; set; }
        public decimal PenaltyPerDay { get; set; }
        public string? Notes { get; set; }
    }

    public class TerminateContractDto
    {
        public int ContractId { get; set; }
        public DateTime TerminationDate { get; set; }
        public string? Reason { get; set; }
    }

    public class PropertyStructureDto
    {
        public long PropertyId { get; set; }
        public string PropertyNameAr { get; set; } = string.Empty;
        public List<FloorStructureDto> Floors { get; set; } = new();
    }

    public class FloorStructureDto
    {
        public long FloorId { get; set; }
        public string FloorLabel { get; set; } = string.Empty;
        public bool IsLocked { get; set; }
        public string? LockReason { get; set; }
        public List<UnitStructureDto> Units { get; set; } = new();
    }

    public class UnitStructureDto
    {
        public long UnitId { get; set; }
        public string UnitNumber { get; set; } = string.Empty;
        public string UnitType { get; set; } = string.Empty;
        public bool IsLocked { get; set; }
        public string? LockReason { get; set; }
    }


}
