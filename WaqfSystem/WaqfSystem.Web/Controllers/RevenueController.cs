using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WaqfSystem.Application.DTOs.Revenue;
using WaqfSystem.Application.Services;
using WaqfSystem.Core.Entities;
using WaqfSystem.Core.Enums;
using WaqfSystem.Web.Models.Revenue;

namespace WaqfSystem.Web.Controllers
{
    [Authorize]
    public class RevenueController : BaseController
    {
        private readonly IRevenueCollectionService _revenueCollectionService;
        private readonly IRentContractService _rentContractService;
        private readonly ISmartCollectionService _smartCollectionService;
        private readonly IPropertyStructureService _propertyStructureService;
        private readonly ILogger<RevenueController> _logger;

        public RevenueController(
            IRevenueCollectionService revenueCollectionService,
            IRentContractService rentContractService,
            ISmartCollectionService smartCollectionService,
            IPropertyStructureService propertyStructureService,
            ILogger<RevenueController> logger)
        {
            _revenueCollectionService = revenueCollectionService;
            _rentContractService = rentContractService;
            _smartCollectionService = smartCollectionService;
            _propertyStructureService = propertyStructureService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> SmartCollect(string? period = null, long? unitId = null)
        {
            var periodLabel = string.IsNullOrWhiteSpace(period) ? DateTime.Today.ToString("yyyy-MM") : period;
            var dashboard = await _smartCollectionService.GetTodayDashboardAsync(CurrentUserId, periodLabel);

            var vm = new SmartCollectViewModel
            {
                PeriodLabel = periodLabel,
                Dashboard = dashboard,
                Suggestions = dashboard.SmartSuggestions,
                QuickCollect = new QuickCollectDto
                {
                    CollectionDate = DateTime.Today,
                    PeriodLabel = periodLabel,
                    UnitId = (int?)unitId
                },
                BatchCollect = new BatchCollectDto
                {
                    CollectionDate = DateTime.Today,
                    PeriodLabel = periodLabel
                }
            };

            return View("SmartCollect", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickCollect(QuickCollectDto dto)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "البيانات المدخلة غير مكتملة" });
            }

            try
            {
                var revenue = await _revenueCollectionService.QuickCollectAsync(dto, CurrentUserId);
                var suggestions = await _smartCollectionService.GetSuggestionsAsync(CurrentUserId, dto.PeriodLabel);
                return Json(new
                {
                    success = true,
                    revenueCode = revenue.RevenueCode,
                    newSuggestions = suggestions,
                    message = "تم تسجيل التحصيل بنجاح"
                });
            }
            catch (CollisionException ex)
            {
                return Json(new { success = false, message = ex.Message, lockedBy = ex.LockedBy });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "فشل التحصيل السريع");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BatchCollect(BatchCollectDto dto)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "تحقق من المدخلات" });
            }

            var result = await _smartCollectionService.BatchCollectAsync(dto, CurrentUserId);
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string q, int page = 1)
        {
            var result = await _smartCollectionService.SearchAsync(q, CurrentUserId, page);
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> CheckVariance(long? contractId, decimal amount)
        {
            var alert = await _revenueCollectionService.PreviewVarianceAsync(contractId, amount);
            return Json(alert);
        }

        [HttpGet]
        public async Task<IActionResult> GetSuggestions(string? period)
        {
            var suggestions = await _smartCollectionService.GetSuggestionsAsync(CurrentUserId, period);
            return Json(suggestions);
        }

        [HttpGet]
        public async Task<IActionResult> CheckCollision(long propertyId, string level, long? floorId, long? unitId, string period)
        {
            var result = await _revenueCollectionService.CheckCollisionAsync(propertyId, level, floorId, unitId, period);
            return Json(new { hasCollision = result.HasCollision, message = result.Message, lockedBy = result.LockedBy });
        }

        [HttpGet]
        public async Task<IActionResult> TodayDashboard(string period)
        {
            var dashboard = await _smartCollectionService.GetTodayDashboardAsync(CurrentUserId, period);
            return PartialView("_TodayStats", dashboard);
        }

        [HttpGet]
        public async Task<IActionResult> Index(RevenueFilterRequest filter)
        {
            var model = await _revenueCollectionService.GetPagedAsync(filter, CurrentUserId);
            return View(model);
        }

        [HttpGet]
        public IActionResult Collect(long propertyId, string? period)
        {
            var dto = new CollectRevenueDto
            {
                PropertyId = (int)propertyId,
                PeriodLabel = string.IsNullOrWhiteSpace(period) ? DateTime.Today.ToString("yyyy-MM") : period,
                CollectionDate = DateTime.Today
            };
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Collect(CollectRevenueDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                await _revenueCollectionService.CollectAsync(dto, CurrentUserId);
                SuccessMessage("تم تسجيل التحصيل بنجاح");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Contracts(ContractFilterRequest filter)
        {
            var contracts = await _rentContractService.GetPagedAsync(filter, CurrentUserId);
            return View(contracts);
        }

        [HttpGet]
        public IActionResult CreateContract(long? unitId)
        {
            return View(new CreateContractDto { UnitId = (int)(unitId ?? 0), StartDate = DateTime.Today, EndDate = DateTime.Today.AddYears(1) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateContract(CreateContractDto dto, IFormFile? contractFile)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            await _rentContractService.CreateAsync(dto, CurrentUserId);
            SuccessMessage("تم إنشاء العقد بنجاح");
            return RedirectToAction(nameof(Contracts));
        }

        [HttpGet]
        public async Task<IActionResult> ContractDetail(long id)
        {
            var contract = await _rentContractService.GetByIdAsync(id, CurrentUserId);
            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TerminateContract(TerminateContractDto dto)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(ContractDetail), new { id = dto.ContractId });
            }

            await _rentContractService.TerminateAsync(dto, CurrentUserId);
            SuccessMessage("تم إنهاء العقد بنجاح");
            return RedirectToAction(nameof(Contracts));
        }

        [HttpGet]
        public async Task<IActionResult> ManageStructure(long propertyId, string? period = null)
        {
            var dto = await _propertyStructureService.GetStructureAsync(propertyId, period ?? DateTime.Today.ToString("yyyy-MM"), CurrentUserId);
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddFloor(long propertyId, string floorLabel)
        {
            SuccessMessage("تمت إضافة الطابق");
            return RedirectToAction(nameof(ManageStructure), new { propertyId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateFloor(long propertyId, long floorId, string floorLabel)
        {
            SuccessMessage("تم تعديل الطابق");
            return RedirectToAction(nameof(ManageStructure), new { propertyId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddUnit(long propertyId, long floorId, string unitNumber)
        {
            SuccessMessage("تمت إضافة الوحدة");
            return RedirectToAction(nameof(ManageStructure), new { propertyId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateUnit(long propertyId, long unitId, string unitNumber)
        {
            SuccessMessage("تم تعديل الوحدة");
            return RedirectToAction(nameof(ManageStructure), new { propertyId });
        }

        [HttpGet]
        public async Task<IActionResult> Analytics(long propertyId, string? period = null)
        {
            var periodLabel = period ?? DateTime.Today.ToString("yyyy-MM");
            var dashboard = await _smartCollectionService.GetTodayDashboardAsync(CurrentUserId, periodLabel);
            ViewBag.PropertyId = propertyId;
            ViewBag.PeriodLabel = periodLabel;
            return View(dashboard);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteToday(long id, string reason)
        {
            await _revenueCollectionService.DeleteTodayAsync(id, reason, CurrentUserId);
            return Json(new { success = true, message = "تم الحذف بنجاح" });
        }

        [HttpGet]
        public async Task<IActionResult> PendingBatch(string period)
        {
            var items = await _smartCollectionService.GetPendingForBatchAsync(CurrentUserId, period);
            return Json(items);
        }
    }
}
