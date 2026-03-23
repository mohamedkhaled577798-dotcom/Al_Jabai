using System.Threading.Tasks;

namespace WaqfSystem.Application.Services
{
    public interface IOcrService
    {
        Task<(string Text, decimal Confidence)> ExtractTextAsync(string fileUrl, string mimeType);
    }
}
