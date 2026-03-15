using AlJabai.Core.Common;
using AlJabai.Core.Enums;

namespace AlJabai.Core.Entities;

public class PaymentSchedule : BaseEntity
{
    public Guid ContractId { get; set; }
    public int PeriodNumber { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public decimal BaseAmount { get; set; }
    public decimal LateFee { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public decimal PaidAmount { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? RemindersSentJson { get; set; }
    public string? Notes { get; set; }

    public Contract Contract { get; set; } = default!;
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
