using AlJabai.Core.Common;
using AlJabai.Core.Enums;

namespace AlJabai.Core.Entities;

public class Tenant : BaseEntity
{
    public TenantType TenantType { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? PhoneAlt { get; set; }
    public string? Address { get; set; }
    public string? CompanyName { get; set; }
    public string? AuthorizedPerson { get; set; }
    public string? TaxNumber { get; set; }
    public TenantStatus Status { get; set; } = TenantStatus.Active;
    public string? Notes { get; set; }
    public string? DocumentsJson { get; set; }

    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
}
