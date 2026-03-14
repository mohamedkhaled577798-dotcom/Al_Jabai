using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WaqfSystem.Application.DTOs.Common;
using WaqfSystem.Application.DTOs.Partnership;
using WaqfSystem.Application.Services;

namespace WaqfSystem.Web.Areas.MobileApi.Controllers
{
    [Area("MobileApi")]
    [Route("api/v1/mobile/partnerships")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PartnershipMobileController : ControllerBase
    {
        private readonly IPartnershipService _partnershipService;

        public PartnershipMobileController(IPartnershipService partnershipService)
        {
            _partnershipService = partnershipService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<object>>> GetForProperty([FromQuery] int propertyId)
        {
            var list = await _partnershipService.GetByPropertyAsync(propertyId);
            return Ok(ApiResponse<object>.Ok(list));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<PartnershipDetailDto>>> GetById(int id)
        {
            var item = await _partnershipService.GetByIdAsync(id);
            if (item == null)
            {
                return NotFound(ApiResponse<PartnershipDetailDto>.Fail("الشراكة غير موجودة"));
            }

            return Ok(ApiResponse<PartnershipDetailDto>.Ok(item));
        }

        [HttpGet("{id:int}/revenue-preview")]
        public async Task<ActionResult<ApiResponse<RevenueCalculationResultDto>>> PreviewRevenue(int id, [FromQuery] decimal total)
        {
            var preview = await _partnershipService.PreviewRevenueCalculationAsync(id, total);
            return Ok(ApiResponse<RevenueCalculationResultDto>.Ok(preview));
        }

        [HttpPost("{id:int}/record-revenue")]
        public async Task<ActionResult<ApiResponse<RevenueDistributionDto>>> RecordRevenue(int id, [FromBody] RevenueDistributionCreateDto dto)
        {
            dto.PartnershipId = id;
            var result = await _partnershipService.RecordDistributionAsync(dto, 1);
            return Ok(ApiResponse<RevenueDistributionDto>.Ok(result));
        }

        [HttpGet("{id:int}/distributions")]
        public async Task<ActionResult<ApiResponse<object>>> GetDistributions(int id)
        {
            var data = await _partnershipService.GetDistributionHistoryAsync(id);
            return Ok(ApiResponse<object>.Ok(data));
        }

        [HttpPost("{id:int}/contact-log")]
        public async Task<ActionResult<ApiResponse<object>>> ContactLog(int id, [FromBody] MobileContactLogRequest request)
        {
            var result = await _partnershipService.SendCommunicationAsync(new SendCommunicationDto
            {
                PartnershipId = id,
                ContactType = request.ContactType,
                MessageBody = request.Notes,
                Subject = "تسجيل تواصل ميداني"
            }, 1);

            return Ok(ApiResponse<object>.Ok(new { logId = result }));
        }

        [HttpGet("expiring")]
        public async Task<ActionResult<ApiResponse<object>>> GetExpiring([FromQuery] int days = 90)
        {
            var data = await _partnershipService.GetExpiringAsync(days);
            return Ok(ApiResponse<object>.Ok(data));
        }
    }

    public class MobileContactLogRequest
    {
        public WaqfSystem.Core.Enums.ContactType ContactType { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
