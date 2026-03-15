using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WaqfSystem.Application.DTOs.Common;
using WaqfSystem.Application.DTOs.Partnership;
using WaqfSystem.Application.DTOs.Property;
using WaqfSystem.Application.Services;
using WaqfSystem.Core.Enums;

namespace WaqfSystem.Web.Areas.MobileApi.Controllers
{
    [Area("MobileApi")]
    [Route("api/mobile-specialized")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class MobileSpecializedController : ControllerBase
    {
        private readonly IPartnershipService _partnershipService;
        private readonly IAgriculturalService _agriculturalService;

        public MobileSpecializedController(IPartnershipService partnershipService, IAgriculturalService agriculturalService)
        {
            _partnershipService = partnershipService;
            _agriculturalService = agriculturalService;
        }

        [HttpPost("partnership")]
        public async Task<ActionResult<ApiResponse<long>>> AddPartnership(WaqfSystem.Application.DTOs.Property.CreatePartnershipDto dto)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var mapped = new WaqfSystem.Application.DTOs.Partnership.CreatePartnershipDto
            {
                PropertyId = dto.PropertyId,
                PartnershipType = PartnershipType.RevenuePercent,
                WaqfSharePercent = 100m - dto.PartnerSharePercent,
                PartnerName = dto.PartnerName,
                PartnerType = (WaqfSystem.Core.Enums.PartnerType)(int)dto.PartnerType,
                PartnerNationalId = dto.PartnerNationalId,
                PartnerPhone = dto.ContactPhone ?? string.Empty,
                PartnerEmail = dto.ContactEmail,
                PartnerBankIBAN = dto.PartnerBankIBAN,
                RevenueDistribMethod = (WaqfSystem.Core.Enums.RevenueDistribMethod)(int)dto.RevenueDistribMethod,
                AgreementDate = dto.AgreementDate,
                Notes = dto.Notes
            };

            var result = await _partnershipService.CreateAsync(mapped, null, userId);
            return Ok(ApiResponse<long>.Ok(result));
        }

        [HttpPost("agricultural")]
        public async Task<ActionResult<ApiResponse<AgriculturalDetailDto>>> UpdateAgricultural(CreateAgriculturalDto dto)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var result = await _agriculturalService.UpsertAsync(dto, userId);
            return Ok(ApiResponse<AgriculturalDetailDto>.Ok(result));
        }
    }

    [Area("MobileApi")]
    [Route("api/mobile-gis")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class MobileGisController : ControllerBase
    {
        private readonly IPropertyService _propertyService;

        public MobileGisController(IPropertyService propertyService)
        {
            _propertyService = propertyService;
        }

        [HttpGet("points")]
        public async Task<ActionResult<ApiResponse<List<PropertyMapPointDto>>>> GetPoints([FromQuery] int? governorateId)
        {
            var results = await _propertyService.GetMapPointsAsync(governorateId);
            return Ok(ApiResponse<List<PropertyMapPointDto>>.Ok(results));
        }

        [HttpPost("polygon")]
        public async Task<ActionResult<ApiResponse<bool>>> SavePolygon(int propertyId, [FromBody] string geoJsonPolygon)
        {
            // Placeholder for GIS polygon update
            return Ok(ApiResponse<bool>.Ok(true));
        }
    }
}
