using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WaqfGIS.Core.Entities;

namespace WaqfGIS.Services;

/// <summary>
/// خدمة توليد تقارير PDF رسمية باستخدام QuestPDF
/// </summary>
public class PdfReportService
{
    // ألوان ثابتة كـ string
    private const string PrimaryBlue   = "#1e3a8a";
    private const string LightBlue     = "#f0f4ff";
    private const string LightGreen    = "#065f46";
    private const string LightYellow   = "#fffbeb";
    private const string AmberBorder   = "#f59e0b";
    private const string RowAlt        = "#f9fafb";
    private const string White         = "#ffffff";
    private const string GrayText      = "#6b7280";
    private const string GrayBorder    = "#e5e7eb";

    static PdfReportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    // =================== بطاقة المسجد ===================
    public byte[] GenerateMosqueCard(Mosque mosque)
    {
        return Document.Create(container => container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(2, Unit.Centimetre);
            page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(11));
            page.ContentFromRightToLeft();

            page.Header().Element(c => PageHeader(c, "بطاقة مسجد وقفي", "نظام إدارة الأوقاف السنية"));

            page.Content().Column(col =>
            {
                col.Spacing(8);

                col.Item().Element(c => InfoSection(c, "البيانات الأساسية", new[]
                {
                    ("الاسم بالعربية", mosque.NameAr),
                    ("الاسم بالإنجليزية", mosque.NameEn ?? "—"),
                    ("الكود", mosque.Code),
                    ("النوع", mosque.MosqueType?.NameAr ?? "—"),
                    ("الحالة", mosque.MosqueStatus?.NameAr ?? "—"),
                    ("صلاة الجمعة", mosque.HasFridayPrayer ? "نعم" : "لا"),
                }));

                col.Item().Element(c => InfoSection(c, "الموقع الجغرافي", new[]
                {
                    ("المحافظة", mosque.Province?.NameAr ?? "—"),
                    ("الدائرة الوقفية", mosque.WaqfOffice?.NameAr ?? "—"),
                    ("العنوان", mosque.Address ?? "—"),
                    ("الحي", mosque.Neighborhood ?? "—"),
                    ("الإحداثيات", mosque.Location != null
                        ? $"{mosque.Location.Y:F6}° شمال،  {mosque.Location.X:F6}° شرق" : "—"),
                }));

                col.Item().Element(c => InfoSection(c, "بيانات الإمام", new[]
                {
                    ("اسم الإمام", mosque.ImamName ?? "—"),
                    ("هاتف الإمام", mosque.ImamPhone ?? "—"),
                    ("السعة", mosque.Capacity?.ToString() ?? "—"),
                    ("مساحة المبنى م²", mosque.AreaSqm?.ToString("N0") ?? "—"),
                }));

                if (!string.IsNullOrEmpty(mosque.WaqifName) || !string.IsNullOrEmpty(mosque.WaqfConditionText))
                {
                    col.Item().Element(c => InfoSection(c, "بيانات الواقف", new[]
                    {
                        ("اسم الواقف", mosque.WaqifName ?? "—"),
                        ("رقم صك الوقف", mosque.WaqfDocumentNumber ?? "—"),
                        ("تاريخ الصك", mosque.WaqfDocumentDate?.ToString("dd/MM/yyyy") ?? "—"),
                        ("شرط الواقف", mosque.WaqfConditionText ?? "—"),
                    }));
                }

                if (!string.IsNullOrEmpty(mosque.Notes))
                    col.Item().Element(c => NoteSection(c, mosque.Notes!));
            });

            page.Footer().Element(c => PageFooter(c));
        })).GeneratePdf();
    }

    // =================== بطاقة العقار ===================
    public byte[] GeneratePropertyCard(WaqfProperty property)
    {
        return Document.Create(container => container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(2, Unit.Centimetre);
            page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(11));
            page.ContentFromRightToLeft();

            page.Header().Element(c => PageHeader(c, "بطاقة عقار وقفي", "نظام إدارة الأوقاف السنية"));

            page.Content().Column(col =>
            {
                col.Spacing(8);

                col.Item().Element(c => InfoSection(c, "البيانات الأساسية", new[]
                {
                    ("الاسم", property.NameAr),
                    ("الكود", property.Code),
                    ("النوع", property.PropertyType?.NameAr ?? "—"),
                    ("الاستخدام", property.UsageType?.NameAr ?? "—"),
                    ("حالة الإيجار", property.RentalStatus ?? "—"),
                    ("رقم الصك", property.DeedNumber ?? "—"),
                }));

                col.Item().Element(c => InfoSection(c, "الموقع", new[]
                {
                    ("المحافظة", property.Province?.NameAr ?? "—"),
                    ("الدائرة الوقفية", property.WaqfOffice?.NameAr ?? "—"),
                    ("العنوان", property.Address ?? "—"),
                    ("الحي", property.Neighborhood ?? "—"),
                    ("الإحداثيات", property.Location != null
                        ? $"{property.Location.Y:F6}°N،  {property.Location.X:F6}°E" : "—"),
                }));

                col.Item().Element(c => InfoSection(c, "البيانات المالية", new[]
                {
                    ("المساحة الإجمالية م²", property.AreaSqm?.ToString("N0") ?? "—"),
                    ("القيمة التقديرية د.ع", property.EstimatedValue?.ToString("N0") ?? "—"),
                    ("الإيجار الشهري د.ع", property.MonthlyRent?.ToString("N0") ?? "—"),
                    ("المستأجر", property.TenantName ?? "—"),
                }));

                if (!string.IsNullOrEmpty(property.WaqifName))
                {
                    col.Item().Element(c => InfoSection(c, "بيانات الواقف", new[]
                    {
                        ("اسم الواقف", property.WaqifName ?? "—"),
                        ("رقم صك الوقف", property.WaqfDocumentNumber ?? "—"),
                        ("تاريخ الصك", property.WaqfDocumentDate?.ToString("dd/MM/yyyy") ?? "—"),
                    }));
                }

                if (!string.IsNullOrEmpty(property.Notes))
                    col.Item().Element(c => NoteSection(c, property.Notes!));
            });

            page.Footer().Element(c => PageFooter(c));
        })).GeneratePdf();
    }

    // =================== تقرير محافظة ===================
    public byte[] GenerateProvinceReport(string provinceName,
        IEnumerable<Mosque> mosques, IEnumerable<WaqfProperty> properties)
    {
        var ml = mosques.ToList();
        var pl = properties.ToList();

        return Document.Create(container => container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(2, Unit.Centimetre);
            page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10));
            page.ContentFromRightToLeft();

            page.Header().Element(c => PageHeader(c, $"تقرير محافظة {provinceName}", "نظام إدارة الأوقاف السنية"));

            page.Content().Column(col =>
            {
                col.Spacing(10);

                // ملخص أرقام
                col.Item().Background(PrimaryBlue).Padding(10).Row(row =>
                {
                    SummaryBox(row, "المساجد", ml.Count.ToString());
                    SummaryBox(row, "العقارات", pl.Count.ToString());
                    SummaryBox(row, "الطاقة الاستيعابية", ml.Sum(m => m.Capacity ?? 0).ToString("N0"));
                    SummaryBox(row, "قيمة العقارات د.ع", pl.Sum(p => p.EstimatedValue ?? 0).ToString("N0"));
                });

                if (ml.Any())
                {
                    col.Item().PaddingTop(8).Text("قائمة المساجد والجوامع").Bold().FontSize(12).FontColor(PrimaryBlue);
                    col.Item().Element(c => MosqueTable(c, ml));
                }

                if (pl.Any())
                {
                    col.Item().PaddingTop(8).Text("قائمة العقارات الوقفية").Bold().FontSize(12).FontColor(PrimaryBlue);
                    col.Item().Element(c => PropertyTable(c, pl));
                }
            });

            page.Footer().Element(c => PageFooter(c));
        })).GeneratePdf();
    }

    // =================== تقرير صيانة ===================
    public byte[] GenerateMaintenanceReport(IEnumerable<MaintenanceRecord> records)
    {
        var list = records.ToList();

        return Document.Create(container => container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(1.5f, Unit.Centimetre);
            page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(9));
            page.ContentFromRightToLeft();

            page.Header().Element(c => PageHeader(c, "تقرير سجل الصيانة", "نظام إدارة الأوقاف السنية"));

            page.Content().Column(col =>
            {
                col.Spacing(6);

                col.Item().Background(LightGreen).Padding(8).Row(row =>
                {
                    SummaryBox(row, "الإجمالي", list.Count.ToString());
                    SummaryBox(row, "مكتملة", list.Count(m => m.Status == "مكتملة").ToString());
                    SummaryBox(row, "متأخرة", list.Count(m => m.Status == "متأخرة").ToString());
                    SummaryBox(row, "التكلفة الفعلية",
                        list.Where(m => m.ActualCost.HasValue).Sum(m => m.ActualCost!.Value).ToString("N0") + " د.ع");
                });

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(3);
                        c.RelativeColumn(2);
                        c.RelativeColumn(2);
                        c.RelativeColumn(1.2f);
                        c.RelativeColumn(1.5f);
                        c.RelativeColumn(2);
                    });

                    // Header row
                    table.Header(h =>
                    {
                        foreach (var hdr in new[] { "العنوان", "الكيان", "النوع", "الحالة", "التاريخ", "التكلفة" })
                            h.Cell().Background(PrimaryBlue).Padding(4)
                             .Text(hdr).Bold().FontColor(White).FontSize(9);
                    });

                    foreach (var r in list)
                    {
                        table.Cell().BorderBottom(0.5f).BorderColor(GrayBorder).Padding(3).AlignRight().Text(r.Title).FontSize(8);
                        table.Cell().BorderBottom(0.5f).BorderColor(GrayBorder).Padding(3).AlignRight().Text(r.EntityName ?? "").FontSize(8);
                        table.Cell().BorderBottom(0.5f).BorderColor(GrayBorder).Padding(3).AlignRight().Text(r.MaintenanceType).FontSize(8);
                        table.Cell().BorderBottom(0.5f).BorderColor(GrayBorder).Padding(3).AlignCenter().Text(r.Status).FontSize(8);
                        table.Cell().BorderBottom(0.5f).BorderColor(GrayBorder).Padding(3).AlignCenter().Text(r.ScheduledDate.ToString("dd/MM/yy")).FontSize(8);
                        table.Cell().BorderBottom(0.5f).BorderColor(GrayBorder).Padding(3).AlignCenter()
                            .Text((r.ActualCost ?? r.EstimatedCost ?? 0).ToString("N0")).FontSize(8);
                    }
                });
            });

            page.Footer().Element(c => PageFooter(c));
        })).GeneratePdf();
    }

    // =================== Shared Components ===================

    private void PageHeader(IContainer container, string title, string subtitle)
    {
        container.Column(col =>
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text(subtitle).FontSize(9).FontColor(GrayText);
                    c.Item().Text(title).Bold().FontSize(16).FontColor(PrimaryBlue);
                    c.Item().Text($"تاريخ الإصدار: {DateTime.Now:dd/MM/yyyy  HH:mm}").FontSize(9).FontColor(GrayText);
                });
                row.ConstantItem(60).Height(55)
                   .Background(PrimaryBlue)
                   .AlignCenter().AlignMiddle()
                   .Text("وقف").Bold().FontSize(20).FontColor(White);
            });
            col.Item().PaddingTop(4).BorderBottom(2).BorderColor(PrimaryBlue);
        });
    }

    private void PageFooter(IContainer container)
    {
        container.BorderTop(1).BorderColor(GrayBorder).PaddingTop(5)
            .Row(row =>
            {
                row.RelativeItem()
                   .Text("نظام إدارة الأوقاف السنية — سري وللاستخدام الرسمي فقط")
                   .FontSize(8).FontColor(GrayText);
                row.RelativeItem().AlignLeft().Text(text =>
                {
                    text.Span("صفحة ").FontSize(8).FontColor(GrayText);
                    text.CurrentPageNumber().FontSize(8);
                    text.Span(" من ").FontSize(8).FontColor(GrayText);
                    text.TotalPages().FontSize(8);
                });
            });
    }

    private void InfoSection(IContainer container, string title, (string Key, string Value)[] items)
    {
        container.Column(col =>
        {
            col.Item().Background(PrimaryBlue).Padding(5)
               .Text(title).Bold().FontColor(White);
            col.Item().Table(t =>
            {
                t.ColumnsDefinition(c => { c.RelativeColumn(2); c.RelativeColumn(3); });
                foreach (var (key, value) in items)
                {
                    t.Cell().Background(LightBlue).BorderBottom(0.5f).BorderColor(GrayBorder)
                     .Padding(4).AlignRight().Text(key).Bold().FontSize(10);
                    t.Cell().BorderBottom(0.5f).BorderColor(GrayBorder)
                     .Padding(4).AlignRight().Text(value).FontSize(10);
                }
            });
        });
    }

    private void NoteSection(IContainer container, string text)
    {
        container.Background(LightYellow).Border(1).BorderColor(AmberBorder)
            .Padding(8).Text(t =>
            {
                t.Span("ملاحظات: ").Bold().FontSize(10);
                t.Span(text).FontSize(10);
            });
    }

    private void SummaryBox(RowDescriptor row, string label, string value)
    {
        row.RelativeItem().AlignCenter().Column(c =>
        {
            c.Item().AlignCenter().Text(value).Bold().FontSize(18).FontColor(White);
            c.Item().AlignCenter().Text(label).FontSize(9).FontColor(White);
        });
    }

    private void MosqueTable(IContainer container, List<Mosque> mosques)
    {
        container.Table(t =>
        {
            t.ColumnsDefinition(c =>
            {
                c.ConstantColumn(25);
                c.RelativeColumn(3);
                c.RelativeColumn(2);
                c.RelativeColumn(2);
                c.RelativeColumn(1.2f);
            });

            t.Header(h =>
            {
                foreach (var hdr in new[] { "#", "الاسم", "النوع", "الحالة", "الطاقة" })
                    h.Cell().Background(PrimaryBlue).Padding(4)
                     .Text(hdr).Bold().FontColor(White).FontSize(9);
            });

            int i = 1;
            foreach (var m in mosques)
            {
                var bg = i % 2 == 0 ? RowAlt : White;
                t.Cell().Background(bg).BorderBottom(0.5f).BorderColor(GrayBorder).Padding(3).AlignCenter().Text(i.ToString()).FontSize(9);
                t.Cell().Background(bg).BorderBottom(0.5f).BorderColor(GrayBorder).Padding(3).AlignRight().Text(m.NameAr).FontSize(9);
                t.Cell().Background(bg).BorderBottom(0.5f).BorderColor(GrayBorder).Padding(3).AlignRight().Text(m.MosqueType?.NameAr ?? "").FontSize(9);
                t.Cell().Background(bg).BorderBottom(0.5f).BorderColor(GrayBorder).Padding(3).AlignRight().Text(m.MosqueStatus?.NameAr ?? "").FontSize(9);
                t.Cell().Background(bg).BorderBottom(0.5f).BorderColor(GrayBorder).Padding(3).AlignCenter().Text((m.Capacity ?? 0).ToString()).FontSize(9);
                i++;
            }
        });
    }

    private void PropertyTable(IContainer container, List<WaqfProperty> props)
    {
        container.Table(t =>
        {
            t.ColumnsDefinition(c =>
            {
                c.ConstantColumn(25);
                c.RelativeColumn(3);
                c.RelativeColumn(2);
                c.RelativeColumn(1.5f);
                c.RelativeColumn(2);
            });

            t.Header(h =>
            {
                foreach (var hdr in new[] { "#", "الاسم", "النوع", "المساحة م²", "القيمة د.ع" })
                    h.Cell().Background(PrimaryBlue).Padding(4)
                     .Text(hdr).Bold().FontColor(White).FontSize(9);
            });

            int i = 1;
            foreach (var p in props)
            {
                var bg = i % 2 == 0 ? RowAlt : White;
                t.Cell().Background(bg).BorderBottom(0.5f).BorderColor(GrayBorder).Padding(3).AlignCenter().Text(i.ToString()).FontSize(9);
                t.Cell().Background(bg).BorderBottom(0.5f).BorderColor(GrayBorder).Padding(3).AlignRight().Text(p.NameAr).FontSize(9);
                t.Cell().Background(bg).BorderBottom(0.5f).BorderColor(GrayBorder).Padding(3).AlignRight().Text(p.PropertyType?.NameAr ?? "").FontSize(9);
                t.Cell().Background(bg).BorderBottom(0.5f).BorderColor(GrayBorder).Padding(3).AlignCenter().Text((p.AreaSqm ?? 0).ToString("N0")).FontSize(9);
                t.Cell().Background(bg).BorderBottom(0.5f).BorderColor(GrayBorder).Padding(3).AlignCenter().Text((p.EstimatedValue ?? 0).ToString("N0")).FontSize(9);
                i++;
            }
        });
    }
}
