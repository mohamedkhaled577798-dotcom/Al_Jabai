using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WaqfSystem.Application.Services;
using WaqfSystem.Web.ViewModels;

namespace WaqfSystem.Web.Controllers
{
    [Authorize]
    public class DashboardController : BaseController
    {
        private readonly IPropertyService _propertyService;
        private readonly IWorkflowService _workflowService;

        public DashboardController(IPropertyService propertyService, IWorkflowService workflowService)
        {
            _propertyService = propertyService;
            _workflowService = workflowService;
        }

        public async Task<IActionResult> Index()
        {
            // Simplified — in production, these would be optimized aggregate queries
            var viewModel = new DashboardViewModel
            {
                TotalProperties = 1250, // Stubs for initial UI development
                PendingApproval = 45,
                ApprovedProperties = 1100,
                RejectedProperties = 15,
                TotalEstimatedValue = 54000000,
                AverageDqsScore = 78.5m,
                ActiveMissions = 12,
                NotificationsCount = 5
            };

            // Add some dummy stats for charts
            viewModel.GovernorateStats.Add(new GovernorateStatsViewModel { Name = "Baghdad", Count = 450, Percentage = 36 });
            viewModel.GovernorateStats.Add(new GovernorateStatsViewModel { Name = "Basra", Count = 200, Percentage = 16 });
            viewModel.GovernorateStats.Add(new GovernorateStatsViewModel { Name = "Nineveh", Count = 150, Percentage = 12 });

            return View(viewModel);
        }
    }
}
