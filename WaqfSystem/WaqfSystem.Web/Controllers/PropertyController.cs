using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WaqfSystem.Application.DTOs.Property;
using WaqfSystem.Application.Services;
using WaqfSystem.Core.Enums;
using WaqfSystem.Core.Interfaces;
using WaqfSystem.Core.Entities;
using WaqfSystem.Web.ViewModels;

namespace WaqfSystem.Web.Controllers
{
    [Authorize]
    public class PropertyController : BaseController
    {
        private readonly IPropertyService _propertyService;
        private readonly IUnitOfWork _unitOfWork; // For lookups

        public PropertyController(IPropertyService propertyService, IUnitOfWork unitOfWork)
        {
            _propertyService = propertyService;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index(PropertyFilterRequest filter)
        {
            var results = await _propertyService.GetPagedAsync(filter, CurrentUserId, CurrentUserRole, CurrentUserGovernorateId);
            
            var viewModel = new PropertyIndexViewModel
            {
                Results = results,
                Filter = filter,
                Governorates = new SelectList(await _unitOfWork.GetQueryable<Governorate>().ToListAsync(), "Id", "NameAr", filter.GovernorateId),
                PropertyTypes = new SelectList(GetPropertyTypeNames(), "Id", "Name", filter.PropertyType),
                OwnershipTypes = new SelectList(GetOwnershipTypeNames(), "Id", "Name", filter.OwnershipType),
                ApprovalStages = new SelectList(GetApprovalStageNames(), "Id", "Name", filter.ApprovalStage)
            };

            return View(viewModel);
        }

        private IEnumerable<object> GetPropertyTypeNames() => new[]
        {
            new { Id = (int)PropertyType.Mosque, Name = "مسجد" },
            new { Id = (int)PropertyType.Hussainiya, Name = "حسينية" },
            new { Id = (int)PropertyType.School, Name = "مدرسة" },
            new { Id = (int)PropertyType.Hospital, Name = "مستشفى" },
            new { Id = (int)PropertyType.CommercialBuilding, Name = "مبنى تجاري" },
            new { Id = (int)PropertyType.ResidentialBuilding, Name = "مبنى سكني" },
            new { Id = (int)PropertyType.MixedUse, Name = "مبنى مختلط" },
            new { Id = (int)PropertyType.Land, Name = "أرض" },
            new { Id = (int)PropertyType.Farm, Name = "مزرعة" },
            new { Id = (int)PropertyType.Cemetery, Name = "مقبرة" },
            new { Id = (int)PropertyType.Agricultural, Name = "زراعي" },
            new { Id = (int)PropertyType.Other, Name = "أخرى" }
        };

        private IEnumerable<object> GetOwnershipTypeNames() => new[]
        {
            new { Id = (int)OwnershipType.FullWaqf, Name = "مضبوط (ملكية كاملة)" },
            new { Id = (int)OwnershipType.Partnership, Name = "ملحق (شراكة)" }
        };

        private IEnumerable<object> GetApprovalStageNames() => new[]
        {
            new { Id = (int)ApprovalStage.Draft, Name = "مسودة" },
            new { Id = (int)ApprovalStage.FieldSupervisorReview, Name = "مراجعة المشرف الميداني" },
            new { Id = (int)ApprovalStage.LegalReview, Name = "المراجعة القانونية" },
            new { Id = (int)ApprovalStage.EngineeringReview, Name = "المراجعة الهندسية" },
            new { Id = (int)ApprovalStage.RegionalApproval, Name = "الموافقة الإقليمية" },
            new { Id = (int)ApprovalStage.Approved, Name = "معتمد" },
            new { Id = (int)ApprovalStage.SentForCorrection, Name = "أعيد للتصحيح" }
        };

        public async Task<IActionResult> Details(int id)
        {
            var property = await _propertyService.GetByIdAsync(id);
            if (property == null) return NotFound();
            return View(property);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var viewModel = new PropertyUpsertViewModel
            {
                Property = new UpdatePropertyDto { OwnershipPercentage = 100 },
                Governorates = new SelectList(await _unitOfWork.GetQueryable<Governorate>().ToListAsync(), "Id", "NameAr"),
                PropertyTypes = new SelectList(Enum.GetValues<PropertyType>().Select(e => new { Id = (int)e, Name = e.ToString() }), "Id", "Name"),
                PropertyCategories = new SelectList(Enum.GetValues<PropertyCategory>().Select(e => new { Id = (int)e, Name = e.ToString() }), "Id", "Name"),
                OwnershipTypes = new SelectList(Enum.GetValues<OwnershipType>().Select(e => new { Id = (int)e, Name = e.ToString() }), "Id", "Name"),
                WaqfTypes = new SelectList(Enum.GetValues<WaqfType>().Select(e => new { Id = (int)e, Name = e.ToString() }), "Id", "Name"),
                ConstructionTypes = new SelectList(Enum.GetValues<ConstructionType>().Select(e => new { Id = (int)e, Name = e.ToString() }), "Id", "Name"),
                StructuralConditions = new SelectList(Enum.GetValues<StructuralCondition>().Select(e => new { Id = (int)e, Name = e.ToString() }), "Id", "Name")
            };
            return View("Upsert", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PropertyUpsertViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _propertyService.CreateAsync(viewModel.Property, CurrentUserId);
                    SuccessMessage("تم إنشاء العقار بنجاح");
                    return RedirectToAction(nameof(Details), new { id = result.Id });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"خطأ أثناء الحفظ: {ex.Message}");
                }
            }

            // Repopulate lookups on error
            viewModel.Governorates = new SelectList(await _unitOfWork.GetQueryable<Governorate>().ToListAsync(), "Id", "NameAr");
            viewModel.PropertyTypes = new SelectList(Enum.GetValues<PropertyType>().Select(e => new { Id = (int)e, Name = e.ToString() }), "Id", "Name");
            viewModel.PropertyCategories = new SelectList(Enum.GetValues<PropertyCategory>().Select(e => new { Id = (int)e, Name = e.ToString() }), "Id", "Name");
            viewModel.OwnershipTypes = new SelectList(Enum.GetValues<OwnershipType>().Select(e => new { Id = (int)e, Name = e.ToString() }), "Id", "Name");
            viewModel.WaqfTypes = new SelectList(Enum.GetValues<WaqfType>().Select(e => new { Id = (int)e, Name = e.ToString() }), "Id", "Name");
            viewModel.ConstructionTypes = new SelectList(Enum.GetValues<ConstructionType>().Select(e => new { Id = (int)e, Name = e.ToString() }), "Id", "Name");
            viewModel.StructuralConditions = new SelectList(Enum.GetValues<StructuralCondition>().Select(e => new { Id = (int)e, Name = e.ToString() }), "Id", "Name");
            return View("Upsert", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var property = await _propertyService.GetByIdAsync(id);
            if (property == null) return NotFound();

            // Mapping DetailDto back to UpdateDto for form
            // (In production, use AutoMapper for this reverse mapping too)
            var updateDto = new UpdatePropertyDto
            {
                Id = property.Id,
                PropertyName = property.PropertyName,
                PropertyType = property.PropertyType,
                // ... map all fields
            };

            var viewModel = new PropertyUpsertViewModel
            {
                Property = updateDto,
                Governorates = new SelectList(await _unitOfWork.GetQueryable<Governorate>().ToListAsync(), "Id", "NameAr", property.GovernorateId),
                PropertyTypes = new SelectList(Enum.GetValues<PropertyType>().Select(e => new { Id = (int)e, Name = e.ToString() }), "Id", "Name"),
                PropertyCategories = new SelectList(Enum.GetValues<PropertyCategory>().Select(e => new { Id = (int)e, Name = e.ToString() }), "Id", "Name"),
                OwnershipTypes = new SelectList(Enum.GetValues<OwnershipType>().Select(e => new { Id = (int)e, Name = e.ToString() }), "Id", "Name"),
                WaqfTypes = new SelectList(Enum.GetValues<WaqfType>().Select(e => new { Id = (int)e, Name = e.ToString() }), "Id", "Name"),
                ConstructionTypes = new SelectList(Enum.GetValues<ConstructionType>().Select(e => new { Id = (int)e, Name = e.ToString() }), "Id", "Name"),
                StructuralConditions = new SelectList(Enum.GetValues<StructuralCondition>().Select(e => new { Id = (int)e, Name = e.ToString() }), "Id", "Name")
            };

            return View("Upsert", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PropertyUpsertViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _propertyService.UpdateAsync(viewModel.Property, CurrentUserId);
                    SuccessMessage("تم تعديل العقار بنجاح");
                    return RedirectToAction(nameof(Details), new { id = result.Id });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"خطأ أثناء التعديل: {ex.Message}");
                }
            }

            viewModel.Governorates = new SelectList(await _unitOfWork.GetQueryable<Governorate>().ToListAsync(), "Id", "NameAr", viewModel.Property.GovernorateId);
            viewModel.PropertyTypes = new SelectList(Enum.GetValues<PropertyType>().Select(e => new { Id = (int)e, Name = e.ToString() }), "Id", "Name");
            viewModel.PropertyCategories = new SelectList(Enum.GetValues<PropertyCategory>().Select(e => new { Id = (int)e, Name = e.ToString() }), "Id", "Name");
            viewModel.OwnershipTypes = new SelectList(Enum.GetValues<OwnershipType>().Select(e => new { Id = (int)e, Name = e.ToString() }), "Id", "Name");
            viewModel.WaqfTypes = new SelectList(Enum.GetValues<WaqfType>().Select(e => new { Id = (int)e, Name = e.ToString() }), "Id", "Name");
            viewModel.ConstructionTypes = new SelectList(Enum.GetValues<ConstructionType>().Select(e => new { Id = (int)e, Name = e.ToString() }), "Id", "Name");
            viewModel.StructuralConditions = new SelectList(Enum.GetValues<StructuralCondition>().Select(e => new { Id = (int)e, Name = e.ToString() }), "Id", "Name");

            return View("Upsert", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _propertyService.SoftDeleteAsync(id);
            SuccessMessage("تم حذف العقار بنجاح");
            return RedirectToAction(nameof(Index));
        }
    }
}
