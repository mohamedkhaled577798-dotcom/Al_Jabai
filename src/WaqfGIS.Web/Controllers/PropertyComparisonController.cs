using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WaqfGIS.Core.Entities;
using WaqfGIS.Core.Interfaces;

namespace WaqfGIS.Web.Controllers;

[Authorize]
public class PropertyComparisonController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PropertyComparisonController> _logger;

    public PropertyComparisonController(
        IUnitOfWork unitOfWork,
        ILogger<PropertyComparisonController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    // GET: PropertyComparison
    public async Task<IActionResult> Index()
    {
        try
        {
            var comparisons = await _unitOfWork.Repository<PropertyComparison>().GetAllAsync();
            return View(comparisons.OrderByDescending(c => c.ComparisonDate).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading property comparisons");
            TempData["Error"] = "حدث خطأ أثناء تحميل المقارنات";
            return View(new List<PropertyComparison>());
        }
    }

    // GET: PropertyComparison/Details/5
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var comparison = await _unitOfWork.Repository<PropertyComparison>().GetByIdAsync(id);
            if (comparison == null)
            {
                TempData["Error"] = "المقارنة غير موجودة";
                return RedirectToAction(nameof(Index));
            }

            var items = await _unitOfWork.Repository<PropertyComparisonItem>()
                .FindAsync(i => i.ComparisonId == id);
            ViewBag.Items = items.ToList();

            return View(comparison);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading comparison details, ID: {ComparisonId}", id);
            TempData["Error"] = "حدث خطأ أثناء تحميل تفاصيل المقارنة";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: PropertyComparison/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string comparisonName, string description, List<int> propertyIds)
    {
        try
        {
            if (propertyIds == null || propertyIds.Count < 2)
            {
                return Json(new { success = false, message = "يجب اختيار عقارين على الأقل للمقارنة" });
            }

            var comparison = new PropertyComparison
            {
                ComparisonName = comparisonName,
                Description = description,
                ComparisonDate = DateTime.Now,
                CreatedBy = User.Identity?.Name ?? "System"
            };

            await _unitOfWork.Repository<PropertyComparison>().AddAsync(comparison);
            await _unitOfWork.SaveChangesAsync();

            // إضافة العقارات للمقارنة
            foreach (var propertyId in propertyIds)
            {
                var property = await _unitOfWork.Repository<WaqfProperty>().GetByIdAsync(propertyId);
                if (property != null)
                {
                    var item = new PropertyComparisonItem
                    {
                        ComparisonId = comparison.Id,
                        EntityType = "WaqfProperty",
                        EntityId = property.Id,
                        EntityName = property.NameAr,
                        AreaSqm = property.AreaSqm ?? 0,
                        PricePerSqm = property.PricePerSqm ?? 0,
                        TotalPrice = (property.AreaSqm ?? 0) * (property.PricePerSqm ?? 0),
                        Location = $"{property.Province?.NameAr} - {property.District?.NameAr}",
                        PropertyType = property.PropertyType?.NameAr
                    };
                    
                    await _unitOfWork.Repository<PropertyComparisonItem>().AddAsync(item);
                }
            }

            await _unitOfWork.SaveChangesAsync();

            return Json(new { success = true, comparisonId = comparison.Id, message = "تم إنشاء المقارنة بنجاح" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating comparison");
            return Json(new { success = false, message = "حدث خطأ أثناء إنشاء المقارنة" });
        }
    }

    // GET: PropertyComparison/PricingAnalysis
    public async Task<IActionResult> PricingAnalysis(int? provinceId, int? districtId, int? propertyTypeId)
    {
        try
        {
            var pricings = await _unitOfWork.Repository<PropertyPricing>().GetAllAsync();

            // تطبيق الفلاتر
            if (provinceId.HasValue)
            {
                pricings = pricings.Where(p => p.ProvinceId == provinceId.Value).ToList();
            }

            if (districtId.HasValue)
            {
                pricings = pricings.Where(p => p.DistrictId == districtId.Value).ToList();
            }

            if (propertyTypeId.HasValue)
            {
                pricings = pricings.Where(p => p.PropertyTypeId == propertyTypeId.Value).ToList();
            }

            // حساب الإحصائيات
            ViewBag.AveragePrice = pricings.Any() ? pricings.Average(p => p.PricePerSqm) : 0;
            ViewBag.MaxPrice = pricings.Any() ? pricings.Max(p => p.PricePerSqm) : 0;
            ViewBag.MinPrice = pricings.Any() ? pricings.Min(p => p.PricePerSqm) : 0;
            ViewBag.TotalRecords = pricings.Count();

            return View(pricings.OrderByDescending(p => p.PriceDate).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading pricing analysis");
            TempData["Error"] = "حدث خطأ أثناء تحميل تحليل الأسعار";
            return View(new List<PropertyPricing>());
        }
    }
}
