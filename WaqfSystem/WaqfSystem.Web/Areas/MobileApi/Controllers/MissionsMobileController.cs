using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WaqfSystem.Application.DTOs.Common;
using WaqfSystem.Application.DTOs.Mission;
using WaqfSystem.Application.Services;
using WaqfSystem.Core.Enums;

namespace WaqfSystem.Web.Areas.MobileApi.Controllers
{
    [Area("MobileApi")]
    [Route("api/v1/mobile/missions")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class MissionsMobileController : ControllerBase
    {
        private readonly IMissionService _missionService;

        public MissionsMobileController(IMissionService missionService)
        {
            _missionService = missionService;
        }

        private int GetUserId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
        private string GetRole() => User.FindFirstValue(ClaimTypes.Role) ?? "FIELD_INSPECTOR";

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<MissionListItemDto>>>> Get([FromQuery] string? stage, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var filter = new MissionFilterRequest
            {
                Stage = Enum.TryParse<MissionStage>(stage, true, out var parsed) ? parsed : null,
                DateFrom = from,
                DateTo = to,
                Page = page,
                PageSize = pageSize
            };

            var data = await _missionService.GetPagedAsync(filter, GetUserId(), "FIELD_INSPECTOR");
            return Ok(ApiResponse<PagedResult<MissionListItemDto>>.Ok(data));
        }

        [HttpGet("today")]
        public async Task<ActionResult<ApiResponse<List<MissionDetailDto>>>> Today()
        {
            var data = await _missionService.GetPagedAsync(new MissionFilterRequest
            {
                DateFrom = DateTime.Today,
                DateTo = DateTime.Today,
                PageSize = 100
            }, GetUserId(), "FIELD_INSPECTOR");

            var details = new List<MissionDetailDto>();
            foreach (var item in data.Items)
            {
                var d = await _missionService.GetDetailAsync(item.Id, GetUserId(), "FIELD_INSPECTOR");
                if (d != null) details.Add(d);
            }

            return Ok(ApiResponse<List<MissionDetailDto>>.Ok(details));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<MissionDetailDto>>> GetById(int id)
        {
            var data = await _missionService.GetDetailAsync(id, GetUserId(), GetRole());
            if (data == null) return NotFound(ApiResponse<MissionDetailDto>.Fail("المهمة غير موجودة"));
            return Ok(ApiResponse<MissionDetailDto>.Ok(data));
        }

        [HttpPost("{id:int}/accept")]
        public async Task<ActionResult<ApiResponse<object>>> Accept(int id)
        {
            var ok = await _missionService.AdvanceStageAsync(new AdvanceStageDto { MissionId = id, ToStage = MissionStage.Accepted }, GetUserId());
            return Ok(ApiResponse<object>.Ok(new { success = ok, newStage = "Accepted", stageAr = "مقبولة" }));
        }

        [HttpPost("{id:int}/reject")]
        public async Task<ActionResult<ApiResponse<object>>> Reject(int id, [FromBody] RejectBody body)
        {
            var ok = await _missionService.AdvanceStageAsync(new AdvanceStageDto { MissionId = id, ToStage = MissionStage.Rejected, Notes = body.Reason }, GetUserId());
            return Ok(ApiResponse<object>.Ok(new { success = ok }));
        }

        [HttpPost("{id:int}/checkin")]
        public async Task<ActionResult<ApiResponse<object>>> Checkin(int id, [FromBody] CheckinBody body)
        {
            var ok = await _missionService.AdvanceStageAsync(new AdvanceStageDto { MissionId = id, ToStage = MissionStage.InProgress, CheckinLat = body.Lat, CheckinLng = body.Lng }, GetUserId());
            return Ok(ApiResponse<object>.Ok(new { success = ok, newStage = "InProgress", formattedTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm") }));
        }

        [HttpPost("{id:int}/advance-stage")]
        public async Task<ActionResult<ApiResponse<object>>> Advance(int id, [FromBody] AdvanceBody body)
        {
            var stage = Enum.Parse<MissionStage>(body.ToStage, true);
            var ok = await _missionService.AdvanceStageAsync(new AdvanceStageDto { MissionId = id, ToStage = stage, Notes = body.Notes }, GetUserId());
            var detail = await _missionService.GetDetailAsync(id, GetUserId(), GetRole());
            return Ok(ApiResponse<object>.Ok(new { success = ok, newStage = stage.ToString(), allowedNext = detail?.AllowedNextStages }));
        }

        [HttpPost("{id:int}/submit-review")]
        public async Task<ActionResult<ApiResponse<object>>> SubmitReview(int id, [FromBody] SubmitBody body)
        {
            var ok = await _missionService.AdvanceStageAsync(new AdvanceStageDto { MissionId = id, ToStage = MissionStage.SubmittedForReview, Notes = body.InspectorNotes }, GetUserId());
            return Ok(ApiResponse<object>.Ok(new { success = ok }));
        }

        [HttpPost("{id:int}/properties")]
        public async Task<ActionResult<ApiResponse<object>>> AddProperty(int id, [FromBody] PropertyBody body)
        {
            await _missionService.RecordPropertyEntryAsync(id, body.PropertyId, body.LocalId, GetUserId());
            var detail = await _missionService.GetDetailAsync(id, GetUserId(), "FIELD_INSPECTOR");
            return Ok(ApiResponse<object>.Ok(new { entryId = 0, missionProgressPercent = detail?.ProgressPercent ?? 0, newStage = detail?.Stage.ToString() }));
        }

        [HttpPatch("{id:int}/properties/{entryId:long}")]
        public async Task<ActionResult<ApiResponse<object>>> PatchProperty(int id, long entryId, [FromBody] PatchPropertyBody body)
        {
            if (body.EntryStatus?.Equals("Approved", StringComparison.OrdinalIgnoreCase) == true)
            {
                await _missionService.ApproveEntryAsync(entryId, GetUserId());
            }
            else if (body.EntryStatus?.Equals("Rejected", StringComparison.OrdinalIgnoreCase) == true)
            {
                await _missionService.RejectEntryAsync(entryId, "رفض من تطبيق الهاتف", GetUserId());
            }
            return Ok(ApiResponse<object>.Ok(new { success = true }));
        }

        [HttpGet("{id:int}/checklist")]
        public async Task<ActionResult<ApiResponse<object>>> Checklist(int id)
        {
            var detail = await _missionService.GetDetailAsync(id, GetUserId(), GetRole());
            return Ok(ApiResponse<object>.Ok(detail?.ChecklistTemplate));
        }

        [HttpPost("{id:int}/checklist")]
        public async Task<ActionResult<ApiResponse<object>>> SubmitChecklist(int id, [FromBody] ChecklistBody body)
        {
            var percent = body.Results.Count == 0 ? 0 : 100;
            return Ok(ApiResponse<object>.Ok(new { completionPercent = percent }));
        }

        [HttpGet("{id:int}/stage-history")]
        public async Task<ActionResult<ApiResponse<List<MissionStageHistoryDto>>>> StageHistory(int id)
        {
            var detail = await _missionService.GetDetailAsync(id, GetUserId(), GetRole());
            return Ok(ApiResponse<List<MissionStageHistoryDto>>.Ok(detail?.StageHistory ?? new List<MissionStageHistoryDto>()));
        }

        [HttpGet("{id:int}/properties")]
        public async Task<ActionResult<ApiResponse<List<MissionPropertyEntryDto>>>> Properties(int id)
        {
            var detail = await _missionService.GetDetailAsync(id, GetUserId(), GetRole());
            return Ok(ApiResponse<List<MissionPropertyEntryDto>>.Ok(detail?.PropertyEntries ?? new List<MissionPropertyEntryDto>()));
        }


        [HttpGet("/api/v1/mobile/inspector/stats")]
        public async Task<ActionResult<ApiResponse<object>>> Stats()
        {
            var data = await _missionService.GetDashboardStatsAsync(GetUserId(), "FIELD_INSPECTOR");
            return Ok(ApiResponse<object>.Ok(new
            {
                todayCompleted = data.CompletedThisMonth,
                monthCompleted = data.CompletedThisMonth,
                pendingMissions = data.ActiveMissions,
                avgDqs = data.AverageDqsScore,
                unsynced = 0
            }));
        }

        public class RejectBody { public string Reason { get; set; } = string.Empty; }
        public class CheckinBody { public decimal Lat { get; set; } public decimal Lng { get; set; } public DateTime CheckinAt { get; set; } }
        public class AdvanceBody { public string ToStage { get; set; } = string.Empty; public string? Notes { get; set; } }
        public class SubmitBody { public string InspectorNotes { get; set; } = string.Empty; public List<ChecklistItemResult>? ChecklistResults { get; set; } }
        public class PropertyBody { public long? PropertyId { get; set; } public string? LocalId { get; set; } }
        public class PatchPropertyBody { public decimal DqsScore { get; set; } public string? EntryStatus { get; set; } }
        public class ChecklistBody { public int TemplateId { get; set; } public List<ChecklistItemResult> Results { get; set; } = new(); }
    }
}
