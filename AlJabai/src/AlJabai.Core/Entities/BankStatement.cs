using AlJabai.Core.Common;
using AlJabai.Core.Enums;

namespace AlJabai.Core.Entities;

public class BankStatement : BaseEntity
{
    public DateTime TransactionDate { get; set; }
    public DateTime? ValueDate { get; set; }
    public string? Description { get; set; }
    public string? Reference { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal? Balance { get; set; }
    public Guid? MatchedPaymentId { get; set; }
    public ReconciliationStatus MatchStatus { get; set; } = ReconciliationStatus.Pending;
    public Guid? ImportBatchId { get; set; }
}
