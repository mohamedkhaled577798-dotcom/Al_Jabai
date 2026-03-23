using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WaqfSystem.Application.DTOs.Common;
using WaqfSystem.Application.DTOs.Document;
using WaqfSystem.Application.Services;

namespace WaqfSystem.Web.Areas.MobileApi.Controllers
{
    [Area("MobileApi")]
    [Route("api/v1/mobile/documents")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class DocumentsMobileController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly IDocumentAlertService _documentAlertService;

        public DocumentsMobileController(IDocumentService documentService, IDocumentAlertService documentAlertService)
        {
            _documentService = documentService;
            _documentAlertService = documentAlertService;
        }

        private int UserId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

        [HttpGet("{propertyId:long}")]
        public async Task<ActionResult<ApiResponse<object>>> GetByProperty(long propertyId)
        {
            var summary = await _documentService.GetPropertySummaryAsync(propertyId);
            var docs = await _documentService.GetByPropertyAsync(propertyId, new DocumentFilterRequest { PropertyId = propertyId, Page = 1, PageSize = 200 });
            return Ok(ApiResponse<object>.Ok(new { summary, documents = docs.Items }));
        }

        [HttpGet("{propertyId:long}/detail")]
        public async Task<ActionResult<ApiResponse<object>>> GetByPropertyDetail(long propertyId)
        {
            var docs = await _documentService.GetByPropertyAsync(propertyId, new DocumentFilterRequest { PropertyId = propertyId, Page = 1, PageSize = 500 });
            var detailList = new List<PropertyDocumentDetailDto>();
            foreach (var item in docs.Items)
            {
                var detail = await _documentService.GetDetailAsync(item.Id, UserId);
                if (detail != null)
                {
                    detailList.Add(detail);
                }
            }

            return Ok(ApiResponse<object>.Ok(new { documents = detailList }));
        }

        [HttpGet("detail/{id:long}")]
        public async Task<ActionResult<ApiResponse<PropertyDocumentDetailDto>>> GetDetail(long id)
        {
            var detail = await _documentService.GetDetailAsync(id, UserId);
            if (detail == null)
            {
                return NotFound(ApiResponse<PropertyDocumentDetailDto>.Fail("الوثيقة غير موجودة"));
            }

            return Ok(ApiResponse<PropertyDocumentDetailDto>.Ok(detail));
        }

        [HttpPost("upload")]
        [RequestSizeLimit(50_000_000)]
        public async Task<ActionResult<ApiResponse<object>>> Upload([FromForm] UploadDocumentDto dto)
        {
            var id = await _documentService.UploadAsync(dto, UserId);
            var detail = await _documentService.GetDetailAsync(id, UserId);
            return Ok(ApiResponse<object>.Ok(new { documentId = id, status = detail?.Status.ToString(), title = detail?.Title }));
        }

        [HttpPost("{id:long}/upload-version")]
        [RequestSizeLimit(50_000_000)]
        public async Task<ActionResult<ApiResponse<object>>> UploadVersion(long id, [FromForm] IFormFile file, [FromForm] string? notes, [FromForm] DateTime? expiryDate)
        {
            var dto = new UploadNewVersionDto
            {
                DocumentId = id,
                File = file,
                Notes = notes,
                UpdateExpiryDate = expiryDate
            };

            var version = await _documentService.UploadNewVersionAsync(dto, UserId);
            return Ok(ApiResponse<object>.Ok(new { versionId = version.Id, versionNumber = version.VersionNumber }));
        }

        [HttpGet("{id:long}/versions")]
        public async Task<ActionResult<ApiResponse<List<DocumentVersionDto>>>> GetVersions(long id)
        {
            var detail = await _documentService.GetDetailAsync(id, UserId);
            return Ok(ApiResponse<List<DocumentVersionDto>>.Ok(detail?.Versions ?? new List<DocumentVersionDto>()));
        }

        [HttpGet("{id:long}/audit-trail")]
        public async Task<ActionResult<ApiResponse<List<DocumentAuditTrailDto>>>> GetAuditTrail(long id)
        {
            var trail = await _documentService.GetAuditTrailAsync(id, 20);
            return Ok(ApiResponse<List<DocumentAuditTrailDto>>.Ok(trail));
        }

        [HttpPost("{id:long}/record-download")]
        public async Task<ActionResult<ApiResponse<object>>> RecordDownload(long id, [FromQuery] long versionId)
        {
            await _documentService.RecordDownloadAsync(id, versionId, UserId);
            return Ok(ApiResponse<object>.Ok(new { success = true }));
        }

        [HttpGet("alerts")]
        public async Task<ActionResult<ApiResponse<object>>> GetAlerts()
        {
            var alerts = await _documentAlertService.GetUnreadAlertsForUserAsync(UserId);
            return Ok(ApiResponse<object>.Ok(new
            {
                expired = alerts.Where(x => x.AlertLevel == Core.Entities.DocumentAlertLevel.Expired).ToList(),
                expiring30 = alerts.Where(x => x.AlertLevel == Core.Entities.DocumentAlertLevel.Day30).ToList(),
                expiring90 = alerts.Where(x => x.AlertLevel == Core.Entities.DocumentAlertLevel.Day90).ToList()
            }));
        }

        [HttpGet("alert-count")]
        public async Task<ActionResult<ApiResponse<object>>> GetAlertCount()
        {
            var count = await _documentAlertService.GetUnreadAlertCountAsync(UserId);
            return Ok(ApiResponse<object>.Ok(new { count }));
        }

        [HttpPost("alerts/{alertId:long}/read")]
        public async Task<ActionResult<ApiResponse<object>>> MarkRead(long alertId)
        {
            await _documentService.MarkAlertReadAsync(alertId, UserId);
            return Ok(ApiResponse<object>.Ok(new { success = true }));
        }

        [HttpGet("types")]
        public async Task<ActionResult<ApiResponse<List<DocumentTypeDto>>>> GetTypes()
        {
            var types = await _documentService.GetDocumentTypesAsync();
            return Ok(ApiResponse<List<DocumentTypeDto>>.Ok(types));
        }
    }
}
