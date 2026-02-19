using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iTextDocument = iText.Layout.Document;
using iTextCell = iText.Layout.Element.Cell;
using iTextTable = iText.Layout.Element.Table;
using iTextParagraph = iText.Layout.Element.Paragraph;
using iTextTextAlignment = iText.Layout.Properties.TextAlignment;
using iTextBorder = iText.Layout.Borders.Border;
using iTextColor = iText.Kernel.Colors.Color;
using iTextVerticalAlignment = iText.Layout.Properties.VerticalAlignment;
using iText.Layout.Properties;
using iText.IO.Image;
using iTextImage = iText.Layout.Element.Image;
using frontend.Models;

namespace frontend.Pages;

public partial class PayrollReceiptPage : ContentPage
{
    private Payroll? payroll;
    private List<PayrollActivityEntry>? activityEntries;

    public PayrollReceiptPage(string html)
    {
        InitializeComponent();
        var source = new HtmlWebViewSource { Html = html };
        ReceiptWebView.Source = source;
    }

    public PayrollReceiptPage(string html, Payroll payrollData, List<PayrollActivityEntry> entries)
    {
        InitializeComponent();
        payroll = payrollData;
        activityEntries = entries;
        var source = new HtmlWebViewSource { Html = html };
        ReceiptWebView.Source = source;
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private string BuildPdfFilePath()
    {
        var safeWorkerName = string.Join("_", (payroll?.WorkerName ?? "").Split(Path.GetInvalidFileNameChars()));
        var fileName = $"Recibo_{safeWorkerName}_{payroll?.WeekStart:yyyy-MM-dd}.pdf";
        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var filePath = Path.Combine(documentsPath, fileName);

        if (File.Exists(filePath))
        {
            try
            {
                File.Delete(filePath);
            }
            catch
            {
                fileName = $"Recibo_{safeWorkerName}_{payroll?.WeekStart:yyyy-MM-dd}_{DateTime.Now:HHmmss}.pdf";
                filePath = Path.Combine(documentsPath, fileName);
            }
        }

        return filePath;
    }

    private async void OnDownloadClicked(object sender, EventArgs e)
    {
        try
        {
            if (payroll == null)
            {
                await DisplayAlertAsync("Error", "No hay datos de nómina para descargar", "OK");
                return;
            }

            var filePath = BuildPdfFilePath();
            GenerateReceiptPdf(filePath, payroll, activityEntries);

            if (!File.Exists(filePath))
            {
                await DisplayAlertAsync("Error", "No se pudo generar el archivo PDF", "OK");
                return;
            }

            var openFile = await DisplayAlertAsync("Éxito", $"Recibo guardado en:\n{filePath}\n\n¿Desea abrirlo?", "Abrir", "Cerrar");

            if (openFile)
            {
#if WINDOWS
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
#else
                await Launcher.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile(filePath)
                });
#endif
            }
        }
        catch (Exception ex)
        {
            var detail = ex.InnerException != null
                ? $"{ex.Message}\nCausa: {ex.InnerException.Message}"
                : ex.Message;
            await DisplayAlertAsync("Error", $"No se pudo descargar el recibo: {detail}", "OK");
        }
    }

    public static void GenerateReceiptPdf(string filePath, Payroll payroll, List<PayrollActivityEntry>? activityEntries)
    {
        PdfWriter? writer = null;
        PdfDocument? pdf = null;
        iTextDocument? document = null;

        try
        {
            writer = new PdfWriter(filePath);
            pdf = new PdfDocument(writer);
            pdf.SetDefaultPageSize(new iText.Kernel.Geom.PageSize(612f, 792f));
            document = new iTextDocument(pdf);
            document.SetMargins(36, 36, 36, 36);

            var borderColor = new DeviceRgb(31, 44, 39);
            var headerBgColor = new DeviceRgb(244, 242, 237);
            var negativeColor = new DeviceRgb(192, 88, 88);
            var footerColor = new DeviceRgb(91, 106, 100);
            var thinBorder = new SolidBorder(borderColor, 0.5f);

            // ── Header ──
            var headerTable = new iTextTable(UnitValue.CreatePointArray([36, 200, 116]))
                .UseAllAvailableWidth()
                .SetBorder(iTextBorder.NO_BORDER);

            var logoPath = FindLogoPath();

            var logoCell = new iTextCell()
                .SetBorder(iTextBorder.NO_BORDER)
                .SetVerticalAlignment(iTextVerticalAlignment.MIDDLE);
            if (logoPath != null)
            {
                var logoData = ImageDataFactory.Create(logoPath);
                var logoImg = new iTextImage(logoData).ScaleToFit(30, 30);
                logoCell.Add(logoImg);
            }
            else
            {
                logoCell.Add(new iTextParagraph("SC").SetBold().SetFontSize(10).SetTextAlignment(iTextTextAlignment.CENTER));
                logoCell.SetBorder(new SolidBorder(borderColor, 1));
            }
            headerTable.AddCell(logoCell);

            headerTable.AddCell(new iTextCell()
                .Add(new iTextParagraph("FINCA BANANERA SANTA CECILIA").SetBold().SetFontSize(10))
                .Add(new iTextParagraph("Sistema de Gestión de Nómina").SetFontSize(7)
                    .SetFontColor(new DeviceRgb(60, 75, 69)))
                .SetBorder(iTextBorder.NO_BORDER)
                .SetVerticalAlignment(iTextVerticalAlignment.MIDDLE)
                .SetPaddingLeft(8));

            headerTable.AddCell(new iTextCell()
                .Add(new iTextParagraph($"Fecha de emision: {DateTime.Now:dd MMM yyyy}").SetFontSize(7)
                    .SetFontColor(footerColor))
                .Add(new iTextParagraph($"ID: {payroll.Id}").SetFontSize(7)
                    .SetFontColor(footerColor))
                .SetBorder(iTextBorder.NO_BORDER)
                .SetVerticalAlignment(iTextVerticalAlignment.MIDDLE)
                .SetTextAlignment(iTextTextAlignment.RIGHT));

            document.Add(headerTable);

            // ── Divider + Title ──
            AddDivider(document, thinBorder);
            document.Add(new iTextParagraph("BOLETA DE PAGO SEMANAL")
                .SetBold().SetFontSize(9).SetTextAlignment(iTextTextAlignment.CENTER));
            AddDivider(document, thinBorder);

            // ── Worker Info Table ──
            var week = $"{payroll.WeekStart:dd MMM} - {payroll.WeekEnd:dd MMM yyyy}";

            var infoTable = new iTextTable(UnitValue.CreatePercentArray([35f, 65f]))
                .UseAllAvailableWidth();

            AddInfoRow(infoTable, "TRABAJADOR", payroll.WorkerName, thinBorder);
            AddInfoRow(infoTable, "DOCUMENTO", payroll.WorkerIdentification, thinBorder);
            AddInfoRow(infoTable, "PERIODO", week, thinBorder);

            document.Add(infoTable);

            // ── Activity Detail ──
            document.Add(new iTextParagraph("DETALLE DE ACTIVIDADES SEMANALES")
                .SetBold().SetFontSize(8).SetMarginTop(6).SetMarginBottom(3));

            var actTable = new iTextTable(UnitValue.CreatePercentArray([14f, 24f, 14f, 12f, 18f, 18f]))
                .UseAllAvailableWidth();

            string[] headers = ["FECHA", "ACTIVIDAD", "LOTE", "HORAS", "TARIFA", "MONTO"];
            foreach (var h in headers)
            {
                var isRight = h is "HORAS" or "TARIFA" or "MONTO";
                var cell = new iTextCell()
                    .Add(new iTextParagraph(h).SetBold().SetFontSize(7))
                    .SetBackgroundColor(headerBgColor)
                    .SetBorder(new SolidBorder(borderColor, 0.5f))
                    .SetPaddingLeft(3).SetPaddingRight(3).SetPaddingTop(4).SetPaddingBottom(4);
                if (isRight) cell.SetTextAlignment(iTextTextAlignment.RIGHT);
                actTable.AddHeaderCell(cell);
            }

            if (activityEntries == null || activityEntries.Count == 0)
            {
                actTable.AddCell(new iTextCell(1, 6)
                    .Add(new iTextParagraph("Sin registros").SetFontSize(8)
                        .SetTextAlignment(iTextTextAlignment.CENTER))
                    .SetBorder(new SolidBorder(borderColor, 0.5f)));
            }
            else
            {
                foreach (var entry in activityEntries)
                {
                    actTable.AddCell(MakeCell($"{entry.Date:dd MMM}", 8, thinBorder));
                    actTable.AddCell(MakeCell(entry.ActivityName, 8, thinBorder));
                    actTable.AddCell(MakeCell(entry.BatchName, 8, thinBorder));
                    actTable.AddCell(MakeCell($"{entry.Hours:F2}", 8, thinBorder, iTextTextAlignment.RIGHT));
                    actTable.AddCell(MakeCell($"B/.{entry.Rate:F4}", 8, thinBorder, iTextTextAlignment.RIGHT));
                    actTable.AddCell(MakeCell($"B/.{entry.Amount:F2}", 8, thinBorder, iTextTextAlignment.RIGHT));
                }
            }

            document.Add(actTable);

            // ── Summary ──
            var summary = new iTextTable(UnitValue.CreatePercentArray([70f, 30f]))
                .UseAllAvailableWidth()
                .SetMarginTop(5);

            summary.AddCell(new iTextCell()
                .Add(new iTextParagraph("DEVENGADO BRUTO").SetBold().SetFontSize(8))
                .SetBorder(thinBorder).SetPaddingLeft(4).SetPaddingRight(4).SetPaddingTop(5).SetPaddingBottom(5));
            summary.AddCell(new iTextCell()
                .Add(new iTextParagraph($"B/.{payroll.GrossAmount:F2}").SetFontSize(8))
                .SetTextAlignment(iTextTextAlignment.RIGHT)
                .SetBorder(thinBorder).SetPaddingLeft(4).SetPaddingRight(4).SetPaddingTop(5).SetPaddingBottom(5));

            AddDeductionRow(summary, "   Seguro Social (9.75%)", payroll.SocialSecurity, thinBorder, negativeColor);
            AddDeductionRow(summary, "   Seguro Educativo (1.25%)", payroll.EducationalInsurance, thinBorder, negativeColor);
            AddDeductionRow(summary, "   Aporte Sindical (Sindicato Bananero de Chiriqui)", payroll.UnionFee, thinBorder, negativeColor);

            summary.AddCell(new iTextCell()
                .Add(new iTextParagraph("TOTAL NETO A PAGAR").SetBold().SetFontSize(8))
                .SetBorder(thinBorder).SetPaddingLeft(4).SetPaddingRight(4).SetPaddingTop(5).SetPaddingBottom(5));
            summary.AddCell(new iTextCell()
                .Add(new iTextParagraph($"B/.{payroll.NetAmount:F2}").SetBold().SetFontSize(8))
                .SetTextAlignment(iTextTextAlignment.RIGHT)
                .SetBorder(thinBorder).SetPaddingLeft(4).SetPaddingRight(4).SetPaddingTop(5).SetPaddingBottom(5));

            document.Add(summary);

            // ── Signature Lines ──
            document.Add(new iTextParagraph("\n"));
            var signTable = new iTextTable(UnitValue.CreatePercentArray([45f, 10f, 45f]))
                .UseAllAvailableWidth();

            signTable.AddCell(new iTextCell()
                .Add(new iTextParagraph("Firma del Trabajador").SetFontSize(8)
                    .SetTextAlignment(iTextTextAlignment.CENTER))
                .SetBorderTop(new SolidBorder(borderColor, 0.5f))
                .SetBorderBottom(iTextBorder.NO_BORDER)
                .SetBorderLeft(iTextBorder.NO_BORDER)
                .SetBorderRight(iTextBorder.NO_BORDER)
                .SetPaddingTop(3));

            signTable.AddCell(new iTextCell()
                .SetBorder(iTextBorder.NO_BORDER));

            signTable.AddCell(new iTextCell()
                .Add(new iTextParagraph("Firma Autorizada").SetFontSize(8)
                    .SetTextAlignment(iTextTextAlignment.CENTER))
                .SetBorderTop(new SolidBorder(borderColor, 0.5f))
                .SetBorderBottom(iTextBorder.NO_BORDER)
                .SetBorderLeft(iTextBorder.NO_BORDER)
                .SetBorderRight(iTextBorder.NO_BORDER)
                .SetPaddingTop(3));

            document.Add(signTable);

            // Explicit close on success — nullify so finally won't re-close
            document.Close();
            document = null;
            pdf = null;
            writer = null;
        }
        finally
        {
            // Suppress secondary exceptions so the original error is not masked
            try { document?.Close(); } catch { /* suppress */ }
            try { pdf?.Close(); } catch { /* suppress */ }
            try { writer?.Close(); } catch { /* suppress */ }
        }
    }

    private static void AddDivider(iTextDocument document, iTextBorder border)
    {
        document.Add(new iTextParagraph("")
            .SetBorderBottom(border).SetMarginTop(6).SetMarginBottom(6));
    }

    private static void AddInfoRow(iTextTable table, string label, string value, iTextBorder border)
    {
        table.AddCell(new iTextCell()
            .Add(new iTextParagraph(label).SetBold().SetFontSize(8))
            .SetBorder(border).SetPaddingLeft(4).SetPaddingRight(4).SetPaddingTop(5).SetPaddingBottom(5));
        table.AddCell(new iTextCell()
            .Add(new iTextParagraph(value).SetFontSize(8))
            .SetBorder(border).SetPaddingLeft(4).SetPaddingRight(4).SetPaddingTop(5).SetPaddingBottom(5));
    }

    private static void AddDeductionRow(iTextTable table, string label, decimal amount, iTextBorder border, iTextColor color)
    {
        table.AddCell(new iTextCell()
            .Add(new iTextParagraph(label).SetFontSize(8))
            .SetBorder(border).SetPaddingLeft(4).SetPaddingRight(4).SetPaddingTop(5).SetPaddingBottom(5));
        table.AddCell(new iTextCell()
            .Add(new iTextParagraph($"-B/.{amount:F2}").SetBold().SetFontSize(8).SetFontColor(color))
            .SetTextAlignment(iTextTextAlignment.RIGHT)
            .SetBorder(border).SetPaddingLeft(4).SetPaddingRight(4).SetPaddingTop(5).SetPaddingBottom(5));
    }

    private static iTextCell MakeCell(string text, float fontSize, iTextBorder border,
        iTextTextAlignment alignment = iTextTextAlignment.LEFT)
    {
        return new iTextCell()
            .Add(new iTextParagraph(text).SetFontSize(fontSize))
            .SetBorder(border)
            .SetTextAlignment(alignment)
            .SetPaddingLeft(3).SetPaddingRight(3).SetPaddingTop(5).SetPaddingBottom(5);
    }

    public static string? FindLogoPath()
    {
        string[] candidates =
        [
            Path.Combine(AppContext.BaseDirectory, "logo_santa_cecilia.png"),
            Path.Combine(AppContext.BaseDirectory, "logo_santa_cecilia.scale-100.png"),
            Path.Combine(AppContext.BaseDirectory, "Resources", "Images", "logo_santa_cecilia.png"),
        ];

        var assemblyDir = Path.GetDirectoryName(typeof(PayrollReceiptPage).Assembly.Location);
        if (!string.IsNullOrEmpty(assemblyDir))
        {
            candidates =
            [
                ..candidates,
                Path.Combine(assemblyDir, "logo_santa_cecilia.png"),
                Path.Combine(assemblyDir, "logo_santa_cecilia.scale-100.png"),
            ];
        }

        foreach (var path in candidates)
        {
            if (File.Exists(path))
                return path;
        }

        return null;
    }
}
