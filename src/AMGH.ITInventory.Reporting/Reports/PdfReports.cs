using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AMGH.ITInventory.Reporting.Reports;

public record ReportTable(string Title, string[] Columns, IReadOnlyList<string[]> Rows);

public interface IReportGenerator
{
    byte[] ToPdf(ReportTable table);
    byte[] ToExcel(ReportTable table);
    byte[] VerificationCertificate(string campaignName, string approvedBy, DateTime approvedUtc, int verified, int total, int exceptions);
}

public class ReportGenerator : IReportGenerator
{
    static ReportGenerator() => QuestPDF.Settings.License = LicenseType.Community;

    public byte[] ToPdf(ReportTable t) => Document.Create(c => c.Page(p =>
    {
        p.Size(PageSizes.A4.Landscape()); p.Margin(25); p.DefaultTextStyle(x => x.FontSize(9));
        p.Header().Text($"AMGH IT Asset Inventory - {t.Title}").FontSize(14).SemiBold();
        p.Content().PaddingTop(10).Table(tbl =>
        {
            tbl.ColumnsDefinition(cd => { foreach (var _ in t.Columns) cd.RelativeColumn(); });
            tbl.Header(h => { foreach (var col in t.Columns) h.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text(col).SemiBold(); });
            foreach (var row in t.Rows) foreach (var cell in row) tbl.Cell().BorderBottom(0.5f).Padding(3).Text(cell);
        });
        p.Footer().AlignRight().Text(x => { x.Span($"Generated {DateTime.UtcNow:u}  |  Page "); x.CurrentPageNumber(); });
    })).GeneratePdf();

    public byte[] ToExcel(ReportTable t)
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet(t.Title.Length > 31 ? t.Title[..31] : t.Title);
        for (int c = 0; c < t.Columns.Length; c++) { ws.Cell(1, c + 1).Value = t.Columns[c]; ws.Cell(1, c + 1).Style.Font.Bold = true; }
        for (int r = 0; r < t.Rows.Count; r++) for (int c = 0; c < t.Columns.Length; c++) ws.Cell(r + 2, c + 1).Value = t.Rows[r][c];
        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream(); wb.SaveAs(ms); return ms.ToArray();
    }

    public byte[] VerificationCertificate(string campaignName, string approvedBy, DateTime approvedUtc, int verified, int total, int exceptions) =>
        Document.Create(c => c.Page(p =>
        {
            p.Size(PageSizes.A4); p.Margin(50);
            p.Content().Column(col =>
            {
                col.Spacing(12);
                col.Item().AlignCenter().Text("Asset Verification Certificate").FontSize(24).Bold();
                col.Item().AlignCenter().Text("AMGH IT Asset Inventory").FontSize(12);
                col.Item().PaddingTop(20).Text($"Campaign: {campaignName}").FontSize(14);
                col.Item().Text($"Assets verified: {verified} of {total}");
                col.Item().Text($"Exceptions recorded: {exceptions}");
                col.Item().Text($"Approved by: {approvedBy} on {approvedUtc:yyyy-MM-dd}");
                col.Item().PaddingTop(40).Text("This certificate is generated from the audited verification record (ISO 27001 A.5.9).").Italic().FontSize(9);
            });
        })).GeneratePdf();
}
