using AlJabai.Core.Common;
using AlJabai.Core.Enums;

namespace AlJabai.Core.Entities;

public class ContractTemplate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public ContractType ContractType { get; set; }
    public string? DocxFilePath { get; set; }
    public string? OriginalFileName { get; set; }
    public long FileSizeBytes { get; set; }
    public string? ExtractedVariablesJson { get; set; }
    public string? MissingVariablesJson { get; set; }
    public string? CustomVariablesJson { get; set; }
    public DateTime? LastParsedAt { get; set; }
    public string RequiredFieldsJson { get; set; } = "[]";
    public string? DefaultClausesJson { get; set; }
    public int Version { get; set; } = 1;
    public bool IsActive { get; set; } = true;

    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
}
