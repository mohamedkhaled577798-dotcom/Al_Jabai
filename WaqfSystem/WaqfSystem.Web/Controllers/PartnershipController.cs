using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using WaqfSystem.Application.DTOs.Partnership;
using WaqfSystem.Application.Services;
using WaqfSystem.Web.ViewModels.Properties;
using WaqfSystem.Core.Enums;

namespace WaqfSystem.Web.Controllers
{
    [Authorize]
    public class PartnershipController : Controller
    {
        private readonly IPartnershipService _partnershipService;
        private readonly IPropertyService _propertyService;
        private readonly IGeographicService _geographicService;
        private readonly ILogger<PartnershipController> _logger;

        public PartnershipController(
            IPartnershipService partnershipService,
            IPropertyService propertyService,
            IGeographicService geographicService,
            ILogger<PartnershipController> logger)
        {
            _partnershipService = partnershipService;
            _propertyService = propertyService;
            _geographicService = geographicService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(PartnershipFilterRequest filter)
        {
            var data = await _partnershipService.GetPagedAsync(filter);
            var all = await _partnershipService.GetPagedAsync(new PartnershipFilterRequest { Page = 1, PageSize = 10000 });

            var vm = new PartnershipIndexViewModel
            {
                Partnerships = data,
                Filter = filter,
                Governorates = (await _geographicService.GetGovernoratesAsync())
                    .Select(g => new SelectListItem(g.NameAr, g.Id.ToString()))
                    .ToList(),
                Stats = new PartnershipStatsDto
                {
                    TotalCount = all.TotalCount,
                    ActiveCount = all.Items.Count(x => x.IsActive),
                    ExpiredCount = all.Items.Count(x => x.IsExpired),
                    ExpiringIn30Days = all.Items.Count(x => x.DaysUntilExpiry.HasValue && x.DaysUntilExpiry.Value <= 30 && x.DaysUntilExpiry.Value >= 0),
                    ExpiringIn90Days = all.Items.Count(x => x.DaysUntilExpiry.HasValue && x.DaysUntilExpiry.Value <= 90 && x.DaysUntilExpiry.Value >= 0),
                    TotalPendingTransfers = all.Items.Sum(x => x.PendingTransferAmount),
                    TotalWaqfRevenueThisMonth = all.Items.Sum(x => x.TotalDistributed),
                    ByPartnershipType = all.Items
                        .GroupBy(x => x.PartnershipType.ToString())
                        .ToDictionary(x => x.Key, x => x.Count())
                }
            };

            return View("Index", vm);
        }

        [HttpGet]
        public async Task<IActionResult> Partners(string searchTerm, PartnerType? partnerType)
        {
            var partners = await _partnershipService.SearchPartnersAsync(searchTerm);

            if (partnerType.HasValue)
            {
                partners = partners.Where(x => x.Type == partnerType.Value).ToList();
            }

            var vm = new PartnerListViewModel
            {
                Partners = partners,
                SearchTerm = searchTerm,
                PartnerType = partnerType
            };

            return View(vm);
        }

        [HttpGet]
        public IActionResult CreatePartner()
        {
            return View(new PartnerCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePartner(CreatePartnerDto dto)
        {
            if (!ModelState.IsValid) return View(new PartnerCreateViewModel { Form = dto });

            try
            {
                await _partnershipService.CreatePartnerAsync(dto, GetUserId());
                
                TempData["Success"] = "تم تسجيل بيانات الشريك بنجاح. يمكنك الآن تخصيص عقارات له.";
                return RedirectToAction(nameof(Partners), new { searchTerm = dto.PartnerName });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(new PartnerCreateViewModel { Form = dto });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ForProperty(int propertyId)
        {
            var list = await _partnershipService.GetByPropertyAsync(propertyId);
            return View("ForProperty", list);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? propertyId, string? partnerName)
        {
            WaqfSystem.Application.DTOs.Property.PropertyDetailDto? property = null;
            if (propertyId.HasValue)
                property = await _propertyService.GetByIdAsync(propertyId.Value);

            var propertiesPaged = await _propertyService.GetPagedAsync(new WaqfSystem.Application.DTOs.Property.PropertyFilterRequest { Page = 1, PageSize = 1000 });

            var vm = new PartnershipCreateViewModel
            {
                Form = new CreatePartnershipDto
                {
                    PropertyId = propertyId ?? 0,
                    PartnerName = partnerName ?? string.Empty
                },
                PropertyName = property?.PropertyName ?? property?.WqfNumber ?? string.Empty,
                PropertyWqfNumber = property?.WqfNumber ?? string.Empty,
                Properties = propertiesPaged.Items
                    .Select(p => new SelectListItem($"{p.PropertyName} ({p.WqfNumber})", p.Id.ToString()))
                    .ToList()
            };

            return View("Create", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePartnershipDto dto, IFormFile? agreementFile)
        {
            try
            {
                var id = await _partnershipService.CreateAsync(dto, agreementFile, GetUserId());
                TempData["Success"] = "تم إضافة الشراكة بنجاح";
                return RedirectToAction(nameof(Detail), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Partnership create failed");
                ModelState.AddModelError(string.Empty, ex.Message);
                var property = await _propertyService.GetByIdAsync(dto.PropertyId);
                var propertiesPaged = await _propertyService.GetPagedAsync(new WaqfSystem.Application.DTOs.Property.PropertyFilterRequest { Page = 1, PageSize = 1000 });

                return View("Create", new PartnershipCreateViewModel
                {
                    Form = dto,
                    PropertyName = property?.PropertyName ?? property?.WqfNumber ?? string.Empty,
                    PropertyWqfNumber = property?.WqfNumber ?? string.Empty,
                    Properties = propertiesPaged.Items
                        .Select(p => new SelectListItem($"{p.PropertyName} ({p.WqfNumber})", p.Id.ToString()))
                        .ToList()
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _partnershipService.GetByIdAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            var vm = new PartnershipEditViewModel
            {
                Form = new UpdatePartnershipDto
                {
                    Id = item.Id,
                    PropertyId = item.PropertyId,
                    PartnershipType = item.PartnershipType,
                    WaqfSharePercent = item.WaqfSharePercent,
                    PartnerName = item.PartnerName,
                    PartnerNameEn = item.PartnerNameEn,
                    PartnerType = item.PartnerType,
                    PartnerNationalId = item.PartnerNationalId,
                    PartnerRegistrationNo = item.PartnerRegistrationNo,
                    PartnerPhone = item.PartnerPhone,
                    PartnerPhone2 = item.PartnerPhone2,
                    PartnerEmail = item.PartnerEmail,
                    PartnerWhatsApp = item.PartnerWhatsApp,
                    PartnerAddress = item.PartnerAddress,
                    PartnerBankName = item.PartnerBankName,
                    PartnerBankIBAN = item.PartnerBankIBAN,
                    PartnerBankAccountNo = item.PartnerBankAccountNo,
                    PartnerBankBranch = item.PartnerBankBranch,
                    OwnedFloorNumbers = item.OwnedFloorNumbersList,
                    OwnedUnitIds = item.OwnedUnitsList,
                    UsufructStartDate = item.UsufructStartDate,
                    UsufructEndDate = item.UsufructEndDate,
                    UsufructAnnualFeePerYear = item.UsufructAnnualFeePerYear,
                    PartnershipStartDate = item.PartnershipStartDate,
                    PartnershipEndDate = item.PartnershipEndDate,
                    LandSharePercentWaqf = item.LandSharePercentWaqf,
                    LandTotalDunum = item.LandTotalDunum,
                    WaqfHarvestPercent = item.WaqfHarvestPercent,
                    FarmerName = item.FarmerName,
                    FarmerNationalId = item.FarmerNationalId,
                    HarvestContractType = item.HarvestContractType,
                    AgreementDate = item.AgreementDate,
                    AgreementNotaryName = item.AgreementNotaryName,
                    AgreementCourt = item.AgreementCourt,
                    AgreementReferenceNo = item.AgreementReferenceNo,
                    RevenueDistribMethod = item.RevenueDistribMethod,
                    RevenueDistribDay = item.RevenueDistribDay,
                    Notes = item.Notes
                },
                PropertyName = item.PropertyNameAr,
                PropertyWqfNumber = item.PropertyWqfNumber
            };

            return View("Edit", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdatePartnershipDto dto, IFormFile? agreementFile)
        {
            try
            {
                await _partnershipService.UpdateAsync(dto, agreementFile, GetUserId());
                TempData["Success"] = "تم تحديث الشراكة بنجاح";
                return RedirectToAction(nameof(Detail), new { id = dto.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Partnership update failed");
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("Edit", new PartnershipEditViewModel { Form = dto });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var detail = await _partnershipService.GetByIdAsync(id);
            if (detail == null)
            {
                return NotFound();
            }

            var vm = new PartnershipDetailViewModel
            {
                Partnership = detail,
                ContactHistory = await _partnershipService.GetContactHistoryAsync(id, 1, 20),
                Distributions = await _partnershipService.GetDistributionHistoryAsync(id)
            };

            return View("Detail", vm);
        }

        [HttpPost]
        public async Task<IActionResult> Deactivate(int id, string reason)
        {
            try
            {
                await _partnershipService.DeactivateAsync(id, reason, GetUserId());
                return Json(new { success = true, message = "تم تعليق الشراكة بنجاح" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> RecordRevenue(int partnershipId)
        {
            var detail = await _partnershipService.GetByIdAsync(partnershipId);
            if (detail == null)
            {
                return NotFound();
            }

            var vm = new PartnershipRecordRevenueViewModel
            {
                Partnership = detail,
                Form = new RevenueDistributionCreateDto
                {
                    PartnershipId = partnershipId,
                    PeriodLabel = DateTime.Today.ToString("Y", new System.Globalization.CultureInfo("ar-IQ")),
                    PeriodStartDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
                    PeriodEndDate = DateTime.Today
                }
            };

            return View("RecordRevenue", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordRevenue(RevenueDistributionCreateDto dto)
        {
            try
            {
                await _partnershipService.RecordDistributionAsync(dto, GetUserId());
                TempData["Success"] = "تم تسجيل التوزيع بنجاح";
                return RedirectToAction(nameof(Detail), new { id = dto.PartnershipId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("RecordRevenue", new PartnershipRecordRevenueViewModel { Form = dto });
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkTransferred(int distributionId, string method, string reference)
        {
            try
            {
                await _partnershipService.MarkTransferredAsync(distributionId, method, reference, GetUserId());
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(SendCommunicationDto dto)
        {
            try
            {
                var deliveryRef = await _partnershipService.SendCommunicationAsync(dto, GetUserId());
                return Json(new { success = true, deliveryRef, message = "تم الإرسال بنجاح" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, deliveryRef = string.Empty, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ContactHistory(int partnershipId, int page = 1)
        {
            var contactList = await _partnershipService.GetContactHistoryAsync(partnershipId, page, 20);
            return PartialView("_ContactHistory", contactList);
        }

        [HttpGet]
        public async Task<IActionResult> DistributionHistory(int partnershipId)
        {
            var data = await _partnershipService.GetDistributionHistoryAsync(partnershipId);
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> PreviewRevenue(int partnershipId, decimal totalRevenue)
        {
            try
            {
                var preview = await _partnershipService.PreviewRevenueCalculationAsync(partnershipId, totalRevenue);
                return Json(new
                {
                    waqfAmount = preview.WaqfAmount,
                    partnerAmount = preview.PartnerAmount,
                    detail = preview.CalculationDetail,
                    calculationMethod = preview.CalculationMethod
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GenerateStatement(int partnershipId, DateTime from, DateTime to)
        {
            var statement = await _partnershipService.GetStatementAsync(partnershipId, from, to);
            var content = $"كشف حساب الشريك: {statement.PartnerName}\nالعقار: {statement.PropertyNameAr}\nمن {from:yyyy/MM/dd} إلى {to:yyyy/MM/dd}\nإجمالي الإيراد: {statement.TotalRevenue:N0} د.ع\nحصة الوقف: {statement.TotalWaqfAmount:N0} د.ع\nحصة الشريك: {statement.TotalPartnerAmount:N0} د.ع";
            var bytes = Encoding.UTF8.GetBytes(content);
            return File(bytes, "application/pdf", $"statement_{partnershipId}_{DateTime.Now:yyyyMMdd}.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> GetPropertyFloors(int propertyId)
        {
            var property = await _propertyService.GetByIdAsync(propertyId);
            if (property == null)
            {
                return Json(Array.Empty<object>());
            }

            var result = property.Floors.Select(f => new
            {
                number = f.FloorNumber,
                label = string.IsNullOrWhiteSpace(f.FloorLabel) ? $"الطابق {f.FloorNumber}" : f.FloorLabel
            });
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetPropertyUnits(int propertyId)
        {
            var property = await _propertyService.GetByIdAsync(propertyId);
            if (property == null)
            {
                return Json(Array.Empty<object>());
            }

            var result = property.Floors.SelectMany(f => f.Units.Select(u => new
            {
                id = u.Id,
                number = u.UnitNumber,
                type = u.UnitType.ToString(),
                floor = f.FloorNumber
            }));

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetPartners(string term)
        {
            var data = await _partnershipService.SearchPartnersAsync(term);
            
            var results = data.Select(p => new { 
                    id = p.Name, // Using name as ID for now since Create contract form expects name string
                    text = p.Name,
                    partnerType = p.Type,
                    phone = p.Phone,
                    email = p.Email,
                    address = p.Address,
                    nationalId = p.NationalId
                })
                .ToList();

            return Json(results);
        }

        private int GetUserId()
        {
            var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(idValue, out var id) ? id : 0;
        }
    }
}
