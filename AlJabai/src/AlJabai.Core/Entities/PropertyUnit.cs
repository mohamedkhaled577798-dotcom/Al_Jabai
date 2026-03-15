using AlJabai.Core.Common;

namespace AlJabai.Core.Entities;

public class PropertyUnit : BaseEntity
{
    public Guid PropertyId { get; set; }
    public string UnitNumber { get; set; } = string.Empty;
    public string? Floor { get; set; }
    public decimal? Area { get; set; }
    public string? UnitType { get; set; }
    public bool IsAvailable { get; set; } = true;

    public Property Property { get; set; } = default!;
}
