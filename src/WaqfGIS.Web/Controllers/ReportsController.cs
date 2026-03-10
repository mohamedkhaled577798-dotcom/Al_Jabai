using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WaqfGIS.Core.Entities;
using WaqfGIS.Core.Interfaces;
using WaqfGIS.Services;
using WaqfGIS.Web.Models;
using System.Text.Json;

namespace WaqfGIS.Web.Controllers;

[Authorize]
public class ReportsController : Controller
{
    private readonly ReportService _reportService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ExcelExportService _excelExportService;
    private readonly AuditLogService _auditLogService;
    private readonly ExcelImportService _excelImportService;
    private readonly PdfReportService _pdfReportService;

    public ReportsController(ReportService reportService, IUnitOfWork unitOfWork,
        ExcelExportService excelExportService, AuditLogService auditLogService,
        ExcelImportService excelImportService, PdfReportService pdfReportService)
    {
        _reportService      = reportService;
        _auditLogService    = auditLogService;
        _unitOfWork         = unitOfWork;
        _excelExportService = excelExportService;
        _excelImportService = excelImportService;
        _pdfReportService   = pdfReportService;
    }

    public async Task<IActionResult> Index()
    {
        var stats = await _reportService.GetDashboardStatisticsAsync();
        ViewBag.Provinces = new SelectList(await _unitOfWork.Provinces.GetAllAsync(), "Id", "NameAr");
        return View(stats);
    }

    public async Task<IActionResult> ProvinceReport(int? provinceId)
    {
        ViewBag.Provinces = new SelectList(await _unitOfWork.Provinces.GetAllAsync(), "Id", "NameAr");
        
        if (!provinceId.HasValue)
            return View(new ProvinceReportViewModel());

        var province = await _unitOfWork.Provinces.GetByIdAsync(provinceId.Value);
        if (province == null) return NotFound();

        var mosques = await _unitOfWork.Mosques.Query()
            .Include(m => m.MosqueType).Include(m => m.MosqueStatus)
            .Where(m => m.ProvinceId == provinceId.Value).ToListAsync();

        var properties = await _unitOfWork.WaqfProperties.Query()
            .Include(p => p.PropertyType).Include(p => p.UsageType)
            .Where(p => p.ProvinceId == provinceId.Value).ToListAsync();

        var offices = await _unitOfWork.WaqfOffices.Query()
            .Include(o => o.OfficeType)
            .Where(o => o.ProvinceId == provinceId.Value).ToListAsync();

        var report = new ProvinceReportViewModel
        {
            ProvinceId = provinceId.Value,
            ProvinceName = province.NameAr,
            TotalMosques = mosques.Count,
            TotalProperties = properties.Count,
            TotalOffices = offices.Count,
            TotalCapacity = mosques.Sum(m => m.Capacity ?? 0),
            TotalPropertyArea = properties.Sum(p => p.AreaSqm ?? 0),
            TotalPropertyValue = properties.Sum(p => p.EstimatedValue ?? 0),
            MosquesByType = mosques.GroupBy(m => m.MosqueType?.NameAr ?? "غير محدد")
                .Select(g => new TypeCount { Name = g.Key, Count = g.Count() }).ToList(),
            MosquesByStatus = mosques.GroupBy(m => m.MosqueStatus?.NameAr ?? "غير محدد")
                .Select(g => new TypeCount { Name = g.Key, Count = g.Count() }).ToList(),
            PropertiesByType = properties.GroupBy(p => p.PropertyType?.NameAr ?? "غير محدد")
                .Select(g => new TypeCount { Name = g.Key, Count = g.Count() }).ToList(),
            PropertiesByUsage = properties.GroupBy(p => p.UsageType?.NameAr ?? "غير محدد")
                .Select(g => new TypeCount { Name = g.Key, Count = g.Count() }).ToList(),
            Mosques = mosques,
            Properties = properties,
            Offices = offices
        };

        ViewBag.SelectedProvinceId = provinceId;
        return View(report);
    }

    public async Task<IActionResult> MosquesReport()
    {
        var mosques = await _unitOfWork.Mosques.Query()
            .Include(m => m.MosqueType).Include(m => m.MosqueStatus)
            .Include(m => m.Province).Include(m => m.WaqfOffice)
            .ToListAsync();

        var report = new MosquesReportViewModel
        {
            TotalMosques = mosques.Count,
            TotalCapacity = mosques.Sum(m => m.Capacity ?? 0),
            WithFridayPrayer = mosques.Count(m => m.HasFridayPrayer),
            ByType = mosques.GroupBy(m => m.MosqueType?.NameAr ?? "غير محدد")
                .Select(g => new TypeCount { Name = g.Key, Count = g.Count() }).ToList(),
            ByStatus = mosques.GroupBy(m => m.MosqueStatus?.NameAr ?? "غير محدد")
                .Select(g => new TypeCount { Name = g.Key, Count = g.Count() }).ToList(),
            ByProvince = mosques.GroupBy(m => m.Province?.NameAr ?? "غير محدد")
                .Select(g => new TypeCount { Name = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count).ToList(),
            Mosques = mosques
        };
        return View(report);
    }

    public async Task<IActionResult> PropertiesReport()
    {
        var properties = await _unitOfWork.WaqfProperties.Query()
            .Include(p => p.PropertyType).Include(p => p.UsageType)
            .Include(p => p.Province).Include(p => p.WaqfOffice)
            .ToListAsync();

        var report = new PropertiesReportViewModel
        {
            TotalProperties = properties.Count,
            TotalArea = properties.Sum(p => p.AreaSqm ?? 0),
            TotalValue = properties.Sum(p => p.EstimatedValue ?? 0),
            TotalRentedCount = properties.Count(p => p.RentalStatus == "مؤجر"),
            TotalMonthlyRent = properties.Where(p => p.RentalStatus == "مؤجر").Sum(p => p.MonthlyRent ?? 0),
            ByType = properties.GroupBy(p => p.PropertyType?.NameAr ?? "غير محدد")
                .Select(g => new TypeCount { Name = g.Key, Count = g.Count() }).ToList(),
            ByUsage = properties.GroupBy(p => p.UsageType?.NameAr ?? "غير محدد")
                .Select(g => new TypeCount { Name = g.Key, Count = g.Count() }).ToList(),
            ByProvince = properties.GroupBy(p => p.Province?.NameAr ?? "غير محدد")
                .Select(g => new TypeCount { Name = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count).ToList(),
            Properties = properties
        };
        return View(report);
    }

    // ========== تصدير Excel ==========

    [HttpGet]
    public async Task<IActionResult> ExportMosques()
    {
        var mosques = await _unitOfWork.Mosques.Query()
            .Include(m => m.MosqueType).Include(m => m.MosqueStatus)
            .Include(m => m.Province).Include(m => m.WaqfOffice)
            .ToListAsync();

        var fileContent = _excelExportService.ExportMosquesToExcel(mosques);
        var fileName = $"المساجد_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        
        return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    [HttpGet]
    public async Task<IActionResult> ExportProperties()
    {
        var properties = await _unitOfWork.WaqfProperties.Query()
            .Include(p => p.PropertyType).Include(p => p.UsageType)
            .Include(p => p.Province).Include(p => p.WaqfOffice)
            .ToListAsync();

        var fileContent = _excelExportService.ExportPropertiesToExcel(properties);
        var fileName = $"العقارات_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        
        return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    [HttpGet]
    public async Task<IActionResult> ExportOffices()
    {
        var offices = await _unitOfWork.WaqfOffices.Query()
            .Include(o => o.OfficeType).Include(o => o.Province).Include(o => o.ParentOffice)
            .ToListAsync();

        var fileContent = _excelExportService.ExportOfficesToExcel(offices);
        var fileName = $"الدوائر_الوقفية_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        
        return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    [HttpGet]
    public async Task<IActionResult> ExportProvinceReport(int provinceId)
    {
        var province = await _unitOfWork.Provinces.GetByIdAsync(provinceId);
        if (province == null) return NotFound();

        var mosques = await _unitOfWork.Mosques.Query()
            .Include(m => m.MosqueType).Include(m => m.MosqueStatus)
            .Include(m => m.Province).Include(m => m.WaqfOffice)
            .Where(m => m.ProvinceId == provinceId).ToListAsync();

        var fileContent = _excelExportService.ExportMosquesToExcel(mosques);
        var fileName = $"تقرير_{province.NameAr}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        
        return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    // ========== استيراد Excel ==========

    [HttpGet]
    public async Task<IActionResult> Import()
    {
        ViewBag.Offices = new SelectList(await _unitOfWork.WaqfOffices.GetAllAsync(), "Id", "NameAr");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10MB
    public async Task<IActionResult> ImportMosques(IFormFile file, int officeId)
    {
        if (file == null || file.Length == 0)
        { TempData["Error"] = "الرجاء اختيار ملف"; return RedirectToAction(nameof(Import)); }

        using var stream = file.OpenReadStream();
        var result = await _excelImportService.ImportMosquesAsync(stream, file.FileName, officeId);
        TempData["ImportResult"] = System.Text.Json.JsonSerializer.Serialize(result);
        return RedirectToAction(nameof(Import));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> ImportProperties(IFormFile file, int officeId)
    {
        if (file == null || file.Length == 0)
        { TempData["Error"] = "الرجاء اختيار ملف"; return RedirectToAction(nameof(Import)); }

        using var stream = file.OpenReadStream();
        var result = await _excelImportService.ImportPropertiesAsync(stream, file.FileName, officeId);
        TempData["ImportResult"] = System.Text.Json.JsonSerializer.Serialize(result);
        return RedirectToAction(nameof(Import));
    }

    // ========== تصدير PDF ==========

    [HttpGet]
    public async Task<IActionResult> ExportMosquePdf(int id)
    {
        var mosque = await _unitOfWork.Mosques.Query()
            .Include(m => m.MosqueType).Include(m => m.MosqueStatus)
            .Include(m => m.Province).Include(m => m.WaqfOffice)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (mosque == null) return NotFound();

        var pdf = _pdfReportService.GenerateMosqueCard(mosque);
        return File(pdf, "application/pdf", $"مسجد_{mosque.Code}_{DateTime.Now:yyyyMMdd}.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> ExportPropertyPdf(int id)
    {
        var property = await _unitOfWork.WaqfProperties.Query()
            .Include(p => p.PropertyType).Include(p => p.UsageType)
            .Include(p => p.Province).Include(p => p.WaqfOffice)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (property == null) return NotFound();

        var pdf = _pdfReportService.GeneratePropertyCard(property);
        return File(pdf, "application/pdf", $"عقار_{property.Code}_{DateTime.Now:yyyyMMdd}.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> ExportProvincePdf(int provinceId)
    {
        var province = await _unitOfWork.Provinces.GetByIdAsync(provinceId);
        if (province == null) return NotFound();

        var mosques = await _unitOfWork.Mosques.Query()
            .Include(m => m.MosqueType).Include(m => m.MosqueStatus)
            .Where(m => m.ProvinceId == provinceId).ToListAsync();
        var properties = await _unitOfWork.WaqfProperties.Query()
            .Include(p => p.PropertyType)
            .Where(p => p.ProvinceId == provinceId).ToListAsync();

        var pdf = _pdfReportService.GenerateProvinceReport(province.NameAr, mosques, properties);
        return File(pdf, "application/pdf", $"تقرير_{province.NameAr}_{DateTime.Now:yyyyMMdd}.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> ExportMaintenancePdf(int? provinceId)
    {
        var records = await _unitOfWork.Repository<MaintenanceRecord>().Query()
            .Include(m => m.Province)
            .Where(m => !provinceId.HasValue || m.ProvinceId == provinceId)
            .OrderByDescending(m => m.ScheduledDate)
            .ToListAsync();

        var pdf = _pdfReportService.GenerateMaintenanceReport(records);
        return File(pdf, "application/pdf", $"سجل_الصيانة_{DateTime.Now:yyyyMMdd}.pdf");
    }

    // ========== سجل التدقيق ==========
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> AuditLog(string? entityType, string? action, string? userName)
    {
        var logs = await _auditLogService.GetRecentLogsAsync(500);

        if (!string.IsNullOrEmpty(entityType))
            logs = logs.Where(l => l.EntityType == entityType);
        if (!string.IsNullOrEmpty(action))
            logs = logs.Where(l => l.Action == action);
        if (!string.IsNullOrEmpty(userName))
            logs = logs.Where(l => l.UserName != null && l.UserName.Contains(userName));

        return View(logs.ToList());
    }
}
