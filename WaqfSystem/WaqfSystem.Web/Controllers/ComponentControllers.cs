using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WaqfSystem.Application.DTOs.Property;
using WaqfSystem.Core.Entities;
using WaqfSystem.Core.Interfaces;
using WaqfSystem.Core.Enums;
using WaqfSystem.Web.ViewModels;

namespace WaqfSystem.Web.Controllers
{
    [Authorize]
    public class FloorController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;

        public FloorController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Create(int propertyId)
        {
            var viewModel = new FloorUpsertViewModel
            {
                Floor = new CreateFloorDto { PropertyId = propertyId },
                FloorUsages = new SelectList(Enum.GetValues<FloorUsage>().Select(e => new { Id = (int)e, Name = e.ToString() }), "Id", "Name"),
                StructuralConditions = new SelectList(Enum.GetValues<StructuralCondition>().Select(e => new { Id = (int)e, Name = e.ToString() }), "Id", "Name")
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FloorUpsertViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var propertyExists = await _unitOfWork.GetQueryable<Property>()
                    .AnyAsync(p => p.Id == viewModel.Floor.PropertyId);

                if (!propertyExists)
                {
                    ModelState.AddModelError(nameof(viewModel.Floor.PropertyId), "العقار المحدد غير موجود.");
                }
                else
                {
                    if (CurrentUserId <= 0)
                    {
                        return Unauthorized();
                    }

                    var entity = new PropertyFloor
                    {
                        PropertyId = viewModel.Floor.PropertyId,
                        FloorNumber = viewModel.Floor.FloorNumber,
                        FloorLabel = viewModel.Floor.FloorLabel,
                        FloorUsage = viewModel.Floor.FloorUsage,
                        TotalAreaSqm = viewModel.Floor.TotalAreaSqm,
                        UsableAreaSqm = viewModel.Floor.UsableAreaSqm,
                        CeilingHeightCm = viewModel.Floor.CeilingHeightCm,
                        StructuralCondition = viewModel.Floor.StructuralCondition,
                        HasBalcony = viewModel.Floor.HasBalcony,
                        Notes = viewModel.Floor.Notes,
                        CreatedById = CurrentUserId
                    };

                    await _unitOfWork.AddAsync(entity);
                    await _unitOfWork.SaveChangesAsync();

                    SuccessMessage("تمت إضافة الطابق بنجاح");
                    return RedirectToAction("Details", "Property", new { id = viewModel.Floor.PropertyId });
                }
            }

            viewModel.FloorUsages = new SelectList(
                Enum.GetValues<FloorUsage>().Select(e => new { Id = (int)e, Name = e.ToString() }),
                "Id",
                "Name",
                (int)viewModel.Floor.FloorUsage);

            viewModel.StructuralConditions = new SelectList(
                Enum.GetValues<StructuralCondition>().Select(e => new { Id = (int)e, Name = e.ToString() }),
                "Id",
                "Name",
                viewModel.Floor.StructuralCondition.HasValue ? (int)viewModel.Floor.StructuralCondition.Value : null);

            return View(viewModel);
        }
    }

    [Authorize]
    public class UnitController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;

        public UnitController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Create(int propertyId, int floorId)
        {
            var viewModel = new UnitUpsertViewModel
            {
                Unit = new CreateUnitDto { PropertyId = propertyId, FloorId = floorId },
                UnitTypes = new SelectList(Enum.GetValues<UnitType>().Select(e => new { Id = (int)e, Name = e.ToString() }), "Id", "Name"),
                OccupancyStatuses = new SelectList(Enum.GetValues<OccupancyStatus>().Select(e => new { Id = (int)e, Name = e.ToString() }), "Id", "Name"),
                FurnishedStatuses = new SelectList(Enum.GetValues<FurnishedStatus>().Select(e => new { Id = (int)e, Name = e.ToString() }), "Id", "Name"),
                StructuralConditions = new SelectList(Enum.GetValues<StructuralCondition>().Select(e => new { Id = (int)e, Name = e.ToString() }), "Id", "Name")
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UnitUpsertViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var floorExists = await _unitOfWork.GetQueryable<PropertyFloor>()
                    .AnyAsync(f => f.Id == viewModel.Unit.FloorId && f.PropertyId == viewModel.Unit.PropertyId);

                if (!floorExists)
                {
                    ModelState.AddModelError(nameof(viewModel.Unit.FloorId), "الطابق المحدد غير موجود أو لا يتبع العقار.");
                }
                else
                {
                    if (CurrentUserId <= 0)
                    {
                        return Unauthorized();
                    }

                    var entity = new PropertyUnit
                    {
                        FloorId = viewModel.Unit.FloorId,
                        PropertyId = viewModel.Unit.PropertyId,
                        UnitNumber = viewModel.Unit.UnitNumber,
                        UnitType = viewModel.Unit.UnitType,
                        AreaSqm = viewModel.Unit.AreaSqm,
                        BedroomCount = viewModel.Unit.BedroomCount,
                        BathroomCount = viewModel.Unit.BathroomCount,
                        OccupancyStatus = viewModel.Unit.OccupancyStatus,
                        MarketRentMonthly = viewModel.Unit.MarketRentMonthly,
                        ElectricMeterNo = viewModel.Unit.ElectricMeterNo,
                        WaterMeterNo = viewModel.Unit.WaterMeterNo,
                        HasAC = viewModel.Unit.HasAC,
                        HasKitchen = viewModel.Unit.HasKitchen,
                        Furnished = viewModel.Unit.Furnished,
                        Condition = viewModel.Unit.Condition,
                        Notes = viewModel.Unit.Notes,
                        CreatedById = CurrentUserId
                    };

                    await _unitOfWork.AddAsync(entity);
                    await _unitOfWork.SaveChangesAsync();

                    SuccessMessage("تمت إضافة الوحدة بنجاح");
                    return RedirectToAction("Details", "Property", new { id = viewModel.Unit.PropertyId });
                }
            }

            viewModel.UnitTypes = new SelectList(
                Enum.GetValues<UnitType>().Select(e => new { Id = (int)e, Name = e.ToString() }),
                "Id",
                "Name",
                (int)viewModel.Unit.UnitType);

            viewModel.OccupancyStatuses = new SelectList(
                Enum.GetValues<OccupancyStatus>().Select(e => new { Id = (int)e, Name = e.ToString() }),
                "Id",
                "Name",
                (int)viewModel.Unit.OccupancyStatus);

            viewModel.FurnishedStatuses = new SelectList(
                Enum.GetValues<FurnishedStatus>().Select(e => new { Id = (int)e, Name = e.ToString() }),
                "Id",
                "Name",
                (int)viewModel.Unit.Furnished);

            viewModel.StructuralConditions = new SelectList(
                Enum.GetValues<StructuralCondition>().Select(e => new { Id = (int)e, Name = e.ToString() }),
                "Id",
                "Name",
                viewModel.Unit.Condition.HasValue ? (int)viewModel.Unit.Condition.Value : null);

            return View(viewModel);
        }
    }
}
