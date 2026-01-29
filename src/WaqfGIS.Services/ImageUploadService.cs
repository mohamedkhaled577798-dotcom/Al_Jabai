using Microsoft.AspNetCore.Http;
using WaqfGIS.Core.Entities;
using WaqfGIS.Core.Interfaces;

namespace WaqfGIS.Services;

/// <summary>
/// خدمة رفع وإدارة الصور
/// </summary>
public class ImageUploadService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _uploadPath;

    public ImageUploadService(IUnitOfWork unitOfWork, string webRootPath)
    {
        _unitOfWork = unitOfWork;
        _uploadPath = Path.Combine(webRootPath, "uploads", "images");
        
        if (!Directory.Exists(_uploadPath))
            Directory.CreateDirectory(_uploadPath);
    }

    public async Task<MosqueImage> UploadMosqueImageAsync(int mosqueId, IFormFile file, string? description, string? uploadedBy)
    {
        var fileName = await SaveFileAsync(file, "mosques");
        
        var image = new MosqueImage
        {
            MosqueId = mosqueId,
            FileName = fileName,
            OriginalFileName = file.FileName,
            FilePath = $"/uploads/images/mosques/{fileName}",
            FileSize = file.Length,
            ContentType = file.ContentType,
            Description = description,
            UploadedAt = DateTime.UtcNow,
            UploadedBy = uploadedBy,
            IsMain = false
        };

        await _unitOfWork.MosqueImages.AddAsync(image);
        await _unitOfWork.SaveChangesAsync();
        return image;
    }

    public async Task<List<MosqueImage>> UploadMosqueImagesAsync(int mosqueId, IFormFileCollection files, string? uploadedBy)
    {
        var images = new List<MosqueImage>();
        bool isFirst = !await HasMosqueImagesAsync(mosqueId);

        foreach (var file in files)
        {
            if (IsValidImage(file))
            {
                var image = await UploadMosqueImageAsync(mosqueId, file, null, uploadedBy);
                if (isFirst)
                {
                    image.IsMain = true;
                    await _unitOfWork.MosqueImages.UpdateAsync(image);
                    await _unitOfWork.SaveChangesAsync();
                    isFirst = false;
                }
                images.Add(image);
            }
        }
        return images;
    }

    public async Task<PropertyImage> UploadPropertyImageAsync(int propertyId, IFormFile file, string? description, string? uploadedBy)
    {
        var fileName = await SaveFileAsync(file, "properties");
        
        var image = new PropertyImage
        {
            WaqfPropertyId = propertyId,
            FileName = fileName,
            OriginalFileName = file.FileName,
            FilePath = $"/uploads/images/properties/{fileName}",
            FileSize = file.Length,
            ContentType = file.ContentType,
            Description = description,
            UploadedAt = DateTime.UtcNow,
            UploadedBy = uploadedBy,
            IsMain = false
        };

        await _unitOfWork.PropertyImages.AddAsync(image);
        await _unitOfWork.SaveChangesAsync();
        return image;
    }

    public async Task<List<PropertyImage>> UploadPropertyImagesAsync(int propertyId, IFormFileCollection files, string? uploadedBy)
    {
        var images = new List<PropertyImage>();
        bool isFirst = !await HasPropertyImagesAsync(propertyId);

        foreach (var file in files)
        {
            if (IsValidImage(file))
            {
                var image = await UploadPropertyImageAsync(propertyId, file, null, uploadedBy);
                if (isFirst)
                {
                    image.IsMain = true;
                    await _unitOfWork.PropertyImages.UpdateAsync(image);
                    await _unitOfWork.SaveChangesAsync();
                    isFirst = false;
                }
                images.Add(image);
            }
        }
        return images;
    }

    public async Task<OfficeImage> UploadOfficeImageAsync(int officeId, IFormFile file, string? description, string? uploadedBy)
    {
        var fileName = await SaveFileAsync(file, "offices");
        
        var image = new OfficeImage
        {
            WaqfOfficeId = officeId,
            FileName = fileName,
            OriginalFileName = file.FileName,
            FilePath = $"/uploads/images/offices/{fileName}",
            FileSize = file.Length,
            ContentType = file.ContentType,
            Description = description,
            UploadedAt = DateTime.UtcNow,
            UploadedBy = uploadedBy,
            IsMain = false
        };

        await _unitOfWork.OfficeImages.AddAsync(image);
        await _unitOfWork.SaveChangesAsync();
        return image;
    }

    public async Task<IEnumerable<MosqueImage>> GetMosqueImagesAsync(int mosqueId)
    {
        return await Task.FromResult(_unitOfWork.MosqueImages.Query()
            .Where(i => i.MosqueId == mosqueId)
            .OrderByDescending(i => i.IsMain)
            .ThenByDescending(i => i.UploadedAt)
            .ToList());
    }

    public async Task<IEnumerable<PropertyImage>> GetPropertyImagesAsync(int propertyId)
    {
        return await Task.FromResult(_unitOfWork.PropertyImages.Query()
            .Where(i => i.WaqfPropertyId == propertyId)
            .OrderByDescending(i => i.IsMain)
            .ThenByDescending(i => i.UploadedAt)
            .ToList());
    }

    public async Task<IEnumerable<OfficeImage>> GetOfficeImagesAsync(int officeId)
    {
        return await Task.FromResult(_unitOfWork.OfficeImages.Query()
            .Where(i => i.WaqfOfficeId == officeId)
            .OrderByDescending(i => i.IsMain)
            .ThenByDescending(i => i.UploadedAt)
            .ToList());
    }

    public async Task DeleteMosqueImageAsync(int imageId)
    {
        var image = await _unitOfWork.MosqueImages.GetByIdAsync(imageId);
        if (image != null)
        {
            DeleteFile(image.FilePath);
            await _unitOfWork.MosqueImages.DeleteAsync(image);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task DeletePropertyImageAsync(int imageId)
    {
        var image = await _unitOfWork.PropertyImages.GetByIdAsync(imageId);
        if (image != null)
        {
            DeleteFile(image.FilePath);
            await _unitOfWork.PropertyImages.DeleteAsync(image);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task SetMainMosqueImageAsync(int mosqueId, int imageId)
    {
        var images = _unitOfWork.MosqueImages.Query().Where(i => i.MosqueId == mosqueId).ToList();
        foreach (var img in images)
        {
            img.IsMain = img.Id == imageId;
            await _unitOfWork.MosqueImages.UpdateAsync(img);
        }
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<string> SaveFileAsync(IFormFile file, string subFolder)
    {
        var folderPath = Path.Combine(_uploadPath, subFolder);
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(folderPath, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return fileName;
    }

    private void DeleteFile(string filePath)
    {
        var fullPath = Path.Combine(_uploadPath, "..", filePath.TrimStart('/'));
        if (File.Exists(fullPath))
            File.Delete(fullPath);
    }

    private bool IsValidImage(IFormFile file)
    {
        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
        var maxSize = 10 * 1024 * 1024; // 10MB
        return allowedTypes.Contains(file.ContentType.ToLower()) && file.Length <= maxSize;
    }

    private async Task<bool> HasMosqueImagesAsync(int mosqueId)
    {
        return await Task.FromResult(_unitOfWork.MosqueImages.Query().Any(i => i.MosqueId == mosqueId));
    }

    private async Task<bool> HasPropertyImagesAsync(int propertyId)
    {
        return await Task.FromResult(_unitOfWork.PropertyImages.Query().Any(i => i.WaqfPropertyId == propertyId));
    }
}
