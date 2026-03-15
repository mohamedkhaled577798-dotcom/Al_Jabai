using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WaqfSystem.Application.DTOs.Common;
using WaqfSystem.Application.DTOs.Property;
using WaqfSystem.Application.Services;

namespace WaqfSystem.Web.Areas.MobileApi.Controllers
{
    [Area("MobileApi")]
    [Route("api/mobile-properties")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class MobilePropertyController : ControllerBase
    {
        private readonly IPropertyService _propertyService;

        public MobilePropertyController(IPropertyService propertyService)
        {
            _propertyService = propertyService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<PropertyListDto>>>> GetList([FromQuery] PropertyFilterRequest filter)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            
            var results = await _propertyService.GetPagedAsync(filter, userId, role);
            return Ok(ApiResponse<PagedResult<PropertyListDto>>.Ok(results));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<PropertyDetailDto>>> GetDetails(int id)
        {
            var result = await _propertyService.GetByIdAsync(id);
            if (result == null) return NotFound(ApiResponse<PropertyDetailDto>.Fail("العقار غير موجود"));
            return Ok(ApiResponse<PropertyDetailDto>.Ok(result));
        }

        [HttpGet("nearby")]
        public async Task<ActionResult<ApiResponse<List<PropertyListDto>>>> GetNearby(decimal lat, decimal lng, double radius = 1000, int limit = 10)
        {
            var results = await _propertyService.GetNearbyAsync(lat, lng, radius, limit);
            return Ok(ApiResponse<List<PropertyListDto>>.Ok(results));
        }
    }
}
