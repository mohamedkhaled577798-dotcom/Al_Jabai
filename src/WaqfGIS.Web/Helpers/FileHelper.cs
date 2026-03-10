namespace WaqfGIS.Web.Helpers;

/// <summary>
/// مساعد لبناء URLs الملفات المشفّرة
/// استخدم FileHelper.Url(dbPath, ext) في أي View
/// </summary>
public static class FileHelper
{
    /// <summary>
    /// بناء URL لعرض ملف مشفَّر عبر FilesController
    /// DbPath المحفوظ في DB: "SubFolder/uid"
    /// </summary>
    public static string Url(string? dbPath, string ext = ".jpg", bool download = false)
    {
        if (string.IsNullOrEmpty(dbPath)) return "/img/no-image.png";
        var parts  = dbPath.Split('/');
        var folder = parts.Length > 1 ? parts[0] : "MosqueImages";
        var uid    = parts.Length > 1 ? parts[1] : parts[0];
        var dl     = download ? "true" : "false";
        return $"/Files/Get?folder={Uri.EscapeDataString(folder)}&uid={uid}&ext={Uri.EscapeDataString(ext)}&dl={dl}";
    }

    public static string ImageUrl(string? dbPath, string ext = ".jpg") => Url(dbPath, ext, false);
    public static string DownloadUrl(string? dbPath, string ext = ".pdf") => Url(dbPath, ext, true);
}
