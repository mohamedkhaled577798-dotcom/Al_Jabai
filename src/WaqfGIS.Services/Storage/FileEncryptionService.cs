using System.Security.Cryptography;
using System.Text;

namespace WaqfGIS.Services.Storage;

/// <summary>
/// خدمة التشفير وفك التشفير — AES-256-CBC
/// جميع الملفات المرفوعة تُخزَّن مشفّرة بالكامل
/// فك التشفير يتم داخل النظام فقط عند قراءة الملف
/// </summary>
public class FileEncryptionService
{
    private readonly byte[] _key;
    private const int KeySize  = 32; // 256 بت
    private const int IvSize   = 16; // 128 بت

    public FileEncryptionService(string rawKey)
    {
        // اشتقاق مفتاح 256-bit من النص باستخدام SHA-256
        using var sha = SHA256.Create();
        _key = sha.ComputeHash(Encoding.UTF8.GetBytes(rawKey));
    }

    // =================== تشفير ملف ===================
    /// <summary>
    /// تشفير stream وكتابته إلى outputPath
    /// البنية: [IV (16 bytes)] + [Encrypted Data]
    /// </summary>
    public async Task EncryptFileAsync(Stream inputStream, string outputPath)
    {
        using var aes = Aes.Create();
        aes.Key     = _key;
        aes.Mode    = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.GenerateIV();

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

        await using var outputFile = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
        // اكتب الـ IV في البداية
        await outputFile.WriteAsync(aes.IV, 0, IvSize);

        await using var cryptoStream = new CryptoStream(
            outputFile, aes.CreateEncryptor(), CryptoStreamMode.Write);

        await inputStream.CopyToAsync(cryptoStream);
        await cryptoStream.FlushFinalBlockAsync();
    }

    // =================== فك تشفير إلى Stream ===================
    /// <summary>
    /// فك تشفير الملف وإرجاعه كـ MemoryStream جاهز للقراءة
    /// يُستخدم عند تقديم الملف للمستخدم داخل النظام
    /// </summary>
    public async Task<MemoryStream> DecryptFileToStreamAsync(string encryptedPath)
    {
        if (!File.Exists(encryptedPath))
            throw new FileNotFoundException($"الملف المشفَّر غير موجود: {encryptedPath}");

        await using var inputFile = new FileStream(encryptedPath, FileMode.Open, FileAccess.Read);

        // اقرأ الـ IV من أول 16 بايت
        var iv = new byte[IvSize];
        var bytesRead = await inputFile.ReadAsync(iv, 0, IvSize);
        if (bytesRead != IvSize)
            throw new InvalidDataException("الملف تالف: لا يحتوي على IV صالح");

        using var aes = Aes.Create();
        aes.Key     = _key;
        aes.IV      = iv;
        aes.Mode    = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        var output = new MemoryStream();
        await using var cryptoStream = new CryptoStream(
            inputFile, aes.CreateDecryptor(), CryptoStreamMode.Read);

        await cryptoStream.CopyToAsync(output);
        output.Position = 0;
        return output;
    }

    // =================== فك تشفير إلى byte[] ===================
    public async Task<byte[]> DecryptFileToBytesAsync(string encryptedPath)
    {
        var stream = await DecryptFileToStreamAsync(encryptedPath);
        return stream.ToArray();
    }

    // =================== التحقق من سلامة الملف ===================
    public bool IsEncryptedFile(string filePath)
    {
        if (!File.Exists(filePath)) return false;
        return new FileInfo(filePath).Length > IvSize;
    }
}
