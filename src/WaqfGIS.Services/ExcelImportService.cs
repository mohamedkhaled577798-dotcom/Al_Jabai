using System.Data;
using System.Text;
using ExcelDataReader;
using WaqfGIS.Core.Entities;
using WaqfGIS.Core.Interfaces;

namespace WaqfGIS.Services;

/// <summary>
/// خدمة استيراد البيانات من Excel/CSV
/// </summary>
public class ExcelImportService
{
    private readonly IUnitOfWork _unitOfWork;

    public ExcelImportService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        // مطلوب لـ ExcelDataReader في .NET Core
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    // =================== استيراد مساجد ===================
    public async Task<ImportResult> ImportMosquesAsync(Stream fileStream, string fileName, int defaultOfficeId)
    {
        var result = new ImportResult { EntityType = "مساجد" };
        var rows = ReadExcel(fileStream, fileName);

        foreach (var row in rows)
        {
            try
            {
                var nameAr = GetString(row, "الاسم بالعربية") ?? GetString(row, "الاسم") ?? "";
                if (string.IsNullOrWhiteSpace(nameAr)) { result.Skipped++; continue; }

                // تحقق من التكرار
                bool exists = (await _unitOfWork.Mosques.GetAllAsync())
                    .Any(m => m.NameAr == nameAr.Trim());
                if (exists) { result.Duplicates++; result.Warnings.Add($"مكرر: {nameAr}"); continue; }

                var mosque = new Mosque
                {
                    NameAr        = nameAr.Trim(),
                    Code          = GetString(row, "الكود") ?? GenerateCode("MSQ"),
                    WaqfOfficeId  = defaultOfficeId,
                    Address       = GetString(row, "العنوان"),
                    Neighborhood  = GetString(row, "الحي"),
                    ImamName      = GetString(row, "اسم الإمام"),
                    ImamPhone     = GetString(row, "هاتف الإمام"),
                    HasFridayPrayer = GetBool(row, "صلاة الجمعة"),
                    Capacity      = GetInt(row, "السعة"),
                    Notes         = GetString(row, "ملاحظات"),
                    CreatedBy     = "Import"
                };

                // الإحداثيات
                var lat = GetDouble(row, "خط العرض") ?? GetDouble(row, "Latitude");
                var lng = GetDouble(row, "خط الطول") ?? GetDouble(row, "Longitude");
                if (lat.HasValue && lng.HasValue)
                    mosque.Location = new NetTopologySuite.Geometries.Point(lng.Value, lat.Value) { SRID = 4326 };

                // المحافظة
                var provName = GetString(row, "المحافظة");
                if (!string.IsNullOrEmpty(provName))
                {
                    var prov = (await _unitOfWork.Provinces.GetAllAsync())
                        .FirstOrDefault(p => p.NameAr.Contains(provName));
                    if (prov != null) mosque.ProvinceId = prov.Id;
                }

                await _unitOfWork.Mosques.AddAsync(mosque);
                result.Imported++;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"خطأ في السطر {result.Imported + result.Errors.Count + 2}: {ex.Message}");
                result.Failed++;
            }
        }

        if (result.Imported > 0)
            await _unitOfWork.SaveChangesAsync();

        return result;
    }

    // =================== استيراد عقارات ===================
    public async Task<ImportResult> ImportPropertiesAsync(Stream fileStream, string fileName, int defaultOfficeId)
    {
        var result = new ImportResult { EntityType = "عقارات" };
        var rows = ReadExcel(fileStream, fileName);

        foreach (var row in rows)
        {
            try
            {
                var nameAr = GetString(row, "الاسم") ?? GetString(row, "الاسم بالعربية") ?? "";
                if (string.IsNullOrWhiteSpace(nameAr)) { result.Skipped++; continue; }

                bool exists = (await _unitOfWork.WaqfProperties.GetAllAsync())
                    .Any(p => p.NameAr == nameAr.Trim());
                if (exists) { result.Duplicates++; result.Warnings.Add($"مكرر: {nameAr}"); continue; }

                var prop = new WaqfProperty
                {
                    NameAr       = nameAr.Trim(),
                    Code         = GetString(row, "الكود") ?? GenerateCode("PROP"),
                    WaqfOfficeId = defaultOfficeId,
                    Address      = GetString(row, "العنوان"),
                    Neighborhood = GetString(row, "الحي"),
                    AreaSqm      = GetDecimal(row, "المساحة") ?? GetDecimal(row, "المساحة م²"),
                    EstimatedValue = GetDecimal(row, "القيمة") ?? GetDecimal(row, "القيمة التقديرية"),
                    RentalStatus = GetString(row, "حالة الإيجار"),
                    TenantName   = GetString(row, "المستأجر"),
                    MonthlyRent  = GetDecimal(row, "الإيجار الشهري"),
                    DeedNumber   = GetString(row, "رقم الصك"),
                    Notes        = GetString(row, "ملاحظات"),
                    CreatedBy    = "Import"
                };

                var lat = GetDouble(row, "خط العرض") ?? GetDouble(row, "Latitude");
                var lng = GetDouble(row, "خط الطول") ?? GetDouble(row, "Longitude");
                if (lat.HasValue && lng.HasValue)
                    prop.Location = new NetTopologySuite.Geometries.Point(lng.Value, lat.Value) { SRID = 4326 };

                var provName = GetString(row, "المحافظة");
                if (!string.IsNullOrEmpty(provName))
                {
                    var prov = (await _unitOfWork.Provinces.GetAllAsync())
                        .FirstOrDefault(p => p.NameAr.Contains(provName));
                    if (prov != null) prop.ProvinceId = prov.Id;
                }

                await _unitOfWork.WaqfProperties.AddAsync(prop);
                result.Imported++;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"خطأ في السطر {result.Imported + result.Failed + 2}: {ex.Message}");
                result.Failed++;
            }
        }

        if (result.Imported > 0)
            await _unitOfWork.SaveChangesAsync();

        return result;
    }

    // =================== تحليل الملف (معاينة) ===================
    public ImportPreview PreviewFile(Stream fileStream, string fileName)
    {
        var rows = ReadExcel(fileStream, fileName);
        var preview = new ImportPreview();
        preview.Headers = rows.FirstOrDefault()?.Keys.ToList() ?? new();
        preview.TotalRows = rows.Count;
        preview.SampleRows = rows.Take(5).ToList();
        return preview;
    }

    // =================== قراءة Excel/CSV ===================
    private List<Dictionary<string, string?>> ReadExcel(Stream stream, string fileName)
    {
        var rows = new List<Dictionary<string, string?>>();
        var ext  = Path.GetExtension(fileName).ToLower();

        if (ext == ".csv")
        {
            using var reader = new StreamReader(stream, Encoding.UTF8);
            string? headerLine = reader.ReadLine();
            if (headerLine == null) return rows;

            var headers = headerLine.Split(',').Select(h => h.Trim('"', ' ')).ToArray();
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;
                var values = line.Split(',');
                var row = new Dictionary<string, string?>();
                for (int i = 0; i < headers.Length; i++)
                    row[headers[i]] = i < values.Length ? values[i].Trim('"', ' ') : null;
                rows.Add(row);
            }
        }
        else // .xlsx / .xls
        {
            using var reader = ExcelReaderFactory.CreateReader(stream);
            var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true }
            });

            var table = dataSet.Tables[0];
            foreach (DataRow row in table.Rows)
            {
                var dict = new Dictionary<string, string?>();
                foreach (DataColumn col in table.Columns)
                    dict[col.ColumnName] = row[col]?.ToString();
                if (dict.Values.Any(v => !string.IsNullOrWhiteSpace(v)))
                    rows.Add(dict);
            }
        }
        return rows;
    }

    // =================== Helpers ===================
    private string? GetString(Dictionary<string, string?> row, string key)
        => row.TryGetValue(key, out var v) ? (string.IsNullOrWhiteSpace(v) ? null : v.Trim()) : null;

    private int? GetInt(Dictionary<string, string?> row, string key)
        => int.TryParse(GetString(row, key), out var v) ? v : null;

    private double? GetDouble(Dictionary<string, string?> row, string key)
        => double.TryParse(GetString(row, key), System.Globalization.NumberStyles.Any,
           System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : null;

    private decimal? GetDecimal(Dictionary<string, string?> row, string key)
        => decimal.TryParse(GetString(row, key), System.Globalization.NumberStyles.Any,
           System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : null;

    private bool GetBool(Dictionary<string, string?> row, string key)
    {
        var v = GetString(row, key)?.ToLower();
        return v == "نعم" || v == "yes" || v == "true" || v == "1";
    }

    private static string GenerateCode(string prefix)
        => $"{prefix}-{DateTime.Now:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}";
}

public class ImportResult
{
    public string  EntityType { get; set; } = "";
    public int     Imported   { get; set; }
    public int     Skipped    { get; set; }
    public int     Duplicates { get; set; }
    public int     Failed     { get; set; }
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors   { get; set; } = new();
    public bool Success => Failed == 0;
}

public class ImportPreview
{
    public List<string> Headers    { get; set; } = new();
    public int          TotalRows  { get; set; }
    public List<Dictionary<string, string?>> SampleRows { get; set; } = new();
}
