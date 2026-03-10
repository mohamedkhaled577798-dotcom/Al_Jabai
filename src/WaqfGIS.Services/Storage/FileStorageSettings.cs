namespace WaqfGIS.Services.Storage;

/// <summary>
/// إعدادات التخزين — تُقرأ من appsettings.json قسم "FileStorage"
/// ════════════════════════════════════════════════════════
///  لتغيير مسار التخزين: غيّر RootPath في appsettings.json فقط
/// ════════════════════════════════════════════════════════
/// </summary>
public class FileStorageSettings
{
    public string RootPath      { get; set; } = @"D:\WakfData";
    public string EncryptionKey { get; set; } = "";
    public Dictionary<string, string> SubFolders { get; set; } = new();

    /// <summary>المسار الكامل لمجلد فرعي معيّن</summary>
    public string GetFolderPath(string subFolderKey)
    {
        var sub = SubFolders.TryGetValue(subFolderKey, out var name) ? name : subFolderKey.ToLower();
        return Path.Combine(RootPath, sub);
    }
}
