using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using WaqfSystem.Application.DTOs.Property;

namespace WaqfSystem.Web.ViewModels
{
    public class FloorUpsertViewModel
    {
        public CreateFloorDto Floor { get; set; } = new();
        public SelectList? FloorUsages { get; set; }
        public SelectList? StructuralConditions { get; set; }
        public bool IsEdit => Floor.PropertyId > 0; // Simplified
    }

    public class UnitUpsertViewModel
    {
        public CreateUnitDto Unit { get; set; } = new();
        public SelectList? UnitTypes { get; set; }
        public SelectList? OccupancyStatuses { get; set; }
        public SelectList? FurnishedStatuses { get; set; }
        public SelectList? StructuralConditions { get; set; }
    }
}
