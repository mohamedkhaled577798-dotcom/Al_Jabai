using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WaqfSystem.Application.DTOs.Property;
using WaqfSystem.Application.Services;
using WaqfSystem.Core.Enums;
using WaqfSystem.Web.ViewModels;

namespace WaqfSystem.Web.Controllers
{
    [Authorize]
    public class AgriculturalController : BaseController
    {
        private readonly IAgriculturalService _agriculturalService;

        public AgriculturalController(IAgriculturalService agriculturalService)
        {
            _agriculturalService = agriculturalService;
        }

        [HttpGet]
        public async Task<IActionResult> Manage(int propertyId)
        {
            var detail = await _agriculturalService.GetByPropertyIdAsync(propertyId);
            var dto = detail != null ? new CreateAgriculturalDto { PropertyId = propertyId } : new CreateAgriculturalDto { PropertyId = propertyId }; 
            // In production, map back from detail to dto
            
            ViewBag.SoilTypes = new SelectList(Enum.GetValues<SoilType>().Select(e => new { Id = (int)e, Name = e.ToString() }), "Id", "Name");
            ViewBag.WaterSources = new SelectList(Enum.GetValues<WaterSourceType>().Select(e => new { Id = (int)e, Name = e.ToString() }), "Id", "Name");
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Manage(CreateAgriculturalDto dto)
        {
            if (ModelState.IsValid)
            {
                await _agriculturalService.UpsertAsync(dto, CurrentUserId);
                SuccessMessage("تمت تحديث تفاصيل الأرض الزراعية");
                return RedirectToAction("Details", "Property", new { id = dto.PropertyId });
            }
            return View(dto);
        }
    }
}
