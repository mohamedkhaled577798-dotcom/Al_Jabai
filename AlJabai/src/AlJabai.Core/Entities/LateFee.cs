using AlJabai.Core.Common;
using AlJabai.Core.Enums;

namespace AlJabai.Core.Entities;

public class LateFee : BaseEntity
{
    public Guid ScheduleId { get; set; }
    public Guid ContractId { get; set; }
    public int DaysOverdue { get; set; }
    public decimal FeeAmount { get; set; }
    public decimal FeeRate { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public Guid? WaivedBy { get; set; }
    public string? WaivedReason { get; set; }
}
