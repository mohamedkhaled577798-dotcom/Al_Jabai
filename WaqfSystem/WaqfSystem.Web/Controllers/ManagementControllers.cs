using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WaqfSystem.Application.DTOs.Property;
using WaqfSystem.Application.Services;
using WaqfSystem.Web.ViewModels;

namespace WaqfSystem.Web.Controllers
{
    [Authorize]
    public class GisController : BaseController
    {
        private readonly IPropertyService _propertyService;

        public GisController(IPropertyService propertyService)
        {
            _propertyService = propertyService;
        }

        public async Task<IActionResult> Explorer(PropertyFilterRequest filter)
        {
            var points = await _propertyService.GetMapPointsAsync(filter.GovernorateId, filter.PropertyType, filter.ApprovalStage);
            var viewModel = new GisExplorerViewModel
            {
                ActiveProperties = points,
                Filter = filter
            };
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetPoints(int? governorateId, int? type)
        {
            var points = await _propertyService.GetMapPointsAsync(governorateId, (Core.Enums.PropertyType?)type);
            return Json(points);
        }
    }

    [Authorize]
    public class MissionController : BaseController
    {
        private readonly IMissionService _missionService;

        public MissionController(IMissionService missionService)
        {
            _missionService = missionService;
        }

        public async Task<IActionResult> Index()
        {
            var missions = await _missionService.GetMyMissionsAsync(CurrentUserId);
            var viewModel = new MissionIndexViewModel { MissionList = missions };
            return View(viewModel);
        }

        public async Task<IActionResult> Details(int id)
        {
            // Placeholder for mission details
            return View();
        }
    }
}
