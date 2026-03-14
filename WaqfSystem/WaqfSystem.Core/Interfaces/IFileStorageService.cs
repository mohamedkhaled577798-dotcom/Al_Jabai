using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace WaqfSystem.Core.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> UploadFileAsync(IFormFile file, string folder);
        Task<string> UploadFileAsync(byte[] fileData, string fileName, string folder);
        Task<bool> DeleteFileAsync(string fileUrl);
        Task<byte[]?> DownloadFileAsync(string fileUrl);
        string GetFullUrl(string relativePath);
        Task<string> GenerateThumbnailAsync(string fileUrl);
    }
}
