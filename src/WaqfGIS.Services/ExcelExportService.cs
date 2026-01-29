using ClosedXML.Excel;
using WaqfGIS.Core.Entities;
using WaqfGIS.Core.Interfaces;

namespace WaqfGIS.Services;

/// <summary>
/// خدمة تصدير البيانات إلى Excel
/// </summary>
public class ExcelExportService
{
    private readonly IUnitOfWork _unitOfWork;

    public ExcelExportService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public byte[] ExportMosquesToExcel(IEnumerable<Mosque> mosques)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("المساجد");

        // Header
        worksheet.Cell(1, 1).Value = "الكود";
        worksheet.Cell(1, 2).Value = "الاسم بالعربية";
        worksheet.Cell(1, 3).Value = "الاسم بالإنجليزية";
        worksheet.Cell(1, 4).Value = "النوع";
        worksheet.Cell(1, 5).Value = "الحالة";
        worksheet.Cell(1, 6).Value = "المحافظة";
        worksheet.Cell(1, 7).Value = "الدائرة الوقفية";
        worksheet.Cell(1, 8).Value = "العنوان";
        worksheet.Cell(1, 9).Value = "السعة";
        worksheet.Cell(1, 10).Value = "المساحة";
        worksheet.Cell(1, 11).Value = "صلاة الجمعة";
        worksheet.Cell(1, 12).Value = "اسم الإمام";
        worksheet.Cell(1, 13).Value = "هاتف الإمام";
        worksheet.Cell(1, 14).Value = "خط العرض";
        worksheet.Cell(1, 15).Value = "خط الطول";
        worksheet.Cell(1, 16).Value = "تاريخ الإنشاء";
        worksheet.Cell(1, 17).Value = "أنشئ بواسطة";

        // Style header
        var headerRange = worksheet.Range(1, 1, 1, 17);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.DarkBlue;
        headerRange.Style.Font.FontColor = XLColor.White;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Data
        int row = 2;
        foreach (var m in mosques)
        {
            worksheet.Cell(row, 1).Value = m.Code;
            worksheet.Cell(row, 2).Value = m.NameAr;
            worksheet.Cell(row, 3).Value = m.NameEn ?? "";
            worksheet.Cell(row, 4).Value = m.MosqueType?.NameAr ?? "";
            worksheet.Cell(row, 5).Value = m.MosqueStatus?.NameAr ?? "";
            worksheet.Cell(row, 6).Value = m.Province?.NameAr ?? "";
            worksheet.Cell(row, 7).Value = m.WaqfOffice?.NameAr ?? "";
            worksheet.Cell(row, 8).Value = m.Address ?? "";
            worksheet.Cell(row, 9).Value = m.Capacity ?? 0;
            worksheet.Cell(row, 10).Value = m.AreaSqm ?? 0;
            worksheet.Cell(row, 11).Value = m.HasFridayPrayer ? "نعم" : "لا";
            worksheet.Cell(row, 12).Value = m.ImamName ?? "";
            worksheet.Cell(row, 13).Value = m.ImamPhone ?? "";
            worksheet.Cell(row, 14).Value = m.Location?.Y ?? 0;
            worksheet.Cell(row, 15).Value = m.Location?.X ?? 0;
            worksheet.Cell(row, 16).Value = m.CreatedAt.ToString("yyyy/MM/dd");
            worksheet.Cell(row, 17).Value = m.CreatedBy ?? "";
            row++;
        }

        worksheet.Columns().AdjustToContents();
        worksheet.RightToLeft = true;

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] ExportPropertiesToExcel(IEnumerable<WaqfProperty> properties)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("العقارات");

        // Header
        worksheet.Cell(1, 1).Value = "الكود";
        worksheet.Cell(1, 2).Value = "الاسم";
        worksheet.Cell(1, 3).Value = "النوع";
        worksheet.Cell(1, 4).Value = "الاستخدام";
        worksheet.Cell(1, 5).Value = "المحافظة";
        worksheet.Cell(1, 6).Value = "الدائرة الوقفية";
        worksheet.Cell(1, 7).Value = "العنوان";
        worksheet.Cell(1, 8).Value = "المساحة (م²)";
        worksheet.Cell(1, 9).Value = "القيمة التقديرية";
        worksheet.Cell(1, 10).Value = "حالة الإيجار";
        worksheet.Cell(1, 11).Value = "الإيجار الشهري";
        worksheet.Cell(1, 12).Value = "المستأجر";
        worksheet.Cell(1, 13).Value = "رقم الصك";
        worksheet.Cell(1, 14).Value = "خط العرض";
        worksheet.Cell(1, 15).Value = "خط الطول";
        worksheet.Cell(1, 16).Value = "تاريخ الإنشاء";
        worksheet.Cell(1, 17).Value = "أنشئ بواسطة";

        var headerRange = worksheet.Range(1, 1, 1, 17);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.DarkGreen;
        headerRange.Style.Font.FontColor = XLColor.White;

        int row = 2;
        foreach (var p in properties)
        {
            worksheet.Cell(row, 1).Value = p.Code;
            worksheet.Cell(row, 2).Value = p.NameAr;
            worksheet.Cell(row, 3).Value = p.PropertyType?.NameAr ?? "";
            worksheet.Cell(row, 4).Value = p.UsageType?.NameAr ?? "";
            worksheet.Cell(row, 5).Value = p.Province?.NameAr ?? "";
            worksheet.Cell(row, 6).Value = p.WaqfOffice?.NameAr ?? "";
            worksheet.Cell(row, 7).Value = p.Address ?? "";
            worksheet.Cell(row, 8).Value = p.AreaSqm ?? 0;
            worksheet.Cell(row, 9).Value = p.EstimatedValue ?? 0;
            worksheet.Cell(row, 10).Value = p.RentalStatus ?? "";
            worksheet.Cell(row, 11).Value = p.MonthlyRent ?? 0;
            worksheet.Cell(row, 12).Value = p.TenantName ?? "";
            worksheet.Cell(row, 13).Value = p.DeedNumber ?? "";
            worksheet.Cell(row, 14).Value = p.Location?.Y ?? 0;
            worksheet.Cell(row, 15).Value = p.Location?.X ?? 0;
            worksheet.Cell(row, 16).Value = p.CreatedAt.ToString("yyyy/MM/dd");
            worksheet.Cell(row, 17).Value = p.CreatedBy ?? "";
            row++;
        }

        worksheet.Columns().AdjustToContents();
        worksheet.RightToLeft = true;

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] ExportOfficesToExcel(IEnumerable<WaqfOffice> offices)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("الدوائر");

        worksheet.Cell(1, 1).Value = "الكود";
        worksheet.Cell(1, 2).Value = "الاسم";
        worksheet.Cell(1, 3).Value = "النوع";
        worksheet.Cell(1, 4).Value = "المحافظة";
        worksheet.Cell(1, 5).Value = "تابع لـ";
        worksheet.Cell(1, 6).Value = "العنوان";
        worksheet.Cell(1, 7).Value = "الهاتف";
        worksheet.Cell(1, 8).Value = "البريد";
        worksheet.Cell(1, 9).Value = "المدير";
        worksheet.Cell(1, 10).Value = "هاتف المدير";

        var headerRange = worksheet.Range(1, 1, 1, 10);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.Orange;
        headerRange.Style.Font.FontColor = XLColor.White;

        int row = 2;
        foreach (var o in offices)
        {
            worksheet.Cell(row, 1).Value = o.Code;
            worksheet.Cell(row, 2).Value = o.NameAr;
            worksheet.Cell(row, 3).Value = o.OfficeType?.NameAr ?? "";
            worksheet.Cell(row, 4).Value = o.Province?.NameAr ?? "";
            worksheet.Cell(row, 5).Value = o.ParentOffice?.NameAr ?? "";
            worksheet.Cell(row, 6).Value = o.Address ?? "";
            worksheet.Cell(row, 7).Value = o.Phone ?? "";
            worksheet.Cell(row, 8).Value = o.Email ?? "";
            worksheet.Cell(row, 9).Value = o.ManagerName ?? "";
            worksheet.Cell(row, 10).Value = o.ManagerPhone ?? "";
            row++;
        }

        worksheet.Columns().AdjustToContents();
        worksheet.RightToLeft = true;

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] ExportAllDataToExcel(IEnumerable<Mosque> mosques, IEnumerable<WaqfProperty> properties, IEnumerable<WaqfOffice> offices)
    {
        using var workbook = new XLWorkbook();

        // Sheet 1: Mosques
        var wsMosques = workbook.Worksheets.Add("المساجد");
        AddMosquesSheet(wsMosques, mosques);

        // Sheet 2: Properties  
        var wsProperties = workbook.Worksheets.Add("العقارات");
        AddPropertiesSheet(wsProperties, properties);

        // Sheet 3: Offices
        var wsOffices = workbook.Worksheets.Add("الدوائر");
        AddOfficesSheet(wsOffices, offices);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private void AddMosquesSheet(IXLWorksheet ws, IEnumerable<Mosque> data)
    {
        ws.Cell(1, 1).Value = "الكود";
        ws.Cell(1, 2).Value = "الاسم";
        ws.Cell(1, 3).Value = "النوع";
        ws.Cell(1, 4).Value = "المحافظة";
        ws.Cell(1, 5).Value = "السعة";
        ws.Cell(1, 6).Value = "خط العرض";
        ws.Cell(1, 7).Value = "خط الطول";

        var header = ws.Range(1, 1, 1, 7);
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.DarkBlue;
        header.Style.Font.FontColor = XLColor.White;

        int row = 2;
        foreach (var m in data)
        {
            ws.Cell(row, 1).Value = m.Code;
            ws.Cell(row, 2).Value = m.NameAr;
            ws.Cell(row, 3).Value = m.MosqueType?.NameAr ?? "";
            ws.Cell(row, 4).Value = m.Province?.NameAr ?? "";
            ws.Cell(row, 5).Value = m.Capacity ?? 0;
            ws.Cell(row, 6).Value = m.Location?.Y ?? 0;
            ws.Cell(row, 7).Value = m.Location?.X ?? 0;
            row++;
        }
        ws.Columns().AdjustToContents();
        ws.RightToLeft = true;
    }

    private void AddPropertiesSheet(IXLWorksheet ws, IEnumerable<WaqfProperty> data)
    {
        ws.Cell(1, 1).Value = "الكود";
        ws.Cell(1, 2).Value = "الاسم";
        ws.Cell(1, 3).Value = "النوع";
        ws.Cell(1, 4).Value = "المحافظة";
        ws.Cell(1, 5).Value = "المساحة";
        ws.Cell(1, 6).Value = "القيمة";
        ws.Cell(1, 7).Value = "خط العرض";
        ws.Cell(1, 8).Value = "خط الطول";

        var header = ws.Range(1, 1, 1, 8);
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.DarkGreen;
        header.Style.Font.FontColor = XLColor.White;

        int row = 2;
        foreach (var p in data)
        {
            ws.Cell(row, 1).Value = p.Code;
            ws.Cell(row, 2).Value = p.NameAr;
            ws.Cell(row, 3).Value = p.PropertyType?.NameAr ?? "";
            ws.Cell(row, 4).Value = p.Province?.NameAr ?? "";
            ws.Cell(row, 5).Value = p.AreaSqm ?? 0;
            ws.Cell(row, 6).Value = p.EstimatedValue ?? 0;
            ws.Cell(row, 7).Value = p.Location?.Y ?? 0;
            ws.Cell(row, 8).Value = p.Location?.X ?? 0;
            row++;
        }
        ws.Columns().AdjustToContents();
        ws.RightToLeft = true;
    }

    private void AddOfficesSheet(IXLWorksheet ws, IEnumerable<WaqfOffice> data)
    {
        ws.Cell(1, 1).Value = "الكود";
        ws.Cell(1, 2).Value = "الاسم";
        ws.Cell(1, 3).Value = "النوع";
        ws.Cell(1, 4).Value = "المحافظة";
        ws.Cell(1, 5).Value = "المدير";

        var header = ws.Range(1, 1, 1, 5);
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.Orange;
        header.Style.Font.FontColor = XLColor.White;

        int row = 2;
        foreach (var o in data)
        {
            ws.Cell(row, 1).Value = o.Code;
            ws.Cell(row, 2).Value = o.NameAr;
            ws.Cell(row, 3).Value = o.OfficeType?.NameAr ?? "";
            ws.Cell(row, 4).Value = o.Province?.NameAr ?? "";
            ws.Cell(row, 5).Value = o.ManagerName ?? "";
            row++;
        }
        ws.Columns().AdjustToContents();
        ws.RightToLeft = true;
    }
}
