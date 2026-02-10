using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WaqfGIS.Core.Entities;
using WaqfGIS.Core.Interfaces;

namespace WaqfGIS.Web.Controllers;

[Authorize]
public class ServiceCategoriesController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ServiceCategoriesController> _logger;

    public ServiceCategoriesController(IUnitOfWork unitOfWork, ILogger<ServiceCategoriesController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    // GET: ServiceCategories
    public async Task<IActionResult> Index()
    {
        var categories = await _unitOfWork.Repository<ServiceCategory>().GetAllAsync();
        return View(categories.OrderBy(c => c.DisplayOrder));
    }

    // GET: ServiceCategories/GetAll (API)
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var categories = await _unitOfWork.Repository<ServiceCategory>()
                .Query()
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .Select(c => new
                {
                    id = c.Id,
                    nameAr = c.NameAr,
                    nameEn = c.NameEn,
                    defaultIcon = c.DefaultIconClass,
                    defaultColor = c.DefaultIconColor
                })
                .ToListAsync();

            return Json(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories");
            return Json(new List<object>());
        }
    }

    // GET: ServiceCategories/GetTypesByCategory (API)
    [HttpGet]
    public async Task<IActionResult> GetTypesByCategory(int categoryId)
    {
        try
        {
            var types = await _unitOfWork.Repository<ServiceType>()
                .Query()
                .Where(t => t.ServiceCategoryId == categoryId && t.IsActive)
                .OrderBy(t => t.DisplayOrder)
                .Select(t => new
                {
                    id = t.Id,
                    nameAr = t.NameAr,
                    nameEn = t.NameEn,
                    iconClass = t.CustomIconClass,
                    iconColor = t.CustomIconColor,
                    mapIconId = t.MapIconId
                })
                .ToListAsync();

            return Json(types);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting types");
            return Json(new List<object>());
        }
    }

    // POST: ServiceCategories/CreateCategory
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> CreateCategory([FromBody] ServiceCategory model)
    {
        try
        {
            model.CreatedBy = User.Identity?.Name ?? "System";
            model.CreatedAt = DateTime.Now;

            await _unitOfWork.Repository<ServiceCategory>().AddAsync(model);
            await _unitOfWork.SaveChangesAsync();

            return Json(new { success = true, message = "تم إضافة التصنيف بنجاح", id = model.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            return Json(new { success = false, message = $"خطأ: {ex.Message}" });
        }
    }

    // POST: ServiceCategories/CreateType
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> CreateType([FromBody] ServiceType model)
    {
        try
        {
            model.CreatedBy = User.Identity?.Name ?? "System";
            model.CreatedAt = DateTime.Now;

            await _unitOfWork.Repository<ServiceType>().AddAsync(model);
            await _unitOfWork.SaveChangesAsync();

            return Json(new { success = true, message = "تم إضافة النوع بنجاح", id = model.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating type");
            return Json(new { success = false, message = $"خطأ: {ex.Message}" });
        }
    }

    // POST: ServiceCategories/DeleteCategory
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        try
        {
            var category = await _unitOfWork.Repository<ServiceCategory>().GetByIdAsync(id);
            if (category == null)
                return Json(new { success = false, message = "التصنيف غير موجود" });

            // التحقق من عدم وجود أنواع
            var hasTypes = _unitOfWork.Repository<ServiceType>()
                .Query()
                .Any(t => t.ServiceCategoryId == id);

            if (hasTypes)
                return Json(new { success = false, message = "لا يمكن حذف التصنيف لأنه يحتوي على أنواع" });

            await _unitOfWork.Repository<ServiceCategory>().DeleteAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return Json(new { success = true, message = "تم حذف التصنيف" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category");
            return Json(new { success = false, message = "حدث خطأ" });
        }
    }

    // POST: ServiceCategories/DeleteType
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> DeleteType(int id)
    {
        try
        {
            var type = await _unitOfWork.Repository<ServiceType>().GetByIdAsync(id);
            if (type == null)
                return Json(new { success = false, message = "النوع غير موجود" });

            await _unitOfWork.Repository<ServiceType>().DeleteAsync(type);
            await _unitOfWork.SaveChangesAsync();

            return Json(new { success = true, message = "تم حذف النوع" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting type");
            return Json(new { success = false, message = "حدث خطأ" });
        }
    }
}
