using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WaqfSystem.Application.DTOs.Mobile;
using WaqfSystem.Application.DTOs.Common;
using WaqfSystem.Application.Services;
using System.Security.Claims;

namespace WaqfSystem.Web.Areas.MobileApi.Controllers
{
    [Area("MobileApi")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class SyncController : ControllerBase
    {
        private readonly IMobileSyncService _syncService;

        public SyncController(IMobileSyncService syncService)
        {
            _syncService = syncService;
        }

        [HttpGet("initial")]
        public async Task<ActionResult<ApiResponse<InitialSyncDto>>> GetInitialData()
        {
            var data = await _syncService.GetInitialSyncDataAsync();
            return Ok(ApiResponse<InitialSyncDto>.Ok(data));
        }

        [HttpPost("push")]
        public async Task<ActionResult<ApiResponse<SyncPushResponseDto>>> Push(SyncPushDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var response = await _syncService.PushSyncAsync(dto, userId);
            return Ok(ApiResponse<SyncPushResponseDto>.Ok(response));
        }

        [HttpGet("pull")]
        public async Task<ActionResult<ApiResponse<SyncPullResponseDto>>> Pull([FromQuery] DateTime lastSyncAt, [FromQuery] int? governorateId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var response = await _syncService.PullSyncAsync(lastSyncAt, userId, governorateId);
            return Ok(ApiResponse<SyncPullResponseDto>.Ok(response));
        }
    }
}
