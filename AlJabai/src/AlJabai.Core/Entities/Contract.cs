using AlJabai.Core.Common;
using AlJabai.Core.Enums;

namespace AlJabai.Core.Entities;

public class Contract : BaseEntity
{
    public string ContractNumber { get; set; } = string.Empty;
    public Guid TemplateId { get; set; }
    public Guid TenantId { get; set; }
    public Guid PropertyId { get; set; }
    public Guid? UnitId { get; set; }
    public ContractType ContractType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal RentAmount { get; set; }
    public PaymentCycle PaymentCycle { get; set; }
    public int PaymentDay { get; set; } = 1;
    public decimal AnnualIncreaseRate { get; set; }
    public DateTime? NextIncreaseDate { get; set; }
    public decimal SecurityDeposit { get; set; }
    public ContractStatus Status { get; set; } = ContractStatus.Draft;
    public bool AutoRenew { get; set; }
    public string? PdfUrl { get; set; }
    public string? SignedPdfUrl { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? Notes { get; set; }
    public string? MetadataJson { get; set; }

    public Tenant Tenant { get; set; } = default!;
    public Property Property { get; set; } = default!;
    public PropertyUnit? Unit { get; set; }
    public ContractTemplate Template { get; set; } = default!;
    public ICollection<PaymentSchedule> PaymentSchedules { get; set; } = new List<PaymentSchedule>();
    public ICollection<ContractAuditLog> AuditLogs { get; set; } = new List<ContractAuditLog>();
}
