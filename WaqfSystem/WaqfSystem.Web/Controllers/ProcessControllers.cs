using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WaqfSystem.Application.DTOs.Property;
using WaqfSystem.Application.Services;
using WaqfSystem.Core.Enums;

namespace WaqfSystem.Web.Controllers
{
    [Authorize]
    public class DocumentController : BaseController
    {
        private readonly IDocumentService _documentService;

        public DocumentController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file, UploadDocumentDto dto)
        {
            if (file == null || file.Length == 0) return BadRequest("الملف غير موجود");

            try
            {
                await _documentService.UploadAsync(file, dto, CurrentUserId);
                SuccessMessage("تم رفع المستند بنجاح");
            }
            catch (Exception ex)
            {
                ErrorMessage(ex.Message);
            }

            return RedirectToAction("Details", "Property", new { id = dto.PropertyId });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, int propertyId)
        {
            await _documentService.DeleteAsync(id, CurrentUserId);
            SuccessMessage("تم حذف المستند");
            return RedirectToAction("Details", "Property", new { id = propertyId });
        }

        [HttpPost]
        public async Task<IActionResult> Verify(int id, int propertyId, VerificationMethod method, string notes)
        {
            await _documentService.VerifyAsync(id, CurrentUserId, method, notes);
            SuccessMessage("تم توثيق المستند");
            return RedirectToAction("Details", "Property", new { id = propertyId });
        }
    }

    [Authorize]
    public class WorkflowController : BaseController
    {
        private readonly IWorkflowService _workflowService;

        public WorkflowController(IWorkflowService workflowService)
        {
            _workflowService = workflowService;
        }

        [HttpPost]
        public async Task<IActionResult> Advance(WorkflowActionDto dto)
        {
            var success = await _workflowService.AdvanceStageAsync(dto, CurrentUserId);
            if (success)
                SuccessMessage("تم تحديث حالة العقار");
            else
                ErrorMessage("فشل في تحديث الحالة. يرجى مراجعة نقاط جودة البيانات");

            return RedirectToAction("Details", "Property", new { id = dto.PropertyId });
        }
    }
}
