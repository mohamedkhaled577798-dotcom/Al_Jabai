using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WaqfSystem.Application.DTOs.Common;
using WaqfSystem.Application.Services;
using WaqfSystem.Core.Entities;

namespace WaqfSystem.Web.Areas.MobileApi.Controllers
{
    [Area("MobileApi")]
    [Route("api/v1/mobile/contracts")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ContractsMobileController : ControllerBase
    {
        private readonly IRentContractService _rentContractService;

        public ContractsMobileController(IRentContractService rentContractService)
        {
            _rentContractService = rentContractService;
        }

        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        [HttpGet("{unitId}")]
        public async Task<ActionResult<ApiResponse<RentContract?>>> ActiveContract(long unitId)
        {
            var contract = await _rentContractService.GetActiveByUnitIdAsync(unitId, CurrentUserId);
            return Ok(ApiResponse<RentContract?>.Ok(contract));
        }

        [HttpGet("expiring")]
        public async Task<ActionResult<ApiResponse<List<RentContract>>>> Expiring([FromQuery] int days = 60)
        {
            var list = await _rentContractService.GetExpiringAsync(days, CurrentUserId);
            return Ok(ApiResponse<List<RentContract>>.Ok(list));
        }

        [HttpGet("schedule/{id}")]
        public async Task<ActionResult<ApiResponse<List<RentPaymentSchedule>>>> Schedule(long id)
        {
            var list = await _rentContractService.GetScheduleAsync(id, CurrentUserId);
            return Ok(ApiResponse<List<RentPaymentSchedule>>.Ok(list));
        }
    }
}
