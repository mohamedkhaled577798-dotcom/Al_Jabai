using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WaqfGIS.Services.Storage;

namespace WaqfGIS.Web.Controllers;

/// <summary>
/// Controller مخصص لتقديم الملفات المشفرة
/// الملفات لا تُخدَّم مباشرة من القرص — تمر عبر هذا الـ Controller فقط
/// يتحقق من صلاحية المستخدم قبل فك التشفير
/// </summary>
[Authorize]
public class FilesController : Controller
{
    private readonly SecureFileStorageService _storage;
    private readonly ILogger<FilesController> _logger;

    // خريطة الامتدادات للـ MIME types
    private static readonly Dictionary<string, string> MimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        { ".jpg",  "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".png",  "image/png"  },
        { ".gif",  "image/gif"  },
        { ".webp", "image/webp" },
        { ".pdf",  "application/pdf" },
        { ".doc",  "application/msword" },
        { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
        { ".xls",  "application/vnd.ms-excel" },
        { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
        { ".csv",  "text/csv" },
        { ".mp4",  "video/mp4" },
        { ".mp3",  "audio/mpeg" },
    };

    public FilesController(SecureFileStorageService storage, ILogger<FilesController> logger)
    {
        _storage = storage;
        _logger  = logger;
    }

    // =================== عرض/تنزيل ملف ===================
    /// <summary>
    /// GET /Files/Get?folder=MosqueImages&uid=abc123&ext=.jpg&dl=false
    /// folder = مفتاح المجلد الفرعي
    /// uid    = معرف الملف (اسمه على القرص)
    /// ext    = الامتداد الأصلي (للـ MIME type)
    /// name   = اسم التنزيل المعروض للمستخدم (اختياري)
    /// dl     = true لتنزيل، false لعرض inline
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Get(
        string folder, string uid, string ext = ".bin",
        string? name = null, bool dl = false)
    {
        try
        {
            var diskPath = _storage.GetDiskPath(folder, uid);

            if (!System.IO.File.Exists(diskPath))
            {
                _logger.LogWarning("ملف غير موجود: {Path}", diskPath);
                return NotFound("الملف غير موجود");
            }

            var (data, _) = await _storage.ReadFileAsync(diskPath);

            var mime        = MimeTypes.TryGetValue(ext, out var m) ? m : "application/octet-stream";
            var displayName = name ?? $"file{ext}";
            var cd          = dl ? "attachment" : "inline";

            Response.Headers["Content-Disposition"] = $"{cd}; filename=\"{Uri.EscapeDataString(displayName)}\"";
            Response.Headers["Cache-Control"]       = "private, max-age=3600";
            Response.Headers["X-Content-Type-Options"] = "nosniff";

            return File(data, mime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطأ في قراءة الملف: folder={Folder}, uid={Uid}", folder, uid);
            return StatusCode(500, "خطأ في تحميل الملف");
        }
    }

    // =================== صورة مصغّرة inline ===================
    /// <summary>
    /// GET /Files/Thumb?folder=MosqueImages&uid=abc123&ext=.jpg
    /// نفس Get لكن دائماً inline مع Cache طويل
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Thumb(string folder, string uid, string ext = ".jpg")
    {
        return await Get(folder, uid, ext, dl: false);
    }
}
