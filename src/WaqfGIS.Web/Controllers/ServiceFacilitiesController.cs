using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NetTopologySuite.Geometries;
using WaqfGIS.Core.Entities;
using WaqfGIS.Core.Interfaces;
using WaqfGIS.Services.GIS;

namespace WaqfGIS.Web.Controllers;

[Authorize]
public class ServiceFacilitiesController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly GeometryService _geometryService;
    private readonly SpatialAnalysisService _spatialAnalysisService;
    private readonly ILogger<ServiceFacilitiesController> _logger;

    public ServiceFacilitiesController(
        IUnitOfWork unitOfWork,
        GeometryService geometryService,
        SpatialAnalysisService spatialAnalysisService,
        ILogger<ServiceFacilitiesController> logger)
    {
        _unitOfWork = unitOfWork;
        _geometryService = geometryService;
        _spatialAnalysisService = spatialAnalysisService;
        _logger = logger;
    }

    // GET: ServiceFacilities
    public async Task<IActionResult> Index(string? category, string? type, int page = 1)
    {
        try
        {
            var pageSize = 20;
            var facilities = await _unitOfWork.Repository<ServiceFacility>().GetAllAsync();

            // تطبيق الفلاتر
            if (!string.IsNullOrEmpty(category))
            {
                facilities = facilities.Where(f => f.ServiceCategory == category).ToList();
            }

            if (!string.IsNullOrEmpty(type))
            {
                facilities = facilities.Where(f => f.ServiceType == type).ToList();
            }

            var totalCount = facilities.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var pagedFacilities = facilities
                .OrderBy(f => f.ServiceCategory)
                .ThenBy(f => f.NameAr)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Category = category;
            ViewBag.Type = type;

            return View(pagedFacilities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading service facilities");
            TempData["Error"] = "حدث خطأ أثناء تحميل المرافق الخدمية";
            return View(new List<ServiceFacility>());
        }
    }

    // API: Get facilities near property
    [HttpGet]
    public async Task<IActionResult> GetNearbyFacilities(int entityId, string entityType, double radiusKm = 5.0)
    {
        try
        {
            Point? location = null;

            // الحصول على موقع العقار/المسجد
            if (entityType == "WaqfProperty")
            {
                var property = await _unitOfWork.Repository<WaqfProperty>().GetByIdAsync(entityId);
                location = property?.Location;
            }
            else if (entityType == "Mosque")
            {
                var mosque = await _unitOfWork.Repository<Mosque>().GetByIdAsync(entityId);
                location = mosque?.Location;
            }
            else if (entityType == "WaqfLand")
            {
                var land = await _unitOfWork.Repository<WaqfLand>().GetByIdAsync(entityId);
                location = land?.CenterPoint;
            }

            if (location == null)
            {
                return Json(new { success = false, message = "الموقع غير موجود" });
            }

            // البحث عن المرافق القريبة
            var allFacilities = await _unitOfWork.Repository<ServiceFacility>().GetAllAsync();
            var nearbyFacilities = allFacilities
                .Select(f => new
                {
                    Facility = f,
                    Distance = _geometryService.CalculateDistance(location, f.Location)
                })
                .Where(x => x.Distance <= radiusKm * 1000) // تحويل إلى متر
                .OrderBy(x => x.Distance)
                .Select(x => new
                {
                    x.Facility.Id,
                    x.Facility.NameAr,
                    x.Facility.ServiceCategory,
                    x.Facility.ServiceType,
                    x.Facility.Latitude,
                    x.Facility.Longitude,
                    Distance = Math.Round(x.Distance, 2)
                })
                .ToList();

            return Json(new { success = true, data = nearbyFacilities });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting nearby facilities");
            return Json(new { success = false, message = "حدث خطأ أثناء البحث عن المرافق" });
        }
    }

    // GET: ServiceFacilities/Create
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    // POST: ServiceFacilities/Create
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ServiceFacility model)
    {
        try
        {
            if (model.Latitude == 0 || model.Longitude == 0)
            {
                return Json(new { success = false, message = "يرجى تحديد الموقع على الخريطة" });
            }

            // إنشاء النقطة الجغرافية
            var geometryFactory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            model.Location = geometryFactory.CreatePoint(new NetTopologySuite.Geometries.Coordinate(model.Longitude, model.Latitude));

            model.CreatedBy = User.Identity?.Name ?? "System";
            model.CreatedAt = DateTime.Now;

            await _unitOfWork.Repository<ServiceFacility>().AddAsync(model);
            await _unitOfWork.SaveChangesAsync();

            return Json(new { success = true, message = "تم إضافة المرفق بنجاح", id = model.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating service facility");
            return Json(new { success = false, message = "حدث خطأ أثناء الحفظ" });
        }
    }
}
