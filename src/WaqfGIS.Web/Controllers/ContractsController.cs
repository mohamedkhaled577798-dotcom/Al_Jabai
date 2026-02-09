using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WaqfGIS.Core.Entities;
using WaqfGIS.Core.Interfaces;
using WaqfGIS.Services;
using WaqfGIS.Web.Models;

namespace WaqfGIS.Web.Controllers;

[Authorize]
public class ContractsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ContractService _contractService;
    private readonly ILogger<ContractsController> _logger;

    public ContractsController(
        IUnitOfWork unitOfWork,
        ContractService contractService,
        ILogger<ContractsController> logger)
    {
        _unitOfWork = unitOfWork;
        _contractService = contractService;
        _logger = logger;
    }

    // GET: Contracts
    public async Task<IActionResult> Index(string? status, string? entityType, int page = 1)
    {
        try
        {
            var pageSize = 20;
            var contracts = await _unitOfWork.Repository<InvestmentContract>().GetAllAsync();

            // تطبيق الفلاتر
            if (!string.IsNullOrEmpty(status))
            {
                contracts = contracts.Where(c => c.Status == status).ToList();
            }

            if (!string.IsNullOrEmpty(entityType))
            {
                contracts = contracts.Where(c => c.EntityType == entityType).ToList();
            }

            var totalCount = contracts.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var pagedContracts = contracts
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Status = status;
            ViewBag.EntityType = entityType;

            return View(pagedContracts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading contracts");
            TempData["Error"] = "حدث خطأ أثناء تحميل العقود";
            return View(new List<InvestmentContract>());
        }
    }

    // GET: Contracts/Details/5
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var contract = await _unitOfWork.Repository<InvestmentContract>().GetByIdAsync(id);
            if (contract == null)
            {
                TempData["Error"] = "العقد غير موجود";
                return RedirectToAction(nameof(Index));
            }

            // جلب المستندات والدفعات
            var documents = await _unitOfWork.Repository<ContractDocument>()
                .FindAsync(d => d.ContractId == id);
            var payments = await _unitOfWork.Repository<ContractPayment>()
                .FindAsync(p => p.ContractId == id);

            ViewBag.Documents = documents.ToList();
            ViewBag.Payments = payments.OrderBy(p => p.DueDate).ToList();

            return View(contract);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading contract details for ID: {ContractId}", id);
            TempData["Error"] = "حدث خطأ أثناء تحميل تفاصيل العقد";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Contracts/Notifications - صفحة التنبيهات
    public async Task<IActionResult> Notifications()
    {
        try
        {
            var notifications = await _contractService.GetContractNotificationsAsync();
            return View(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading contract notifications");
            TempData["Error"] = "حدث خطأ أثناء تحميل التنبيهات";
            return View(new Dictionary<string, List<InvestmentContract>>());
        }
    }

    // POST: Contracts/Renew/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Renew(int id, DateTime newEndDate)
    {
        try
        {
            var contract = await _unitOfWork.Repository<InvestmentContract>().GetByIdAsync(id);
            if (contract == null)
            {
                return Json(new { success = false, message = "العقد غير موجود" });
            }

            if (!contract.IsRenewable)
            {
                return Json(new { success = false, message = "هذا العقد غير قابل للتجديد" });
            }

            contract.EndDate = newEndDate;
            var durationMonths = (newEndDate.Year - contract.StartDate.Year) * 12 + 
                                newEndDate.Month - contract.StartDate.Month;
            contract.DurationMonths = durationMonths;
            contract.DurationYears = durationMonths / 12;
            contract.TotalContractValue = contract.MonthlyRent * durationMonths;
            contract.Status = "نشط";
            contract.IsActive = true;
            contract.UpdatedBy = User.Identity?.Name ?? "System";

            await _unitOfWork.Repository<InvestmentContract>().UpdateAsync(contract);
            await _unitOfWork.SaveChangesAsync();

            return Json(new { success = true, message = "تم تجديد العقد بنجاح" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error renewing contract, ID: {ContractId}", id);
            return Json(new { success = false, message = "حدث خطأ أثناء تجديد العقد" });
        }
    }

    // POST: Contracts/Terminate/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Terminate(int id, string reason)
    {
        try
        {
            var contract = await _unitOfWork.Repository<InvestmentContract>().GetByIdAsync(id);
            if (contract == null)
            {
                return Json(new { success = false, message = "العقد غير موجود" });
            }

            contract.Status = "منتهي";
            contract.IsActive = false;
            contract.TerminationDate = DateTime.Now;
            contract.TerminationReason = reason;
            contract.UpdatedBy = User.Identity?.Name ?? "System";

            await _unitOfWork.Repository<InvestmentContract>().UpdateAsync(contract);
            await _unitOfWork.SaveChangesAsync();

            return Json(new { success = true, message = "تم إنهاء العقد بنجاح" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error terminating contract, ID: {ContractId}", id);
            return Json(new { success = false, message = "حدث خطأ أثناء إنهاء العقد" });
        }
    }

    private async Task LoadSelectListsAsync()
    {
        ViewBag.ContractTypes = new SelectList(new[]
        {
            "إيجار", "استثمار", "تطوير", "شراكة"
        });

        ViewBag.InvestorTypes = new SelectList(new[]
        {
            "فرد", "شركة", "مؤسسة"
        });

        ViewBag.PaymentMethods = new SelectList(new[]
        {
            "شهري", "ربع سنوي", "نصف سنوي", "سنوي"
        });

        ViewBag.StatusList = new SelectList(new[]
        {
            "نشط", "منتهي", "ملغي", "متوقف", "قيد التجديد"
        });
    }
}

    // GET: Contracts/Create
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadSelectListsAsync();
        return View(new ContractViewModel());
    }

    // POST: Contracts/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ContractViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadSelectListsAsync();
            return View(model);
        }

        try
        {
            var contract = new InvestmentContract
            {
                ContractNumber = model.ContractNumber,
                ContractDate = model.ContractDate,
                ContractType = model.ContractType,
                ContractPurpose = model.ContractPurpose,
                EntityType = model.EntityType,
                EntityId = model.EntityId,
                
                InvestorName = model.InvestorName,
                InvestorType = model.InvestorType,
                InvestorIdNumber = model.InvestorIdNumber,
                InvestorPhone = model.InvestorPhone,
                InvestorMobile = model.InvestorMobile,
                InvestorEmail = model.InvestorEmail,
                InvestorAddress = model.InvestorAddress,
                
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                MonthlyRent = model.MonthlyRent,
                Currency = model.Currency,
                PaymentMethod = model.PaymentMethod,
                SecurityDeposit = model.SecurityDeposit,
                HasBankGuarantee = model.HasBankGuarantee,
                
                IsRenewable = model.IsRenewable,
                RenewalNoticeDays = model.RenewalNoticeDays,
                HasAnnualIncrease = model.HasAnnualIncrease,
                AnnualIncreasePercentage = model.AnnualIncreasePercentage,
                AnnualIncreaseAmount = model.AnnualIncreaseAmount,
                
                SpecialTerms = model.SpecialTerms,
                Notes = model.Notes,
                
                IsActive = true,
                Status = "نشط",
                CreatedBy = User.Identity?.Name ?? "System"
            };

            // حساب القيم
            contract.DurationMonths = (int)((contract.EndDate - contract.StartDate).TotalDays / 30);
            contract.DurationYears = contract.DurationMonths / 12;
            contract.AnnualRent = contract.MonthlyRent * 12;
            contract.TotalContractValue = contract.MonthlyRent * contract.DurationMonths;

            await _unitOfWork.Repository<InvestmentContract>().AddAsync(contract);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = "تم إضافة العقد بنجاح";
            return RedirectToAction(nameof(Details), new { id = contract.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating contract");
            ModelState.AddModelError("", "حدث خطأ أثناء إضافة العقد");
            await LoadSelectListsAsync();
            return View(model);
        }
    }

    // API: Get entities by type
    [HttpGet]
    public async Task<IActionResult> GetEntitiesByType(string entityType)
    {
        try
        {
            var entities = new List<object>();

            switch (entityType)
            {
                case "Mosque":
                    var mosques = await _unitOfWork.Repository<Mosque>().GetAllAsync();
                    entities = mosques.Select(m => new { id = m.Id, name = m.NameAr }).ToList<object>();
                    break;

                case "WaqfProperty":
                    var properties = await _unitOfWork.Repository<WaqfProperty>().GetAllAsync();
                    entities = properties.Select(p => new { id = p.Id, name = p.NameAr }).ToList<object>();
                    break;

                case "WaqfLand":
                    var lands = await _unitOfWork.Repository<WaqfLand>().GetAllAsync();
                    entities = lands.Select(l => new { id = l.Id, name = l.NameAr }).ToList<object>();
                    break;
            }

            return Json(entities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting entities by type: {EntityType}", entityType);
            return Json(new List<object>());
        }
    }
