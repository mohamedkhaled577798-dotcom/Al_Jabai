using System.Collections.Generic;
using WaqfSystem.Application.DTOs.Mobile;
using WaqfSystem.Application.DTOs.Property;

namespace WaqfSystem.Web.ViewModels
{
    public class GisExplorerViewModel
    {
        public List<PropertyMapPointDto> ActiveProperties { get; set; } = new();
        public PropertyFilterRequest Filter { get; set; } = new();
    }

    public class MissionIndexViewModel
    {
        public List<Core.Entities.InspectionMission> MissionList { get; set; } = new();
    }

    public class LoginViewModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
        public string? ReturnUrl { get; set; }
    }
}
