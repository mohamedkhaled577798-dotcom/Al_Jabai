using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WaqfSystem.Application.DTOs.Common;
using WaqfSystem.Application.DTOs.Revenue;
using WaqfSystem.Application.Services;

namespace WaqfSystem.Web.Areas.MobileApi.Controllers
{
    [Area("MobileApi")]
    [Route("api/v1/mobile/collection")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class SmartCollectionMobileController : ControllerBase
    {
        private readonly ISmartCollectionService _smartCollectionService;
        private readonly IRevenueCollectionService _revenueCollectionService;
        private readonly IPropertyStructureService _propertyStructureService;

        public SmartCollectionMobileController(
            ISmartCollectionService smartCollectionService,
            IRevenueCollectionService revenueCollectionService,
            IPropertyStructureService propertyStructureService)
        {
            _smartCollectionService = smartCollectionService;
            _revenueCollectionService = revenueCollectionService;
            _propertyStructureService = propertyStructureService;
        }

        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        [HttpGet("suggestions")]
        public async Task<ActionResult<ApiResponse<System.Collections.Generic.List<SmartSuggestionDto>>>> GetSuggestions([FromQuery] string? period)
        {
            var list = await _smartCollectionService.GetSuggestionsAsync(CurrentUserId, period);
            return Ok(ApiResponse<System.Collections.Generic.List<SmartSuggestionDto>>.Ok(list));
        }

        [HttpGet("today")]
        public async Task<ActionResult<ApiResponse<TodayDashboardDto>>> Today()
        {
            var period = DateTime.Today.ToString("yyyy-MM");
            var dto = await _smartCollectionService.GetTodayDashboardAsync(CurrentUserId, period);
            return Ok(ApiResponse<TodayDashboardDto>.Ok(dto));
        }

        [HttpPost("quick")]
        public async Task<IActionResult> Quick([FromBody] QuickCollectDto dto)
        {
            try
            {
                var collision = await _revenueCollectionService.CheckCollisionAsync(dto.PropertyId, dto.CollectionLevel.ToString(), dto.FloorId, dto.UnitId, dto.PeriodLabel);
                if (collision.HasCollision)
                {
                    return StatusCode(409, ApiResponse<object>.Fail(collision.Message));
                }

                var revenue = await _revenueCollectionService.QuickCollectAsync(dto, CurrentUserId);
                return Ok(ApiResponse<object>.Ok(new { success = true, revenueCode = revenue.RevenueCode, message = "تم التسجيل" }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpPost("batch")]
        public async Task<ActionResult<ApiResponse<BatchCollectResultDto>>> Batch([FromBody] BatchCollectDto dto)
        {
            var result = await _smartCollectionService.BatchCollectAsync(dto, CurrentUserId);
            return Ok(ApiResponse<BatchCollectResultDto>.Ok(result));
        }

        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse<PagedResult<SearchResultDto>>>> Search([FromQuery] string q, [FromQuery] int page = 1)
        {
            var result = await _smartCollectionService.SearchAsync(q, CurrentUserId, page);
            return Ok(ApiResponse<PagedResult<SearchResultDto>>.Ok(result));
        }

        [HttpGet("check-collision")]
        public async Task<ActionResult<ApiResponse<object>>> CheckCollision(long propertyId, string level, long? floorId, long? unitId, string period)
        {
            var c = await _revenueCollectionService.CheckCollisionAsync(propertyId, level, floorId, unitId, period);
            return Ok(ApiResponse<object>.Ok(new { hasCollision = c.HasCollision, message = c.Message }));
        }

        [HttpGet("check-variance")]
        public async Task<ActionResult<ApiResponse<VarianceAlertDto>>> CheckVariance(long? contractId, decimal amount)
        {
            var result = await _revenueCollectionService.PreviewVarianceAsync(contractId, amount);
            return Ok(ApiResponse<VarianceAlertDto>.Ok(result));
        }

        [HttpGet("structure/{propertyId}")]
        public async Task<ActionResult<ApiResponse<PropertyStructureDto>>> Structure(long propertyId, [FromQuery] string period)
        {
            var dto = await _propertyStructureService.GetStructureAsync(propertyId, period, CurrentUserId);
            return Ok(ApiResponse<PropertyStructureDto>.Ok(dto));
        }
    }
}
