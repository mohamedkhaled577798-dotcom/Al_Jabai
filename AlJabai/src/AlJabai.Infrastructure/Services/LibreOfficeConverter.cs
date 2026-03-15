using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AlJabai.Infrastructure.Services;

public interface IDocxToPdfConverter
{
    Task<byte[]> ConvertAsync(byte[] docxBytes, CancellationToken ct = default);
}

public class LibreOfficeConverter : IDocxToPdfConverter
{
    private readonly ILogger<LibreOfficeConverter> _logger;
    private readonly string _libreOfficePath;

    public LibreOfficeConverter(IConfiguration configuration, ILogger<LibreOfficeConverter> logger)
    {
        _logger = logger;
        _libreOfficePath = configuration["LibreOffice:ExecutablePath"] ?? "libreoffice";
    }

    public async Task<byte[]> ConvertAsync(byte[] docxBytes, CancellationToken ct = default)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "pms_conversions", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        var inputPath = Path.Combine(tempDir, "input.docx");
        var outputPath = Path.Combine(tempDir, "input.pdf");

        try
        {
            await File.WriteAllBytesAsync(inputPath, docxBytes, ct);

            using var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = _libreOfficePath,
                    Arguments = $"--headless --convert-to pdf --outdir \"{tempDir}\" \"{inputPath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.WaitForExitAsync(ct);

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            if (!File.Exists(outputPath))
            {
                _logger.LogError("LibreOffice conversion failed. ExitCode={ExitCode}. Output={Output}. Error={Error}", process.ExitCode, output, error);
                throw new InvalidOperationException("LibreOffice conversion failed. Ensure LibreOffice is installed and configured.");
            }

            return await File.ReadAllBytesAsync(outputPath, ct);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }
}
