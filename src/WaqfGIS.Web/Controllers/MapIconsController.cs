using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WaqfGIS.Core.Entities;
using WaqfGIS.Core.Interfaces;

namespace WaqfGIS.Web.Controllers;

[Authorize]
public class MapIconsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MapIconsController> _logger;

    public MapIconsController(IUnitOfWork unitOfWork, ILogger<MapIconsController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    // GET: MapIcons
    public async Task<IActionResult> Index()
    {
        var icons = await _unitOfWork.Repository<MapIcon>().GetAllAsync();
        return View(icons.OrderBy(i => i.Category).ThenBy(i => i.NameAr));
    }

    // GET: MapIcons/GetIconsByCategory
    [HttpGet]
    public async Task<IActionResult> GetIconsByCategory(string category = "", string usedFor = "")
    {
        try
        {
            var query = _unitOfWork.Repository<MapIcon>().Query()
                .Where(i => i.IsActive);

            if (!string.IsNullOrEmpty(category))
                query = query.Where(i => i.Category == category);

            if (!string.IsNullOrEmpty(usedFor) && usedFor != "All")
                query = query.Where(i => i.UsedFor == usedFor || i.UsedFor == "All");

            var icons = await query.Select(i => new
            {
                id = i.Id,
                nameAr = i.NameAr,
                category = i.Category,
                iconClass = i.IconClass,
                iconColor = i.IconColor,
                iconShape = i.IconShape,
                iconSize = i.IconSize,
                customSvg = i.CustomSvg,
                isDefault = i.IsDefault
            }).ToListAsync();

            return Json(icons);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting icons");
            return Json(new List<object>());
        }
    }

    // POST: MapIcons/Create
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Create([FromBody] MapIcon model)
    {
        try
        {
            if (string.IsNullOrEmpty(model.NameAr))
            {
                return Json(new { success = false, message = "الاسم بالعربية مطلوب" });
            }

            if (string.IsNullOrEmpty(model.IconClass))
            {
                return Json(new { success = false, message = "رمز Font Awesome مطلوب" });
            }

            model.CreatedBy = User.Identity?.Name ?? "System";
            model.CreatedAt = DateTime.Now;

            await _unitOfWork.Repository<MapIcon>().AddAsync(model);
            await _unitOfWork.SaveChangesAsync();

            return Json(new { success = true, message = "تم إضافة الرمز بنجاح", id = model.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating icon");
            return Json(new { success = false, message = $"حدث خطأ: {ex.Message}" });
        }
    }

    // POST: MapIcons/SetDefault
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> SetDefault(int id, string usedFor)
    {
        try
        {
            // إزالة الافتراضي من الباقي
            var icons = _unitOfWork.Repository<MapIcon>().Query()
                .Where(i => i.UsedFor == usedFor).ToList();

            foreach (var icon in icons)
            {
                icon.IsDefault = false;
                await _unitOfWork.Repository<MapIcon>().UpdateAsync(icon);
            }

            // تعيين الجديد كافتراضي
            var selectedIcon = await _unitOfWork.Repository<MapIcon>().GetByIdAsync(id);
            if (selectedIcon != null)
            {
                selectedIcon.IsDefault = true;
                await _unitOfWork.Repository<MapIcon>().UpdateAsync(selectedIcon);
            }

            await _unitOfWork.SaveChangesAsync();

            return Json(new { success = true, message = "تم تحديث الرمز الافتراضي" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting default icon");
            return Json(new { success = false, message = "حدث خطأ" });
        }
    }

    // DELETE: MapIcons/Delete
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var icon = await _unitOfWork.Repository<MapIcon>().GetByIdAsync(id);
            if (icon == null)
                return Json(new { success = false, message = "الرمز غير موجود" });

            if (icon.IsSystemIcon)
                return Json(new { success = false, message = "لا يمكن حذف رمز النظام" });

            await _unitOfWork.Repository<MapIcon>().DeleteAsync(icon);
            await _unitOfWork.SaveChangesAsync();

            return Json(new { success = true, message = "تم حذف الرمز" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting icon");
            return Json(new { success = false, message = "حدث خطأ أثناء الحذف" });
        }
    }
}
