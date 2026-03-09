using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WaqfGIS.Core.Entities;
using WaqfGIS.Core.Interfaces;
using WaqfGIS.Web.Models;

namespace WaqfGIS.Web.Controllers;

[Authorize]
public class DisputesController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DisputesController> _logger;

    public DisputesController(
        IUnitOfWork unitOfWork,
        ILogger<DisputesController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    // GET: Disputes
    public async Task<IActionResult> Index(string? status, string? disputeType, int page = 1)
    {
        try
        {
            var pageSize = 20;
            var disputes = await _unitOfWork.Repository<LegalDispute>().GetAllAsync();

            // تطبيق الفلاتر
            if (!string.IsNullOrEmpty(status))
            {
                disputes = disputes.Where(d => d.CaseStatus == status).ToList();
            }

            if (!string.IsNullOrEmpty(disputeType))
            {
                disputes = disputes.Where(d => d.DisputeType == disputeType).ToList();
            }

            var totalCount = disputes.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var pagedDisputes = disputes
                .OrderByDescending(d => d.CaseDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Status = status;
            ViewBag.DisputeType = disputeType;

            return View(pagedDisputes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading disputes");
            TempData["Error"] = "حدث خطأ أثناء تحميل الدعاوى";
            return View(new List<LegalDispute>());
        }
    }

    // GET: Disputes/Details/5
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var dispute = await _unitOfWork.Repository<LegalDispute>().GetByIdAsync(id);
            if (dispute == null)
            {
                TempData["Error"] = "الدعوى غير موجودة";
                return RedirectToAction(nameof(Index));
            }

            // جلب المستندات
            var documents = await _unitOfWork.Repository<DisputeDocument>()
                .FindAsync(d => d.DisputeId == id);
            ViewBag.Documents = documents.ToList();

            return View(dispute);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dispute details for ID: {DisputeId}", id);
            TempData["Error"] = "حدث خطأ أثناء تحميل تفاصيل الدعوى";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Disputes/UpdateStage
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStage(int id, string stage, DateTime? hearingDate, string? notes)
    {
        try
        {
            var dispute = await _unitOfWork.Repository<LegalDispute>().GetByIdAsync(id);
            if (dispute == null)
            {
                return Json(new { success = false, message = "الدعوى غير موجودة" });
            }

            dispute.CurrentStage = stage;
            if (hearingDate.HasValue)
            {
                dispute.LastHearingDate = DateTime.Now;
                dispute.NextHearingDate = hearingDate.Value;
            }
            
            if (!string.IsNullOrEmpty(notes))
            {
                dispute.LastProcedure = notes;
            }

            dispute.UpdatedBy = User.Identity?.Name ?? "System";
            await _unitOfWork.Repository<LegalDispute>().UpdateAsync(dispute);
            await _unitOfWork.SaveChangesAsync();

            return Json(new { success = true, message = "تم تحديث مرحلة الدعوى بنجاح" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating dispute stage, ID: {DisputeId}", id);
            return Json(new { success = false, message = "حدث خطأ أثناء تحديث المرحلة" });
        }
    }

    // POST: Disputes/AddVerdict
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddVerdict(int id, DateTime verdictDate, string verdictSummary, string verdictResult)
    {
        try
        {
            var dispute = await _unitOfWork.Repository<LegalDispute>().GetByIdAsync(id);
            if (dispute == null)
            {
                return Json(new { success = false, message = "الدعوى غير موجودة" });
            }

            dispute.HasVerdict = true;
            dispute.VerdictDate = verdictDate;
            dispute.VerdictSummary = verdictSummary;
            dispute.VerdictResult = verdictResult;
            dispute.CaseStatus = "منتهية";
            dispute.UpdatedBy = User.Identity?.Name ?? "System";

            await _unitOfWork.Repository<LegalDispute>().UpdateAsync(dispute);
            await _unitOfWork.SaveChangesAsync();

            return Json(new { success = true, message = "تم إضافة الحكم بنجاح" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding verdict, ID: {DisputeId}", id);
            return Json(new { success = false, message = "حدث خطأ أثناء إضافة الحكم" });
        }
    }

    // GET: Disputes/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var dispute = await _unitOfWork.Repository<LegalDispute>().GetByIdAsync(id);
        if (dispute == null) { TempData["Error"] = "الدعوى غير موجودة"; return RedirectToAction(nameof(Index)); }
        var model = new DisputeViewModel {
            Id = dispute.Id,
            CaseNumber = dispute.CaseNumber, CaseDate = dispute.CaseDate,
            CourtName = dispute.CourtName, CourtType = dispute.CourtType,
            EntityType = dispute.EntityType, EntityId = dispute.EntityId, EntityName = dispute.EntityName,
            DisputeType = dispute.DisputeType, DisputeSubject = dispute.DisputeSubject,
            DisputeDescription = dispute.DisputeDescription, ClaimAmount = dispute.ClaimAmount,
            PlaintiffName = dispute.PlaintiffName, PlaintiffPhone = dispute.PlaintiffPhone, PlaintiffAddress = dispute.PlaintiffAddress,
            DefendantName = dispute.DefendantName, DefendantPhone = dispute.DefendantPhone, DefendantAddress = dispute.DefendantAddress,
            LawyerName = dispute.LawyerName, LawyerPhone = dispute.LawyerPhone, LawyerLicenseNumber = dispute.LawyerLicenseNumber, LegalCosts = dispute.LegalCosts,
            CaseStatus = dispute.CaseStatus, CurrentStage = dispute.CurrentStage, NextHearingDate = dispute.NextHearingDate
        };
        return View(model);
    }

    // POST: Disputes/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DisputeViewModel model)
    {
        if (id != model.Id) return NotFound();
        if (!ModelState.IsValid) return View(model);
        try
        {
            var dispute = await _unitOfWork.Repository<LegalDispute>().GetByIdAsync(id);
            if (dispute == null) return NotFound();
            dispute.CaseNumber = model.CaseNumber; dispute.CaseDate = model.CaseDate;
            dispute.CourtName = model.CourtName; dispute.CourtType = model.CourtType;
            dispute.EntityType = model.EntityType; dispute.EntityId = model.EntityId; dispute.EntityName = model.EntityName;
            dispute.DisputeType = model.DisputeType; dispute.DisputeSubject = model.DisputeSubject;
            dispute.DisputeDescription = model.DisputeDescription; dispute.ClaimAmount = model.ClaimAmount;
            dispute.PlaintiffName = model.PlaintiffName; dispute.PlaintiffPhone = model.PlaintiffPhone; dispute.PlaintiffAddress = model.PlaintiffAddress;
            dispute.DefendantName = model.DefendantName; dispute.DefendantPhone = model.DefendantPhone; dispute.DefendantAddress = model.DefendantAddress;
            dispute.LawyerName = model.LawyerName; dispute.LawyerPhone = model.LawyerPhone; dispute.LawyerLicenseNumber = model.LawyerLicenseNumber; dispute.LegalCosts = model.LegalCosts;
            dispute.CaseStatus = model.CaseStatus; dispute.CurrentStage = model.CurrentStage; dispute.NextHearingDate = model.NextHearingDate;
            dispute.UpdatedBy = User.Identity?.Name ?? "System";
            await _unitOfWork.Repository<LegalDispute>().UpdateAsync(dispute);
            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = "تم تحديث بيانات الدعوى";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error editing dispute ID: {Id}", id);
            ModelState.AddModelError("", "حدث خطأ أثناء الحفظ");
            return View(model);
        }
    }

    // GET: Disputes/Create
    [HttpGet]
    public IActionResult Create()
    {
        return View(new DisputeViewModel { CaseDate = DateTime.Now, CaseStatus = "جارية", CurrentStage = "ابتدائية" });
    }

    // POST: Disputes/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DisputeViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var dispute = new LegalDispute
            {
                CaseNumber = model.CaseNumber,
                CaseDate = model.CaseDate,
                CourtName = model.CourtName,
                CourtType = model.CourtType,
                EntityType = model.EntityType,
                EntityId = model.EntityId,
                
                DisputeType = model.DisputeType,
                DisputeSubject = model.DisputeSubject,
                DisputeDescription = model.DisputeDescription,
                ClaimAmount = model.ClaimAmount,
                
                PlaintiffName = model.PlaintiffName,
                PlaintiffPhone = model.PlaintiffPhone,
                PlaintiffAddress = model.PlaintiffAddress,
                
                DefendantName = model.DefendantName,
                DefendantPhone = model.DefendantPhone,
                DefendantAddress = model.DefendantAddress,
                
                LawyerName = model.LawyerName,
                LawyerPhone = model.LawyerPhone,
                LawyerLicenseNumber = model.LawyerLicenseNumber,
                LegalCosts = model.LegalCosts,
                
                CaseStatus = model.CaseStatus,
                CurrentStage = model.CurrentStage,
                NextHearingDate = model.NextHearingDate,
                
                HasVerdict = false,
                CreatedBy = User.Identity?.Name ?? "System"
            };

            await _unitOfWork.Repository<LegalDispute>().AddAsync(dispute);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = "تم إضافة الدعوى بنجاح";
            return RedirectToAction(nameof(Details), new { id = dispute.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating dispute");
            ModelState.AddModelError("", "حدث خطأ أثناء إضافة الدعوى");
            return View(model);
        }
    }
}
