using frontend.Services;
using frontend.Models;
using ClosedXML.Excel;
using CommunityToolkit.Maui.Storage;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iTextDocument = iText.Layout.Document;
using iTextCell = iText.Layout.Element.Cell;
using iTextTable = iText.Layout.Element.Table;
using iTextParagraph = iText.Layout.Element.Paragraph;
using iTextImage = iText.Layout.Element.Image;
using iTextTextAlignment = iText.Layout.Properties.TextAlignment;
using iTextVerticalAlignment = iText.Layout.Properties.VerticalAlignment;
using iText.Layout.Properties;

namespace frontend.Pages;

public partial class ReportsPage : ContentPage
{
    private readonly ApiService _api;
    private List<ReportRow> reportItems = new();
    private List<WorkedTimeDto> workedTimes = new();
    private Dictionary<string, WorkTypeDto> workTypeMap = new();
    private bool dataLoaded;

    public ReportsPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var weekStart = GetWeekStart(DateTime.Now);
        StartDatePicker.Date = weekStart.Date;
        EndDatePicker.Date = weekStart.AddDays(6).Date;
        UpdatePeriodLabel();

        if (!dataLoaded)
        {
            await LoadDataAsync();
            dataLoaded = true;
        }

        await BuildReportAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            var workTypes = await _api.GetWorkTypesAsync();
            workedTimes = await _api.GetWorkedTimesAsync();

            workTypeMap = workTypes.ToDictionary(wt => wt.Id, wt => wt);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudieron cargar los datos del reporte: {ex.Message}", "OK");
        }
    }

    private async Task BuildReportAsync()
    {
        try
        {
            var rangeStart = (StartDatePicker.Date ?? DateTime.Today).Date;
            var rangeEnd = (EndDatePicker.Date ?? DateTime.Today).Date;
            if (rangeEnd < rangeStart)
            {
                rangeEnd = rangeStart;
                EndDatePicker.Date = rangeEnd;
            }

            UpdatePeriodLabel();
            UpdateHeaders();

            var entries = workedTimes
                .Where(wt => wt.Date.Date >= rangeStart && wt.Date.Date <= rangeEnd)
                .ToList();

            reportItems = BuildActivityReport(entries);

            // Force refresh CollectionView
            ReportList.ItemsSource = null;
            ReportList.ItemsSource = reportItems;

            var totalHours = reportItems.Sum(r => r.TotalHours);
            var totalAmount = reportItems.Sum(r => r.TotalAmount);
            TotalHoursLabel.Text = $"{totalHours:F1}h";
            TotalAmountLabel.Text = $"B/.{totalAmount:F2}";

            if (reportItems.Count == 0)
            {
                ReportTitleLabel.Text = "Sin datos en el período seleccionado";
            }
            else
            {
                var itemCountText = reportItems.Count == 1 ? "1 registro" : $"{reportItems.Count} registros";
                ReportTitleLabel.Text = $"Resumen - {itemCountText}";
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudo cargar el reporte: {ex.Message}", "OK");
        }
    }

    private List<ReportRow> BuildActivityReport(List<WorkedTimeDto> entries)
    {
        ReportTitleLabel.Text = "Actividades";
        return entries
            .GroupBy(wt => wt.WorkTypeId)
            .Select(group =>
            {
                var workType = workTypeMap.GetValueOrDefault(group.Key);
                var name = workType?.Name ?? group.Key;
                var rate = (decimal)(workType?.DefaultRate ?? 0);
                var totalHours = group.Sum(e => e.MinutesWorked / 60m);
                var totalAmount = group.Sum(e => (decimal)e.MinutesWorked / 60m * rate);

                return new ReportRow
                {
                    Col1 = name,
                    Col2 = $"B/.{rate:F4}",
                    Col3 = $"{totalHours:F1}h",
                    Col4 = $"B/.{totalAmount:F2}",
                    Col3Sub = string.Empty,
                    TotalHours = totalHours,
                    TotalAmount = totalAmount
                };
            })
            .OrderByDescending(r => r.TotalHours)
            .ToList();
    }

    private void UpdateHeaders()
    {
        HeaderCol1.Text = "ACTIVIDAD";
        HeaderCol2.Text = "TARIFA";
        HeaderCol3.Text = "HORAS";
        HeaderCol4.Text = "MONTO";
        ReportSubtitleLabel.Text = "Totales agrupados por actividad";
    }

    private void UpdatePeriodLabel()
    {
        var start = StartDatePicker.Date ?? DateTime.Today;
        var end = EndDatePicker.Date ?? DateTime.Today;
        var culture = new System.Globalization.CultureInfo("es-ES");
        ReportPeriodLabel.Text = $"{start.ToString("dd 'de' MMMM 'de' yyyy", culture)} - {end.ToString("dd 'de' MMMM 'de' yyyy", culture)}";
    }

    private DateTime GetWeekStart(DateTime date)
    {
        return date.Date.AddDays(-(int)date.DayOfWeek);
    }

    private async void OnDateRangeChanged(object sender, DateChangedEventArgs e)
    {
        UpdatePeriodLabel();
        await BuildReportAsync();
    }

    private async void OnDownloadClicked(object sender, EventArgs e)
    {
        if (reportItems == null || reportItems.Count == 0)
        {
            await DisplayAlertAsync("Sin datos", "No hay datos para exportar. Genera un reporte primero.", "OK");
            return;
        }

        var options = new string[] { "Excel", "PDF" };
        var selected = await DisplayActionSheetAsync(
            "Descargar reporte como",
            "Cancelar",
            null,
            options);

        if (selected == "Excel")
        {
            await ExportToExcelAsync();
        }
        else if (selected == "PDF")
        {
            await ExportToPdfAsync();
        }
    }

    private async Task ExportToExcelAsync()
    {
        try
        {
            var exportFolder = await GetExportFolderPathAsync();
            if (string.IsNullOrWhiteSpace(exportFolder))
                return;

            var fileName = $"Reporte-Actividad-{DateTime.Now:yyyy-MM-dd-HHmmss}.xlsx";
            var filePath = Path.Combine(exportFolder, fileName);

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Reporte");
                var logoPath = PayrollReceiptPage.FindLogoPath();

                // Encabezado corporativo
                int row = 1;
                if (!string.IsNullOrEmpty(logoPath) && File.Exists(logoPath))
                {
                    try
                    {
                        worksheet.AddPicture(logoPath)
                            .MoveTo(worksheet.Cell(row, 1))
                            .Scale(0.22);
                    }
                    catch
                    {
                    }
                }

                worksheet.Cell(row, 2).Value = "FINCA BANANERA SANTA CECILIA";
                worksheet.Cell(row + 1, 2).Value = "Sistema de Gestión de Nómina";
                worksheet.Cell(row + 2, 2).Value = "REPORTE ADMINISTRATIVO";
                worksheet.Cell(row + 3, 2).Value = $"Fecha de emisión: {DateTime.Now:dd/MM/yyyy HH:mm}";
                worksheet.Cell(row + 4, 2).Value = $"Período: {(StartDatePicker.Date ?? DateTime.Today):dd/MM/yyyy} - {(EndDatePicker.Date ?? DateTime.Today):dd/MM/yyyy}";

                worksheet.Range(row, 2, row, 4).Merge();
                worksheet.Range(row + 1, 2, row + 1, 4).Merge();
                worksheet.Range(row + 2, 2, row + 2, 4).Merge();
                worksheet.Range(row + 3, 2, row + 3, 4).Merge();
                worksheet.Range(row + 4, 2, row + 4, 4).Merge();

                worksheet.Cell(row, 2).Style.Font.Bold = true;
                worksheet.Cell(row, 1).Style.Font.Bold = true;
                worksheet.Cell(row, 2).Style.Font.FontSize = 14;
                worksheet.Cell(row + 1, 2).Style.Font.FontSize = 10;
                worksheet.Cell(row + 2, 2).Style.Font.FontSize = 12;
                worksheet.Cell(row + 2, 2).Style.Font.Bold = true;
                worksheet.Cell(row + 3, 2).Style.Font.FontSize = 10;
                worksheet.Cell(row + 4, 2).Style.Font.FontSize = 10;

                row += 8;

                worksheet.Cell(row, 1).Value = "DETALLE DEL REPORTE";
                worksheet.Range(row, 1, row, 4).Merge();
                worksheet.Cell(row, 1).Style.Font.Bold = true;
                worksheet.Cell(row, 1).Style.Font.FontSize = 11;
                worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#E8F5EE");
                worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.FromHtml("#2E3A34");
                row++;

                // Encabezados de columna
                worksheet.Cell(row, 1).Value = HeaderCol1.Text;
                worksheet.Cell(row, 2).Value = HeaderCol2.Text;
                worksheet.Cell(row, 3).Value = HeaderCol3.Text;
                worksheet.Cell(row, 4).Value = HeaderCol4.Text;

                var headerRange = worksheet.Range(row, 1, row, 4);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#9EC9B4");
                headerRange.Style.Font.FontColor = XLColor.White;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                row++;

                // Datos
                foreach (var item in reportItems)
                {
                    worksheet.Cell(row, 1).Value = item.Col1;
                    worksheet.Cell(row, 2).Value = item.Col2;
                    worksheet.Cell(row, 3).Value = item.Col3;
                    worksheet.Cell(row, 4).Value = item.Col4;

                    var dataRowRange = worksheet.Range(row, 1, row, 4);
                    dataRowRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    dataRowRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    if (item.HasCol3Sub)
                        worksheet.Cell(row, 4).Style.Font.Bold = true;

                    row++;
                }

                row++;

                // Totales
                worksheet.Cell(row, 1).Value = "TOTALES";
                worksheet.Cell(row, 1).Style.Font.Bold = true;
                worksheet.Cell(row, 2).Value = "";
                worksheet.Cell(row, 3).Value = TotalHoursLabel.Text;
                worksheet.Cell(row, 3).Style.Font.Bold = true;
                worksheet.Cell(row, 4).Value = TotalAmountLabel.Text;
                worksheet.Cell(row, 4).Style.Font.Bold = true;

                var totalsRange = worksheet.Range(row, 1, row, 4);
                totalsRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#F6F5F0");
                totalsRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                totalsRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                worksheet.Columns().AdjustToContents();
                workbook.SaveAs(filePath);

                var openFile = await DisplayAlertAsync("Éxito", $"Reporte guardado en:\n{filePath}\n\n¿Desea abrirlo?", "Abrir", "Cerrar");

                if (openFile && File.Exists(filePath))
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
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudo exportar a Excel: {ex.Message}", "OK");
        }
    }

    private async Task ExportToPdfAsync()
    {
        try
        {
            var exportFolder = await GetExportFolderPathAsync();
            if (string.IsNullOrWhiteSpace(exportFolder))
                return;

            var fileName = $"Reporte-Actividad-{DateTime.Now:yyyy-MM-dd-HHmmss}.pdf";
            var filePath = Path.Combine(exportFolder, fileName);

            using (var writer = new PdfWriter(filePath))
            using (var pdf = new PdfDocument(writer))
            using (var document = new iTextDocument(pdf))
            {
                document.SetMargins(30, 30, 30, 30);

                var borderColor = new DeviceRgb(31, 44, 39);
                var headerBgColor = new DeviceRgb(244, 242, 237);
                var greenHeader = new DeviceRgb(158, 201, 180);
                var thinBorder = new SolidBorder(borderColor, 0.5f);

                var headerTable = new iTextTable(UnitValue.CreatePointArray([36, 250, 140]))
                    .UseAllAvailableWidth()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER);

                var logoPath = PayrollReceiptPage.FindLogoPath();
                var logoCell = new iTextCell()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .SetVerticalAlignment(iTextVerticalAlignment.MIDDLE);

                if (!string.IsNullOrEmpty(logoPath) && File.Exists(logoPath))
                {
                    var logoData = ImageDataFactory.Create(logoPath);
                    logoCell.Add(new iTextImage(logoData).ScaleToFit(30, 30));
                }
                else
                {
                    logoCell.Add(new iTextParagraph("SC").SetBold().SetFontSize(10));
                }

                headerTable.AddCell(logoCell);
                headerTable.AddCell(new iTextCell()
                    .Add(new iTextParagraph("FINCA BANANERA SANTA CECILIA").SetBold().SetFontSize(10))
                    .Add(new iTextParagraph("Sistema de Gestión de Nómina").SetFontSize(7))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .SetVerticalAlignment(iTextVerticalAlignment.MIDDLE)
                    .SetPaddingLeft(8));
                headerTable.AddCell(new iTextCell()
                    .Add(new iTextParagraph($"Fecha de emisión: {DateTime.Now:dd/MM/yyyy HH:mm}").SetFontSize(7))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .SetTextAlignment(iTextTextAlignment.RIGHT));
                document.Add(headerTable);

                document.Add(new iTextParagraph("").SetBorderBottom(thinBorder).SetMarginTop(6).SetMarginBottom(6));
                document.Add(new iTextParagraph("REPORTE ADMINISTRATIVO")
                    .SetBold().SetFontSize(11).SetTextAlignment(iTextTextAlignment.CENTER));
                document.Add(new iTextParagraph($"Período: {(StartDatePicker.Date ?? DateTime.Today):dd/MM/yyyy} - {(EndDatePicker.Date ?? DateTime.Today):dd/MM/yyyy}")
                    .SetFontSize(8).SetTextAlignment(iTextTextAlignment.CENTER));
                document.Add(new iTextParagraph("").SetBorderBottom(thinBorder).SetMarginTop(4).SetMarginBottom(8));

                document.Add(new iTextParagraph("DETALLE DEL REPORTE")
                    .SetBold().SetFontSize(9)
                    .SetBackgroundColor(headerBgColor)
                    .SetPadding(5));

                var table = new iTextTable(UnitValue.CreatePercentArray([36f, 20f, 20f, 24f])).UseAllAvailableWidth();

                string[] headers = [HeaderCol1.Text, HeaderCol2.Text, HeaderCol3.Text, HeaderCol4.Text];
                foreach (var header in headers)
                {
                    table.AddHeaderCell(new iTextCell()
                        .Add(new iTextParagraph(header).SetBold().SetFontSize(8))
                        .SetBackgroundColor(greenHeader)
                        .SetFontColor(ColorConstants.WHITE)
                        .SetBorder(thinBorder)
                        .SetPadding(4));
                }

                foreach (var item in reportItems)
                {
                    table.AddCell(new iTextCell().Add(new iTextParagraph(item.Col1).SetFontSize(8)).SetBorder(thinBorder).SetPadding(4));
                    table.AddCell(new iTextCell().Add(new iTextParagraph(item.Col2).SetFontSize(8)).SetBorder(thinBorder).SetPadding(4));
                    table.AddCell(new iTextCell().Add(new iTextParagraph(item.Col3).SetFontSize(8)).SetBorder(thinBorder).SetPadding(4));
                    table.AddCell(new iTextCell().Add(new iTextParagraph(item.Col4).SetFontSize(8)).SetBorder(thinBorder).SetPadding(4));
                }

                document.Add(table);

                var totalsTable = new iTextTable(UnitValue.CreatePercentArray([36f, 20f, 20f, 24f]))
                    .UseAllAvailableWidth()
                    .SetMarginTop(6);
                totalsTable.AddCell(new iTextCell().Add(new iTextParagraph("TOTALES").SetBold().SetFontSize(8)).SetBorder(thinBorder).SetPadding(4));
                totalsTable.AddCell(new iTextCell().Add(new iTextParagraph("").SetFontSize(8)).SetBorder(thinBorder).SetPadding(4));
                totalsTable.AddCell(new iTextCell().Add(new iTextParagraph(TotalHoursLabel.Text).SetBold().SetFontSize(8)).SetBorder(thinBorder).SetPadding(4));
                totalsTable.AddCell(new iTextCell().Add(new iTextParagraph(TotalAmountLabel.Text).SetBold().SetFontSize(8)).SetBorder(thinBorder).SetPadding(4));
                document.Add(totalsTable);

            }

            var openFile = await DisplayAlertAsync("Éxito", $"PDF guardado en:\n{filePath}\n\n¿Desea abrirlo?", "Abrir", "Cerrar");

            if (openFile && File.Exists(filePath))
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
            await DisplayAlertAsync("Error", $"No se pudo exportar a PDF: {ex.Message}", "OK");
        }
    }

    private async Task<string?> GetExportFolderPathAsync()
    {
#if ANDROID
        var result = await FolderPicker.Default.PickAsync();
        if (!result.IsSuccessful || result.Folder is null)
        {
            await DisplayAlertAsync("Exportar", "No se seleccionó carpeta.", "OK");
            return null;
        }

        return result.Folder.Path;
#else
        return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#endif
    }

}
