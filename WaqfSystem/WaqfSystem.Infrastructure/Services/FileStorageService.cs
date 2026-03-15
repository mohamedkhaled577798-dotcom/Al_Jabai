using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using WaqfSystem.Core.Interfaces;

namespace WaqfSystem.Infrastructure.Services
{

    public class FileStorageService : IFileStorageService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<FileStorageService> _logger;
        private readonly string _basePath;
        private readonly string _baseUrl;

        public FileStorageService(IConfiguration configuration, ILogger<FileStorageService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _basePath = _configuration["FileStorage:BasePath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            _baseUrl = _configuration["FileStorage:BaseUrl"] ?? "/uploads";
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("الملف فارغ أو غير موجود");

            // Validate MIME type server-side
            var allowedMimeTypes = new[]
            {
                "application/pdf", "image/jpeg", "image/png", "image/tiff",
                "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
            };

            if (!Array.Exists(allowedMimeTypes, m => m.Equals(file.ContentType, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException($"نوع الملف غير مسموح: {file.ContentType}");
            }

            var uploadPath = Path.Combine(_basePath, folder);
            Directory.CreateDirectory(uploadPath);

            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploadPath, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = $"{_baseUrl}/{folder}/{uniqueFileName}";
            _logger.LogInformation("File uploaded: {FileName} to {Path}", file.FileName, relativePath);
            return relativePath;
        }

        public async Task<string> UploadFileAsync(byte[] fileData, string fileName, string folder)
        {
            if (fileData == null || fileData.Length == 0)
                throw new ArgumentException("بيانات الملف فارغة");

            var uploadPath = Path.Combine(_basePath, folder);
            Directory.CreateDirectory(uploadPath);

            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var filePath = Path.Combine(uploadPath, uniqueFileName);

            await File.WriteAllBytesAsync(filePath, fileData);

            var relativePath = $"{_baseUrl}/{folder}/{uniqueFileName}";
            _logger.LogInformation("File uploaded from bytes: {FileName} to {Path}", fileName, relativePath);
            return relativePath;
        }

        public Task<bool> DeleteFileAsync(string fileUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(fileUrl)) return Task.FromResult(false);

                var relativePath = fileUrl.Replace(_baseUrl, "").TrimStart('/');
                var filePath = Path.Combine(_basePath, relativePath);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("File deleted: {Path}", filePath);
                    return Task.FromResult(true);
                }

                _logger.LogWarning("File not found for deletion: {Path}", filePath);
                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: {FileUrl}", fileUrl);
                return Task.FromResult(false);
            }
        }

        public async Task<byte[]?> DownloadFileAsync(string fileUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(fileUrl)) return null;

                var relativePath = fileUrl.Replace(_baseUrl, "").TrimStart('/');
                var filePath = Path.Combine(_basePath, relativePath);

                if (File.Exists(filePath))
                {
                    return await File.ReadAllBytesAsync(filePath);
                }

                _logger.LogWarning("File not found for download: {Path}", filePath);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file: {FileUrl}", fileUrl);
                return null;
            }
        }

        public string GetFullUrl(string relativePath)
        {
            return relativePath;
        }

        public Task<string> GenerateThumbnailAsync(string fileUrl)
        {
            // Thumbnail generation stub — in production, use ImageSharp or SkiaSharp
            _logger.LogInformation("Thumbnail generation requested for: {FileUrl}", fileUrl);
            var thumbUrl = fileUrl.Replace("/uploads/", "/uploads/thumbs/");
            return Task.FromResult(thumbUrl);
        }
    }
}
