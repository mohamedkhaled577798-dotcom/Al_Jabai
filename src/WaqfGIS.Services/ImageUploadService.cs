using Microsoft.AspNetCore.Http;
using WaqfGIS.Core.Entities;
using WaqfGIS.Core.Interfaces;
using WaqfGIS.Services.Storage;

namespace WaqfGIS.Services;

/// <summary>
/// خدمة رفع وإدارة الصور — محدّثة لاستخدام التخزين الآمن المشفَّر
/// جميع الصور تُخزَّن مشفّرة في D:\WakfData
/// </summary>
public class ImageUploadService
{
    private readonly IUnitOfWork             _unitOfWork;
    private readonly SecureFileStorageService _storage;

    private static readonly string[] ImageExtensions =
        { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

    public ImageUploadService(IUnitOfWork unitOfWork, SecureFileStorageService storage)
    {
        _unitOfWork = unitOfWork;
        _storage    = storage;
    }

    // =================== مساجد ===================
    public async Task<MosqueImage> UploadMosqueImageAsync(
        int mosqueId, IFormFile file, string? description, string? uploadedBy)
    {
        var saved = await _storage.SaveFileAsync(file, "MosqueImages", ImageExtensions);

        var image = new MosqueImage
        {
            MosqueId         = mosqueId,
            FileName         = saved.Uid,                  // UID على القرص
            OriginalFileName = saved.OriginalName,
            FilePath         = saved.DbPath,               // "MosqueImages/uid"
            FileSize         = saved.FileSize,
            ContentType      = saved.MimeType,
            Description      = description,
            UploadedAt       = DateTime.UtcNow,
            UploadedBy       = uploadedBy,
            IsMain           = false
        };

        await _unitOfWork.MosqueImages.AddAsync(image);
        await _unitOfWork.SaveChangesAsync();
        return image;
    }

    public async Task<List<MosqueImage>> UploadMosqueImagesAsync(
        int mosqueId, IFormFileCollection files, string? uploadedBy)
    {
        var images  = new List<MosqueImage>();
        bool isFirst = !await HasMosqueImagesAsync(mosqueId);

        foreach (var file in files)
        {
            if (!IsValidImage(file)) continue;
            var image = await UploadMosqueImageAsync(mosqueId, file, null, uploadedBy);
            if (isFirst) { image.IsMain = true; await _unitOfWork.MosqueImages.UpdateAsync(image); await _unitOfWork.SaveChangesAsync(); isFirst = false; }
            images.Add(image);
        }
        return images;
    }

    // =================== عقارات ===================
    public async Task<PropertyImage> UploadPropertyImageAsync(
        int propertyId, IFormFile file, string? description, string? uploadedBy)
    {
        var saved = await _storage.SaveFileAsync(file, "PropertyImages", ImageExtensions);

        var image = new PropertyImage
        {
            WaqfPropertyId   = propertyId,
            FileName         = saved.Uid,
            OriginalFileName = saved.OriginalName,
            FilePath         = saved.DbPath,
            FileSize         = saved.FileSize,
            ContentType      = saved.MimeType,
            Description      = description,
            UploadedAt       = DateTime.UtcNow,
            UploadedBy       = uploadedBy,
            IsMain           = false
        };

        await _unitOfWork.PropertyImages.AddAsync(image);
        await _unitOfWork.SaveChangesAsync();
        return image;
    }

    public async Task<List<PropertyImage>> UploadPropertyImagesAsync(
        int propertyId, IFormFileCollection files, string? uploadedBy)
    {
        var images  = new List<PropertyImage>();
        bool isFirst = !await HasPropertyImagesAsync(propertyId);

        foreach (var file in files)
        {
            if (!IsValidImage(file)) continue;
            var image = await UploadPropertyImageAsync(propertyId, file, null, uploadedBy);
            if (isFirst) { image.IsMain = true; await _unitOfWork.PropertyImages.UpdateAsync(image); await _unitOfWork.SaveChangesAsync(); isFirst = false; }
            images.Add(image);
        }
        return images;
    }

    // =================== دوائر ===================
    public async Task<OfficeImage> UploadOfficeImageAsync(
        int officeId, IFormFile file, string? description, string? uploadedBy)
    {
        var saved = await _storage.SaveFileAsync(file, "OfficeDocs", ImageExtensions);

        var image = new OfficeImage
        {
            WaqfOfficeId     = officeId,
            FileName         = saved.Uid,
            OriginalFileName = saved.OriginalName,
            FilePath         = saved.DbPath,
            FileSize         = saved.FileSize,
            ContentType      = saved.MimeType,
            Description      = description,
            UploadedAt       = DateTime.UtcNow,
            UploadedBy       = uploadedBy,
            IsMain           = false
        };

        await _unitOfWork.OfficeImages.AddAsync(image);
        await _unitOfWork.SaveChangesAsync();
        return image;
    }

    // =================== قراءة ===================
    public async Task<IEnumerable<MosqueImage>> GetMosqueImagesAsync(int mosqueId)
        => await Task.FromResult(_unitOfWork.MosqueImages.Query()
            .Where(i => i.MosqueId == mosqueId)
            .OrderByDescending(i => i.IsMain).ThenByDescending(i => i.UploadedAt).ToList());

    public async Task<IEnumerable<PropertyImage>> GetPropertyImagesAsync(int propertyId)
        => await Task.FromResult(_unitOfWork.PropertyImages.Query()
            .Where(i => i.WaqfPropertyId == propertyId)
            .OrderByDescending(i => i.IsMain).ThenByDescending(i => i.UploadedAt).ToList());

    public async Task<IEnumerable<OfficeImage>> GetOfficeImagesAsync(int officeId)
        => await Task.FromResult(_unitOfWork.OfficeImages.Query()
            .Where(i => i.WaqfOfficeId == officeId)
            .OrderByDescending(i => i.IsMain).ThenByDescending(i => i.UploadedAt).ToList());

    // =================== حذف ===================
    public async Task DeleteMosqueImageAsync(int imageId)
    {
        var image = await _unitOfWork.MosqueImages.GetByIdAsync(imageId);
        if (image == null) return;
        DeleteStoredFile(image.FilePath);
        await _unitOfWork.MosqueImages.DeleteAsync(image);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeletePropertyImageAsync(int imageId)
    {
        var image = await _unitOfWork.PropertyImages.GetByIdAsync(imageId);
        if (image == null) return;
        DeleteStoredFile(image.FilePath);
        await _unitOfWork.PropertyImages.DeleteAsync(image);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task SetMainMosqueImageAsync(int mosqueId, int imageId)
    {
        var images = _unitOfWork.MosqueImages.Query().Where(i => i.MosqueId == mosqueId).ToList();
        foreach (var img in images)
        { img.IsMain = img.Id == imageId; await _unitOfWork.MosqueImages.UpdateAsync(img); }
        await _unitOfWork.SaveChangesAsync();
    }

    // =================== مساعدات ===================

    /// <summary>
    /// بناء URL الخادم لعرض صورة مشفّرة
    /// DbPath = "MosqueImages/uid" → /Files/Get?folder=MosqueImages&uid=xxx&ext=.jpg
    /// </summary>
    public static string BuildFileUrl(string dbPath, string ext = ".jpg", bool download = false)
    {
        if (string.IsNullOrEmpty(dbPath)) return "/img/no-image.png";
        var parts  = dbPath.Split('/');
        var folder = parts.Length > 1 ? parts[0] : "MosqueImages";
        var uid    = parts.Length > 1 ? parts[1] : parts[0];
        return $"/Files/Get?folder={folder}&uid={uid}&ext={Uri.EscapeDataString(ext)}&dl={(download?"true":"false")}";
    }

    private void DeleteStoredFile(string dbPath)
    {
        if (string.IsNullOrEmpty(dbPath)) return;
        var parts  = dbPath.Split('/');
        if (parts.Length < 2) return;
        var diskPath = _storage.GetDiskPath(parts[0], parts[1]);
        _storage.DeleteFile(diskPath);
    }

    private static bool IsValidImage(IFormFile file)
    {
        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
        return allowedTypes.Contains(file.ContentType.ToLower()) && file.Length <= 10 * 1024 * 1024;
    }

    private async Task<bool> HasMosqueImagesAsync(int mosqueId)
        => await Task.FromResult(_unitOfWork.MosqueImages.Query().Any(i => i.MosqueId == mosqueId));

    private async Task<bool> HasPropertyImagesAsync(int propertyId)
        => await Task.FromResult(_unitOfWork.PropertyImages.Query().Any(i => i.WaqfPropertyId == propertyId));
}
