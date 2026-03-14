using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WaqfSystem.Application.DTOs.Common;
using WaqfSystem.Application.DTOs.Property;
using WaqfSystem.Application.Services;
using WaqfSystem.Core.Interfaces;
using WaqfSystem.Core.Entities;

namespace WaqfSystem.Web.Areas.MobileApi.Controllers
{
    [Area("MobileApi")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class MobileComponentController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork; // Simplified access for unit/floor CRUD

        public MobileComponentController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpPost("floor")]
        public async Task<ActionResult<ApiResponse<int>>> AddFloor(CreateFloorDto dto)
        {
            if (dto.PropertyId <= 0)
            {
                return BadRequest(ApiResponse<int>.Fail("رقم العقار غير صالح"));
            }

            var propertyExists = await _unitOfWork.GetQueryable<Property>()
                .AnyAsync(p => p.Id == dto.PropertyId);

            if (!propertyExists)
            {
                return NotFound(ApiResponse<int>.Fail("العقار غير موجود"));
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId <= 0)
            {
                return Unauthorized(ApiResponse<int>.Fail("بيانات المستخدم غير صالحة"));
            }

            var floor = new PropertyFloor
            {
                PropertyId = dto.PropertyId,
                FloorNumber = dto.FloorNumber,
                FloorLabel = dto.FloorLabel,
                FloorUsage = dto.FloorUsage,
                TotalAreaSqm = dto.TotalAreaSqm,
                UsableAreaSqm = dto.UsableAreaSqm,
                CeilingHeightCm = dto.CeilingHeightCm,
                StructuralCondition = dto.StructuralCondition,
                HasBalcony = dto.HasBalcony,
                Notes = dto.Notes,
                CreatedById = userId
            };

            await _unitOfWork.AddAsync(floor);
            await _unitOfWork.SaveChangesAsync();

            return Ok(ApiResponse<int>.Ok(floor.Id, "تمت إضافة الطابق بنجاح"));
        }

        [HttpPost("unit")]
        public async Task<ActionResult<ApiResponse<int>>> AddUnit(CreateUnitDto dto)
        {
            if (dto.PropertyId <= 0 || dto.FloorId <= 0)
            {
                return BadRequest(ApiResponse<int>.Fail("بيانات العقار أو الطابق غير صالحة"));
            }

            var floorExists = await _unitOfWork.GetQueryable<PropertyFloor>()
                .AnyAsync(f => f.Id == dto.FloorId && f.PropertyId == dto.PropertyId);

            if (!floorExists)
            {
                return NotFound(ApiResponse<int>.Fail("الطابق غير موجود أو لا يتبع العقار"));
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId <= 0)
            {
                return Unauthorized(ApiResponse<int>.Fail("بيانات المستخدم غير صالحة"));
            }

            var unit = new PropertyUnit
            {
                FloorId = dto.FloorId,
                PropertyId = dto.PropertyId,
                UnitNumber = dto.UnitNumber,
                UnitType = dto.UnitType,
                AreaSqm = dto.AreaSqm,
                BedroomCount = dto.BedroomCount,
                BathroomCount = dto.BathroomCount,
                OccupancyStatus = dto.OccupancyStatus,
                MarketRentMonthly = dto.MarketRentMonthly,
                ElectricMeterNo = dto.ElectricMeterNo,
                WaterMeterNo = dto.WaterMeterNo,
                HasAC = dto.HasAC,
                HasKitchen = dto.HasKitchen,
                Furnished = dto.Furnished,
                Condition = dto.Condition,
                Notes = dto.Notes,
                CreatedById = userId
            };

            await _unitOfWork.AddAsync(unit);
            await _unitOfWork.SaveChangesAsync();

            return Ok(ApiResponse<int>.Ok(unit.Id, "تمت إضافة الوحدة بنجاح"));
        }
    }

    [Area("MobileApi")]
    [Route("api/mobile-missions")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class MobileMissionController : ControllerBase
    {
        private readonly IMissionService _missionService;

        public MobileMissionController(IMissionService missionService)
        {
            _missionService = missionService;
        }

        [HttpGet("my")]
        public async Task<ActionResult<ApiResponse<List<InspectionMission>>>> GetMissions()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var results = await _missionService.GetMyMissionsAsync(userId);
            return Ok(ApiResponse<List<InspectionMission>>.Ok(results));
        }

        [HttpPost("{id}/start")]
        public async Task<ActionResult<ApiResponse<bool>>> Start(int id, [FromQuery] decimal lat, [FromQuery] decimal lng)
        {
            var success = await _missionService.StartMissionAsync(id, lat, lng);
            return Ok(ApiResponse<bool>.Ok(success));
        }

        [HttpPost("{id}/complete")]
        public async Task<ActionResult<ApiResponse<bool>>> Complete(int id, [FromBody] string notes)
        {
            var success = await _missionService.CompleteMissionAsync(id, notes);
            return Ok(ApiResponse<bool>.Ok(success));
        }
    }
}
