using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using WaqfSystem.Application.DTOs.Common;
using WaqfSystem.Application.DTOs.Property;

namespace WaqfSystem.Web.ViewModels
{
    public class PropertyIndexViewModel
    {
        public PagedResult<PropertyListDto> Results { get; set; } = new();
        public PropertyFilterRequest Filter { get; set; } = new();
        public SelectList? Governorates { get; set; }
        public SelectList? PropertyTypes { get; set; }
        public SelectList? OwnershipTypes { get; set; }
        public SelectList? ApprovalStages { get; set; }
    }

    public class PropertyUpsertViewModel
    {
        public UpdatePropertyDto Property { get; set; } = new();
        public SelectList? Governorates { get; set; }
        public SelectList? PropertyTypes { get; set; }
        public SelectList? PropertyCategories { get; set; }
        public SelectList? OwnershipTypes { get; set; }
        public SelectList? WaqfTypes { get; set; }
        public SelectList? ConstructionTypes { get; set; }
        public SelectList? StructuralConditions { get; set; }
        public bool IsEdit => Property.Id > 0;
    }
}
