using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Tesseract;
using UglyToad.PdfPig;
using WaqfSystem.Core.Interfaces;

namespace WaqfSystem.Infrastructure.Services
{
    public class OcrService : WaqfSystem.Application.Services.IOcrService, WaqfSystem.Core.Interfaces.IOcrService
    {
        private readonly IConfiguration _configuration;
        private readonly IFileStorageService _fileStorage;
        private readonly IHttpClientFactory? _httpClientFactory;
        private readonly ILogger<OcrService> _logger;

        public OcrService(
            IConfiguration configuration,
            IFileStorageService fileStorage,
            ILogger<OcrService> logger,
            IHttpClientFactory? httpClientFactory = null)
        {
            _configuration = configuration;
            _fileStorage = fileStorage;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<(string Text, decimal Confidence)> ExtractTextAsync(string fileUrl, string mimeType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileUrl))
                {
                    return (string.Empty, 0m);
                }

                if (mimeType.Contains("pdf", StringComparison.OrdinalIgnoreCase) || fileUrl.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    var pdf = await ReadPdfTextAsync(fileUrl);
                    return (pdf, string.IsNullOrWhiteSpace(pdf) ? 0m : 92m);
                }

                if (mimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase) ||
                    fileUrl.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                    fileUrl.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                    fileUrl.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                    fileUrl.EndsWith(".tiff", StringComparison.OrdinalIgnoreCase) ||
                    fileUrl.EndsWith(".tif", StringComparison.OrdinalIgnoreCase))
                {
                    var provider = (_configuration["Ocr:Provider"] ?? "Local").Trim();
                    if (provider.Equals("Azure", StringComparison.OrdinalIgnoreCase))
                    {
                        return await ExtractFromAzureAsync(fileUrl);
                    }

                    return await ExtractFromTesseractAsync(fileUrl);
                }

                return (string.Empty, 0m);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "OCR extraction failed for {FileUrl}", fileUrl);
                return (string.Empty, 0m);
            }
        }

        public async Task<(string Text, decimal Confidence)> ExtractTextAsync(string fileUrl)
        {
            var mime = fileUrl.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ? "application/pdf" : "image/*";
            return await ExtractTextAsync(fileUrl, mime);
        }

        public async Task<(string Text, decimal Confidence)> ExtractTextFromBytesAsync(byte[] fileData, string mimeType)
        {
            try
            {
                if (fileData == null || fileData.Length == 0)
                {
                    return (string.Empty, 0m);
                }

                if (mimeType.Contains("pdf", StringComparison.OrdinalIgnoreCase))
                {
                    using var ms = new MemoryStream(fileData);
                    using var doc = PdfDocument.Open(ms);
                    var text = string.Join(Environment.NewLine, doc.GetPages().Select(p => p.Text));
                    return (text, string.IsNullOrWhiteSpace(text) ? 0m : 90m);
                }

                var tempPath = Path.Combine(Path.GetTempPath(), $"ocr_{Guid.NewGuid():N}.bin");
                await File.WriteAllBytesAsync(tempPath, fileData);
                try
                {
                    return await ExtractFromTesseractAsync(tempPath);
                }
                finally
                {
                    if (File.Exists(tempPath))
                    {
                        File.Delete(tempPath);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "OCR extraction from bytes failed");
                return (string.Empty, 0m);
            }
        }

        private async Task<string> ReadPdfTextAsync(string fileUrl)
        {
            var bytes = await _fileStorage.DownloadFileAsync(fileUrl);
            if (bytes == null || bytes.Length == 0)
            {
                return string.Empty;
            }

            using var stream = new MemoryStream(bytes);
            using var doc = PdfDocument.Open(stream);
            var pages = doc.GetPages().Select(p => p.Text?.Trim() ?? string.Empty).Where(t => !string.IsNullOrWhiteSpace(t));
            return string.Join(Environment.NewLine, pages);
        }

        private async Task<(string Text, decimal Confidence)> ExtractFromAzureAsync(string fileUrl)
        {
            var endpoint = _configuration["Ocr:AzureEndpoint"];
            var key = _configuration["Ocr:AzureKey"];
            if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(key))
            {
                _logger.LogWarning("Azure OCR selected without endpoint/key. Falling back to local.");
                return await ExtractFromTesseractAsync(fileUrl);
            }

            if (_httpClientFactory == null)
            {
                _logger.LogWarning("IHttpClientFactory not available for Azure OCR. Falling back to local.");
                return await ExtractFromTesseractAsync(fileUrl);
            }

            var bytes = await _fileStorage.DownloadFileAsync(fileUrl);
            if (bytes == null || bytes.Length == 0)
            {
                return (string.Empty, 0m);
            }

            var client = _httpClientFactory.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint.TrimEnd('/') + "/vision/v3.2/read/analyze");
            request.Headers.Add("Ocp-Apim-Subscription-Key", key);
            request.Content = new ByteArrayContent(bytes);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Azure OCR analyze request failed with status {StatusCode}", response.StatusCode);
                return (string.Empty, 0m);
            }

            if (!response.Headers.TryGetValues("Operation-Location", out var values))
            {
                return (string.Empty, 0m);
            }

            var operationUrl = values.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(operationUrl))
            {
                return (string.Empty, 0m);
            }

            for (var i = 0; i < 12; i++)
            {
                await Task.Delay(1000);
                using var statusReq = new HttpRequestMessage(HttpMethod.Get, operationUrl);
                statusReq.Headers.Add("Ocp-Apim-Subscription-Key", key);
                var statusResp = await client.SendAsync(statusReq);
                if (!statusResp.IsSuccessStatusCode)
                {
                    continue;
                }

                var json = await statusResp.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var status = doc.RootElement.GetProperty("status").GetString();
                if (status == "succeeded")
                {
                    var sb = new StringBuilder();
                    if (doc.RootElement.TryGetProperty("analyzeResult", out var analyze) && analyze.TryGetProperty("readResults", out var pages))
                    {
                        foreach (var page in pages.EnumerateArray())
                        {
                            if (!page.TryGetProperty("lines", out var lines))
                            {
                                continue;
                            }

                            foreach (var line in lines.EnumerateArray())
                            {
                                if (line.TryGetProperty("text", out var textEl))
                                {
                                    sb.AppendLine(textEl.GetString());
                                }
                            }
                        }
                    }

                    var output = sb.ToString().Trim();
                    return (output, string.IsNullOrWhiteSpace(output) ? 0m : 88m);
                }

                if (status == "failed")
                {
                    break;
                }
            }

            return (string.Empty, 0m);
        }

        private async Task<(string Text, decimal Confidence)> ExtractFromTesseractAsync(string fileUrl)
        {
            try
            {
                var localPath = fileUrl;
                byte[]? tempBytes = null;

                if (fileUrl.StartsWith("/", StringComparison.OrdinalIgnoreCase) || fileUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    tempBytes = await _fileStorage.DownloadFileAsync(fileUrl);
                    if (tempBytes == null || tempBytes.Length == 0)
                    {
                        return (string.Empty, 0m);
                    }

                    localPath = Path.Combine(Path.GetTempPath(), $"ocr_{Guid.NewGuid():N}{Path.GetExtension(fileUrl)}");
                    await File.WriteAllBytesAsync(localPath, tempBytes);
                }

                var lang = _configuration["Ocr:LocalLanguage"] ?? "ara";
                var tessDataPath = Path.Combine(AppContext.BaseDirectory, "tessdata");
                if (!Directory.Exists(tessDataPath))
                {
                    tessDataPath = Path.Combine(Directory.GetCurrentDirectory(), "tessdata");
                }

                using var engine = new TesseractEngine(tessDataPath, lang, EngineMode.Default);
                using var image = Pix.LoadFromFile(localPath);
                using var page = engine.Process(image);

                var text = page.GetText() ?? string.Empty;
                var confidence = (decimal)page.GetMeanConfidence() * 100m;

                if (tempBytes != null && File.Exists(localPath))
                {
                    File.Delete(localPath);
                }

                return (text.Trim(), Math.Round(confidence, 2));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Local OCR failed for {FileUrl}", fileUrl);
                return (string.Empty, 0m);
            }
        }
    }
}
