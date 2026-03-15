using System.Threading.Tasks;

namespace WaqfSystem.Core.Interfaces
{
    public interface IOcrService
    {
        Task<(string Text, decimal Confidence)> ExtractTextAsync(string fileUrl);
        Task<(string Text, decimal Confidence)> ExtractTextFromBytesAsync(byte[] fileData, string mimeType);
    }
}
