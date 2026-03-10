using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using WaqfGIS.Core.Entities;
using WaqfGIS.Core.Interfaces;
using WaqfGIS.Services.Storage;

namespace WaqfGIS.Services;

/// <summary>
/// خدمة إدارة سجلات الصيانة — محدّثة للتخزين الآمن المشفَّر
/// </summary>
public class MaintenanceService
{
    private readonly IUnitOfWork              _unitOfWork;
    private readonly SecureFileStorageService _storage;

    private static readonly string[] PhotoExtensions =
        { ".jpg", ".jpeg", ".png", ".webp" };

    public MaintenanceService(IUnitOfWork unitOfWork, SecureFileStorageService storage)
    {
        _unitOfWork = unitOfWork;
        _storage    = storage;
    }

    // =================== CRUD ===================

    public async Task<List<MaintenanceRecord>> GetAllAsync(
        int? provinceId = null, string? entityType = null, string? status = null)
    {
        IQueryable<MaintenanceRecord> q = _unitOfWork.Repository<MaintenanceRecord>().Query()
            .Include(m => m.Province)
            .Include(m => m.Photos);

        if (provinceId.HasValue)            q = q.Where(m => m.ProvinceId == provinceId.Value);
        if (!string.IsNullOrEmpty(entityType)) q = q.Where(m => m.EntityType == entityType);
        if (!string.IsNullOrEmpty(status))  q = q.Where(m => m.Status == status);

        return await q.OrderByDescending(m => m.ScheduledDate).ToListAsync();
    }

    public async Task<MaintenanceRecord?> GetByIdAsync(int id)
        => await _unitOfWork.Repository<MaintenanceRecord>().Query()
            .Include(m => m.Province)
            .Include(m => m.Photos)
            .FirstOrDefaultAsync(m => m.Id == id);

    public async Task<MaintenanceRecord> CreateAsync(
        MaintenanceRecord record, List<IFormFile>? photos = null)
    {
        if (record.ScheduledDate < DateTime.Today && record.Status == "مجدولة")
            record.Status = "متأخرة";

        await _unitOfWork.Repository<MaintenanceRecord>().AddAsync(record);
        await _unitOfWork.SaveChangesAsync();

        if (photos != null && photos.Any())
            await SavePhotosAsync(record.Id, photos, "قبل");

        return record;
    }

    public async Task UpdateAsync(
        MaintenanceRecord record, List<IFormFile>? newPhotos = null, string photoType = "بعد")
    {
        if (record.CompletionDate.HasValue && record.Status != "مكتملة")
            record.Status = "مكتملة";
        else if (record.ScheduledDate < DateTime.Today && record.Status == "مجدولة")
            record.Status = "متأخرة";

        await _unitOfWork.Repository<MaintenanceRecord>().UpdateAsync(record);
        await _unitOfWork.SaveChangesAsync();

        if (newPhotos != null && newPhotos.Any())
            await SavePhotosAsync(record.Id, newPhotos, photoType);
    }

    public async Task DeleteAsync(int id)
    {
        var record = await GetByIdAsync(id);
        if (record == null) return;

        // حذف جميع الصور المشفّرة من القرص
        foreach (var photo in record.Photos)
            DeletePhotoFile(photo.FilePath);

        await _unitOfWork.Repository<MaintenanceRecord>().DeleteAsync(record);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeletePhotoAsync(int photoId)
    {
        var photo = await _unitOfWork.Repository<MaintenancePhoto>().GetByIdAsync(photoId);
        if (photo == null) return;

        DeletePhotoFile(photo.FilePath);

        await _unitOfWork.Repository<MaintenancePhoto>().DeleteAsync(photo);
        await _unitOfWork.SaveChangesAsync();
    }

    // =================== إحصاءات ===================

    public async Task<MaintenanceSummary> GetSummaryAsync(int? provinceId = null)
    {
        var q = _unitOfWork.Repository<MaintenanceRecord>().Query();
        if (provinceId.HasValue) q = q.Where(m => m.ProvinceId == provinceId.Value);

        var all = await q.ToListAsync();
        return new MaintenanceSummary
        {
            Total          = all.Count,
            Scheduled      = all.Count(m => m.Status == "مجدولة"),
            InProgress     = all.Count(m => m.Status == "جارية"),
            Completed      = all.Count(m => m.Status == "مكتملة"),
            Overdue        = all.Count(m => m.Status == "متأخرة"),
            TotalEstimated = all.Sum(m => m.EstimatedCost ?? 0),
            TotalActual    = all.Where(m => m.ActualCost.HasValue).Sum(m => m.ActualCost!.Value),
            UpcomingWeek   = all.Count(m => m.Status == "مجدولة"
                                        && m.ScheduledDate >= DateTime.Today
                                        && m.ScheduledDate <= DateTime.Today.AddDays(7)),
            ByType         = all.GroupBy(m => m.MaintenanceType)
                               .Select(g => new TypeCount { Name = g.Key, Count = g.Count() })
                               .OrderByDescending(x => x.Count).ToList(),
            ByPriority     = all.GroupBy(m => m.Priority)
                               .Select(g => new TypeCount { Name = g.Key, Count = g.Count() }).ToList(),
            MonthlyStats   = Enumerable.Range(0, 6).Select(i => {
                                 var m = DateTime.Today.AddMonths(-5 + i);
                                 return new MonthCount {
                                     Month = m.ToString("MMM"),
                                     Count = all.Count(r => r.ScheduledDate.Year == m.Year
                                                         && r.ScheduledDate.Month == m.Month)
                                 };
                             }).ToList()
        };
    }

    public async Task<List<MaintenanceRecord>> GetUpcomingAsync(int days = 30, int? provinceId = null)
    {
        var until = DateTime.Today.AddDays(days);
        var q = _unitOfWork.Repository<MaintenanceRecord>().Query()
            .Include(m => m.Province)
            .Where(m => m.Status == "مجدولة" && m.ScheduledDate <= until);
        if (provinceId.HasValue) q = q.Where(m => m.ProvinceId == provinceId.Value);
        return await q.OrderBy(m => m.ScheduledDate).ToListAsync();
    }

    public async Task<List<MaintenanceRecord>> GetOverdueAsync(int? provinceId = null)
    {
        var q = _unitOfWork.Repository<MaintenanceRecord>().Query()
            .Include(m => m.Province)
            .Where(m => m.Status == "متأخرة");
        if (provinceId.HasValue) q = q.Where(m => m.ProvinceId == provinceId.Value);
        return await q.OrderBy(m => m.ScheduledDate).ToListAsync();
    }

    // =================== حفظ الصور (مشفّرة) ===================
    private async Task SavePhotosAsync(int maintenanceId, List<IFormFile> files, string photoType)
    {
        foreach (var file in files.Where(f => f.Length > 0))
        {
            try
            {
                var saved = await _storage.SaveFileAsync(file, "Maintenance", PhotoExtensions);

                var photo = new MaintenancePhoto
                {
                    MaintenanceId = maintenanceId,
                    FileName      = saved.OriginalName,         // الاسم الأصلي في DB
                    FilePath      = saved.DbPath,               // "Maintenance/uid" في DB
                    FileSize      = saved.FileSize,
                    MimeType      = saved.MimeType,
                    PhotoType     = photoType
                };
                await _unitOfWork.Repository<MaintenancePhoto>().AddAsync(photo);
            }
            catch { /* تجاهل الملفات ذات الامتداد غير المدعوم */ }
        }
        await _unitOfWork.SaveChangesAsync();
    }

    private void DeletePhotoFile(string dbPath)
    {
        if (string.IsNullOrEmpty(dbPath)) return;
        var parts = dbPath.Split('/');
        if (parts.Length < 2) return;
        var diskPath = _storage.GetDiskPath(parts[0], parts[1]);
        _storage.DeleteFile(diskPath);
    }
}

public class MaintenanceSummary
{
    public int     Total          { get; set; }
    public int     Scheduled      { get; set; }
    public int     InProgress     { get; set; }
    public int     Completed      { get; set; }
    public int     Overdue        { get; set; }
    public int     UpcomingWeek   { get; set; }
    public decimal TotalEstimated { get; set; }
    public decimal TotalActual    { get; set; }
    public List<TypeCount>  ByType       { get; set; } = new();
    public List<TypeCount>  ByPriority   { get; set; } = new();
    public List<MonthCount> MonthlyStats { get; set; } = new();
}
