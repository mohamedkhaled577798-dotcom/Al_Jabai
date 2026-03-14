using AlJabai.Core.Common;

namespace AlJabai.Core.Entities;

public class Property : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Governorate { get; set; }
    public string? PropertyType { get; set; }
    public int TotalUnits { get; set; } = 1;
    public string? Description { get; set; }

    public ICollection<PropertyUnit> Units { get; set; } = new List<PropertyUnit>();
    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
}
