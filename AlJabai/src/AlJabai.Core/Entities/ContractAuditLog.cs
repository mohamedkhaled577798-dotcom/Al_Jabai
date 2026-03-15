using AlJabai.Core.Common;

namespace AlJabai.Core.Entities;

public class ContractAuditLog : BaseEntity
{
    public Guid ContractId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldValuesJson { get; set; }
    public string? NewValuesJson { get; set; }
    public Guid PerformedBy { get; set; }
    public string? IpAddress { get; set; }
    public string? Notes { get; set; }

    public Contract Contract { get; set; } = default!;
}
