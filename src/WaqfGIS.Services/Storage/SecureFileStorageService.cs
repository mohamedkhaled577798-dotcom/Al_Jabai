using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;

namespace WaqfGIS.Services.Storage;

/// <summary>
/// ════════════════════════════════════════════════════════════════
///  خدمة التخزين الآمن المركزية — SecureFileStorageService
/// ════════════════════════════════════════════════════════════════
///  • جميع الملفات تُخزَّن في D:\WakfData (قابل للتغيير)
///  • كل نوع في مجلد فرعي باسم يعكس وظيفته
///  • اسم كل ملف = UUID عشوائي (يمنع التكرار والتسلسل)
///  • جميع الملفات مشفّرة AES-256
///  • فك التشفير يتم داخل النظام فقط عند التقديم
/// ════════════════════════════════════════════════════════════════
/// </summary>
public class SecureFileStorageService
{
    private readonly FileStorageSettings  _settings;
    private readonly FileEncryptionService _encryption;
    private readonly FileExtensionContentTypeProvider _mimeProvider = new();

    public SecureFileStorageService(FileStorageSettings settings)
    {
        _settings   = settings;
        _encryption = new FileEncryptionService(settings.EncryptionKey);
        EnsureRootDirectories();
    }

    // =================== رفع ملف واحد ===================
    /// <summary>
    /// رفع ملف مع تشفير كامل
    /// </summary>
    /// <param name="file">الملف المرفوع</param>
    /// <param name="subFolderKey">مفتاح المجلد الفرعي من appsettings (مثل "MosqueImages")</param>
    /// <param name="allowedExtensions">ملحقات مسموحة (null = كل شيء)</param>
    /// <returns>معلومات الملف المحفوظ</returns>
    public async Task<StoredFileInfo> SaveFileAsync(
        IFormFile file,
        string subFolderKey,
        string[]? allowedExtensions = null)
    {
        ValidateFile(file, allowedExtensions);

        var folderPath = _settings.GetFolderPath(subFolderKey);
        Directory.CreateDirectory(folderPath);

        // اسم الملف = UUID (بدون امتداد — الامتداد غير ضروري للملفات المشفرة)
        var uid      = Guid.NewGuid().ToString("N"); // 32 حرف بدون شرطات
        var ext      = Path.GetExtension(file.FileName).ToLower();
        // نحتفظ بالامتداد مشفوراً داخل البيانات الوصفية فقط
        var diskName = uid; // بدون امتداد — يمنع التخمين

        var fullPath = Path.Combine(folderPath, diskName);

        using var stream = file.OpenReadStream();
        await _encryption.EncryptFileAsync(stream, fullPath);

        _mimeProvider.TryGetContentType(file.FileName, out var mime);

        return new StoredFileInfo
        {
            Uid          = uid,
            OriginalName = file.FileName,
            DiskPath     = fullPath,
            SubFolder    = subFolderKey,
            Extension    = ext,
            MimeType     = mime ?? file.ContentType,
            FileSize     = file.Length,
            StoredAt     = DateTime.UtcNow
        };
    }

    // =================== رفع عدة ملفات ===================
    public async Task<List<StoredFileInfo>> SaveFilesAsync(
        IEnumerable<IFormFile> files,
        string subFolderKey,
        string[]? allowedExtensions = null)
    {
        var results = new List<StoredFileInfo>();
        foreach (var file in files.Where(f => f.Length > 0))
            results.Add(await SaveFileAsync(file, subFolderKey, allowedExtensions));
        return results;
    }

    // =================== قراءة ملف (فك التشفير) ===================
    /// <summary>
    /// فك تشفير الملف وإرجاعه كـ byte[]
    /// يُستخدم في Controller لإرسال الملف للمستخدم
    /// </summary>
    public async Task<(byte[] Data, string MimeType)> ReadFileAsync(
        string diskPath, string mimeType = "application/octet-stream")
    {
        var data = await _encryption.DecryptFileToBytesAsync(diskPath);
        return (data, mimeType);
    }

    public async Task<MemoryStream> ReadFileStreamAsync(string diskPath)
        => await _encryption.DecryptFileToStreamAsync(diskPath);

    // =================== حذف ملف ===================
    public void DeleteFile(string diskPath)
    {
        if (!string.IsNullOrEmpty(diskPath) && File.Exists(diskPath))
            File.Delete(diskPath);
    }

    // =================== استيراد من Stream (للاستخدام في ExcelImportService) ===================
    public async Task<StoredFileInfo> SaveFromStreamAsync(
        Stream stream, string originalName, string subFolderKey, string mimeType = "")
    {
        var folderPath = _settings.GetFolderPath(subFolderKey);
        Directory.CreateDirectory(folderPath);

        var uid      = Guid.NewGuid().ToString("N");
        var ext      = Path.GetExtension(originalName).ToLower();
        var fullPath = Path.Combine(folderPath, uid);

        await _encryption.EncryptFileAsync(stream, fullPath);
        _mimeProvider.TryGetContentType(originalName, out var detectedMime);

        return new StoredFileInfo
        {
            Uid          = uid,
            OriginalName = originalName,
            DiskPath     = fullPath,
            SubFolder    = subFolderKey,
            Extension    = ext,
            MimeType     = mimeType.Length > 0 ? mimeType : (detectedMime ?? "application/octet-stream"),
            FileSize     = stream.Length,
            StoredAt     = DateTime.UtcNow
        };
    }

    // =================== المسار الكامل من UID ===================
    public string GetDiskPath(string subFolderKey, string uid)
        => Path.Combine(_settings.GetFolderPath(subFolderKey), uid);

    // =================== معلومات التخزين ===================
    public FileStorageSettings Settings => _settings;

    // =================== تهيئة المجلدات ===================
    private void EnsureRootDirectories()
    {
        Directory.CreateDirectory(_settings.RootPath);
        foreach (var sub in _settings.SubFolders.Values)
            Directory.CreateDirectory(Path.Combine(_settings.RootPath, sub));
    }

    // =================== التحقق من الملف ===================
    private static void ValidateFile(IFormFile file, string[]? allowed)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("الملف فارغ أو غير موجود");

        if (allowed != null && allowed.Length > 0)
        {
            var ext = Path.GetExtension(file.FileName).ToLower();
            if (!allowed.Contains(ext))
                throw new InvalidOperationException(
                    $"نوع الملف '{ext}' غير مسموح. المسموح: {string.Join(", ", allowed)}");
        }

        // حد أقصى 50MB
        if (file.Length > 50 * 1024 * 1024)
            throw new InvalidOperationException("حجم الملف يتجاوز الحد الأقصى (50MB)");
    }
}

// =================== نموذج معلومات الملف ===================
public class StoredFileInfo
{
    /// <summary>المعرف الفريد = اسم الملف على القرص</summary>
    public string Uid          { get; set; } = "";
    /// <summary>الاسم الأصلي قبل الرفع</summary>
    public string OriginalName { get; set; } = "";
    /// <summary>المسار الكامل على القرص (للاستخدام الداخلي فقط)</summary>
    public string DiskPath     { get; set; } = "";
    /// <summary>مفتاح المجلد الفرعي</summary>
    public string SubFolder    { get; set; } = "";
    /// <summary>الامتداد (للحفظ في قاعدة البيانات)</summary>
    public string Extension    { get; set; } = "";
    public string MimeType     { get; set; } = "";
    public long   FileSize     { get; set; }
    public DateTime StoredAt   { get; set; }

    /// <summary>
    /// المسار الافتراضي المحفوظ في DB:
    /// SubFolder + "/" + Uid (بدون مسار القرص الكامل)
    /// </summary>
    public string DbPath => $"{SubFolder}/{Uid}";
}
