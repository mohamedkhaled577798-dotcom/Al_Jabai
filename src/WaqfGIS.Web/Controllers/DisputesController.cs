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
            _unitOfWork.Repository<LegalDispute>().Update(dispute);
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

            _unitOfWork.Repository<LegalDispute>().Update(dispute);
            await _unitOfWork.SaveChangesAsync();

            return Json(new { success = true, message = "تم إضافة الحكم بنجاح" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding verdict, ID: {DisputeId}", id);
            return Json(new { success = false, message = "حدث خطأ أثناء إضافة الحكم" });
        }
    }
}
