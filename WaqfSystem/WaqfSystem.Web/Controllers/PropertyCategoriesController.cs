using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WaqfSystem.Application.DTOs.Partnership;
using WaqfSystem.Application.Services;
using WaqfSystem.Web.ViewModels.Properties;

namespace WaqfSystem.Web.Controllers
{
    [Authorize]
    public class PropertyCategoriesController : Controller
    {
        private readonly IPartnershipService _partnershipService;
        private readonly IGeographicService _geographicService;

        public PropertyCategoriesController(IPartnershipService partnershipService, IGeographicService geographicService)
        {
            _partnershipService = partnershipService;
            _geographicService = geographicService;
        }

        [HttpGet]
        public async Task<IActionResult> Partnership(PartnershipFilterRequest filter)
        {
            var partnerships = await _partnershipService.GetPagedAsync(filter);
            var all = await _partnershipService.GetPagedAsync(new PartnershipFilterRequest { Page = 1, PageSize = 10000 });

            var vm = new PartnershipIndexViewModel
            {
                Partnerships = partnerships,
                Filter = filter,
                Governorates = (await _geographicService.GetGovernoratesAsync())
                    .Select(g => new SelectListItem(g.NameAr, g.Id.ToString()))
                    .ToList(),
                Stats = new PartnershipStatsDto
                {
                    TotalCount = all.TotalCount,
                    ActiveCount = all.Items.Count(x => x.IsActive),
                    TotalWaqfRevenueThisMonth = all.Items.Sum(x => x.TotalDistributed),
                    TotalPendingTransfers = all.Items.Sum(x => x.PendingTransferAmount),
                    ExpiringIn30Days = all.Items.Count(x => x.DaysUntilExpiry.HasValue && x.DaysUntilExpiry.Value <= 30 && x.DaysUntilExpiry.Value >= 0),
                    ExpiringIn90Days = all.Items.Count(x => x.DaysUntilExpiry.HasValue && x.DaysUntilExpiry.Value <= 90 && x.DaysUntilExpiry.Value >= 0),
                    ExpiredCount = all.Items.Count(x => x.IsExpired),
                    ByPartnershipType = all.Items.GroupBy(x => x.PartnershipType.ToString()).ToDictionary(x => x.Key, x => x.Count())
                }
            };

            return View("PartnershipIndex", vm);
        }
    }
}
