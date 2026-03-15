using AlJabai.Core.Common;
using AlJabai.Core.Enums;

namespace AlJabai.Core.Entities;

public class Payment : BaseEntity
{
    public string PaymentNumber { get; set; } = string.Empty;
    public Guid ContractId { get; set; }
    public Guid? ScheduleId { get; set; }
    public Guid TenantId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "KWD";
    public DateTime PaymentDate { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentChannel Channel { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? BankName { get; set; }
    public string? ChequeNumber { get; set; }
    public DateTime? ChequeDate { get; set; }
    public Guid? ReceivedBy { get; set; }
    public bool IsPartial { get; set; }
    public decimal RemainingBalance { get; set; }
    public string? ReceiptPdfUrl { get; set; }
    public ReconciliationStatus ReconciliationStatus { get; set; } = ReconciliationStatus.Pending;
    public DateTime? ReconciledAt { get; set; }
    public string? Notes { get; set; }

    public Contract Contract { get; set; } = default!;
    public Tenant Tenant { get; set; } = default!;
    public PaymentSchedule? Schedule { get; set; }
}
