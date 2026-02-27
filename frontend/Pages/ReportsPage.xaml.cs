using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2010.Excel;
using frontend.Models;
using frontend.Services;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Properties;
using iTextCell = iText.Layout.Element.Cell;
using iTextDocument = iText.Layout.Document;
using iTextParagraph = iText.Layout.Element.Paragraph;
using iTextTable = iText.Layout.Element.Table;
using iTextTextAlignment = iText.Layout.Properties.TextAlignment;
using iTextBorder = iText.Layout.Borders.Border;
using iTextVerticalAlignment = iText.Layout.Properties.VerticalAlignment;
using iTextImage = iText.Layout.Element.Image;

namespace frontend.Pages;

public partial class ReportsPage : ContentPage
{
    private readonly ApiService _api;
    private List<ReportRow> reportItems = new();
    private List<WorkedTimeDto> workedTimes = new();
    private Dictionary<string, WorkTypeDto> workTypeMap = new();
    private Dictionary<string, WorkerDto> workerMap = new();
    private bool dataLoaded;
    private ReportView currentView = ReportView.Activity;

    public ReportsPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (ReportViewPicker.SelectedIndex < 0)
            ReportViewPicker.SelectedIndex = 0;

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
            var workers = await _api.GetWorkersAsync();
            workedTimes = await _api.GetWorkedTimesAsync();

            workTypeMap = workTypes.ToDictionary(wt => wt.Id, wt => wt);
            workerMap = workers.ToDictionary(w => w.Id, w => w);
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
            UpdateHeaders(currentView);

            var entries = workedTimes
                .Where(wt => wt.Date.Date >= rangeStart && wt.Date.Date <= rangeEnd)
                .ToList();

            reportItems = currentView switch
            {
                ReportView.Activity => BuildActivityReport(entries),
                ReportView.Worker => BuildWorkerReport(entries),
                ReportView.Period => BuildPeriodReport(entries),
                _ => new List<ReportRow>()
            };

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
        var totalAmount = totalHours * rate;

        return new ReportRow
        {
            Col1 = name,
            Col2 = $"B/.{rate:F4}",
            Col3 = $"{totalHours:F1}h",
            Col4 = $"B/.{totalAmount:F2}",
            TotalHours = totalHours,
            TotalAmount = totalAmount,
            HasCol4 = true
        };
    })
    .ToList();
    }

    private List<ReportRow> BuildWorkerReport(List<WorkedTimeDto> entries)
    {
        ReportTitleLabel.Text = "Trabajadores";
        return entries
            .GroupBy(wt => wt.WorkerId)
            .Select(group =>
            {
                var worker = workerMap.GetValueOrDefault(group.Key);
                var name = worker == null ? group.Key : $"{worker.Name} {worker.LastName}";
                var totalHours = group.Sum(e => e.MinutesWorked / 60m);
                var totalAmount = group.Sum(e =>
                {
                    var rate = (decimal)(workTypeMap.GetValueOrDefault(e.WorkTypeId)?.DefaultRate ?? 0);
                    return (decimal)e.MinutesWorked / 60m * rate;
                });

                return new ReportRow
                {
                    Col1 = name,
                    Col2 = $"{totalHours:F1}h",
                    Col3 = $"B/.{totalAmount:F2}",
                    
                    TotalHours = totalHours,
                    TotalAmount = totalAmount,

                    HasCol4 = false,
                };
            })
            .OrderByDescending(r => r.TotalHours)
            .ToList();
    }

    private List<ReportRow> BuildPeriodReport(List<WorkedTimeDto> entries)
    {
        ReportTitleLabel.Text = "Resumen por periodo";
        return entries
            .GroupBy(wt => wt.Date.Date)
            .Select(group =>
            {
                var totalHours = group.Sum(e => e.MinutesWorked / 60m);
                var totalAmount = group.Sum(e =>
                {
                    var rate = (decimal)(workTypeMap.GetValueOrDefault(e.WorkTypeId)?.DefaultRate ?? 0);
                    return (decimal)e.MinutesWorked / 60m * rate;
                });

                return new ReportRow
                {
                    Col1 = group.Key.ToString("dd MMM yyyy"),
                    Col2 = $"{totalHours:F1}h",
                    Col3 = $"B/.{totalAmount:F2}",
                    
                    TotalHours = totalHours,
                    TotalAmount = totalAmount,
                    SortDate = group.Key,

                    HasCol4 = false,
                };
            })
            .OrderByDescending(r => r.SortDate)
            .ToList();
    }

    private void UpdateHeaders(ReportView view)
    {

        
        switch (view)
        {
            case ReportView.Activity:
                HeaderCol4.IsVisible = true;

                HeaderCol1.Text = "ACTIVIDAD";
                HeaderCol2.Text = "TARIFA";
                HeaderCol3.Text = "HORAS";
                HeaderCol4.Text = "TOTAL";
                break;
            case ReportView.Worker:
                

                HeaderCol4.IsVisible = false;
                HeaderCol1.Text = "TRABAJADOR";
                HeaderCol2.Text = "HORAS";
                HeaderCol3.Text = "TOTAL";
                HeaderCol4.Text = string.Empty;
                break;
            case ReportView.Period:
                

                HeaderCol4.IsVisible = false;
                HeaderCol1.Text = "FECHA";
                HeaderCol2.Text = "HORAS";
                HeaderCol3.Text = "TOTAL";
                HeaderCol4.Text = string.Empty;
                break;
        }
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

    private async void OnReportViewChanged(object sender, EventArgs e)
    {
        currentView = ReportViewPicker.SelectedIndex switch
        {
            1 => ReportView.Worker,
            2 => ReportView.Period,
            _ => ReportView.Activity
        };

        await BuildReportAsync();
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
            var fileName = $"Reporte-{currentView}-{DateTime.Now:yyyy-MM-dd-HHmmss}.xlsx";
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var filePath = Path.Combine(documentsPath, fileName);

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Reporte");

                bool hasCol4 = currentView == ReportView.Activity;
                int totalColumns = hasCol4 ? 4 : 3;

                int row = 1;

                // ===== TÍTULO =====
                worksheet.Cell(row, 1).Value = "REPORTE SANTA CECILIA";
                worksheet.Cell(row, 1).Style.Font.Bold = true;
                worksheet.Cell(row, 1).Style.Font.FontSize = 14;
                worksheet.Range(row, 1, row, totalColumns).Merge();

                row += 2;

                // ===== PERÍODO =====
                worksheet.Cell(row, 1).Value =
                    $"Período: {(StartDatePicker.Date ?? DateTime.Today):dd/MM/yyyy} - {(EndDatePicker.Date ?? DateTime.Today):dd/MM/yyyy}";

                row += 1;

                var viewName = currentView switch
                {
                    ReportView.Activity => "Por Actividad",
                    ReportView.Worker => "Por Trabajador",
                    ReportView.Period => "Por Período",
                    _ => "Reporte"
                };

                worksheet.Cell(row, 1).Value = $"Vista: {viewName}";

                row += 2;

                // ===== HEADERS =====
                worksheet.Cell(row, 1).Value = HeaderCol1.Text;
                worksheet.Cell(row, 2).Value = HeaderCol2.Text;
                worksheet.Cell(row, 3).Value = HeaderCol3.Text;

                if (hasCol4)
                    worksheet.Cell(row, 4).Value = HeaderCol4.Text;

                var headerRange = worksheet.Range(row, 1, row, totalColumns);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#F5F5F5");
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                row++;

                // ===== DATOS =====
                foreach (var item in reportItems)
                {
                    // Columna 1 (texto normal)
                    worksheet.Cell(row, 1).Value = item.Col1;

                    // ===== TARIFA (Col2) =====
                    var rateTextData = item.Col2?
                        .Replace("B/.", "")
                        .Replace("B/", "")
                        .Trim();

                    if (decimal.TryParse(rateTextData, out decimal rateData))
                    {
                        worksheet.Cell(row, 2).Value = rateData;
                        worksheet.Cell(row, 2).Style.NumberFormat.Format = "\"B/.\" #,##0.0000";
                    }
                    else
                    {
                        worksheet.Cell(row, 2).Value = item.Col2;
                    }

                    // ===== HORAS (Col3) =====
                    var hoursTextData = item.Col3?
                        .Replace("h", "")
                        .Trim();

                    if (double.TryParse(hoursTextData, out double hoursData))
                    {
                        worksheet.Cell(row, 3).Value = hoursData;
                    }
                    else
                    {
                        worksheet.Cell(row, 3).Value = item.Col3;
                    }

                    // ===== TOTAL (Col4 si existe) =====
                    if (hasCol4)
                    {
                        var amountTextData = item.Col4?
                            .Replace("B/.", "")
                            .Replace("B/", "")
                            .Trim();

                        if (decimal.TryParse(amountTextData, out decimal amountData))
                        {
                            worksheet.Cell(row, 4).Value = amountData;
                            worksheet.Cell(row, 4).Style.NumberFormat.Format = "\"B/.\" #,##0.00";
                        }
                        else
                        {
                            worksheet.Cell(row, 4).Value = item.Col4;
                        }
                    }

                    row++;
                }

                row++;

                // ===== TOTALES =====
                worksheet.Cell(row, 1).Value = "TOTALES";
                worksheet.Cell(row, 1).Style.Font.Bold = true;

                // 🔹 Convertir horas (quitar la "h")
                var hoursText = TotalHoursLabel.Text.Replace("h", "").Trim();
                if (double.TryParse(hoursText, out double totalHours))
                {
                    if (hasCol4)
                        worksheet.Cell(row, 3).Value = totalHours;
                    else
                        worksheet.Cell(row, 2).Value = totalHours;
                }

                // 🔹 Convertir dinero (quitar B/.)
                var amountText = TotalAmountLabel.Text
                    .Replace("B/.", "")
                    .Replace("B/", "")
                    .Trim();

                if (decimal.TryParse(amountText, out decimal totalAmount))
                {
                    if (hasCol4)
                    {
                        worksheet.Cell(row, 4).Value = totalAmount;
                        worksheet.Cell(row, 4).Style.NumberFormat.Format = "\"B/.\" #,##0.00";
                        worksheet.Cell(row, 4).Style.Font.Bold = true;
                    }
                    else
                    {
                        worksheet.Cell(row, 3).Value = totalAmount;
                        worksheet.Cell(row, 3).Style.NumberFormat.Format = "\"B/.\" #,##0.00";
                        worksheet.Cell(row, 3).Style.Font.Bold = true;
                    }
                }

                worksheet.Columns().AdjustToContents();
                workbook.SaveAs(filePath);

                var openFile = await DisplayAlertAsync(
                    "Éxito",
                    $"Reporte guardado en:\n{filePath}\n\n¿Desea abrirlo?",
                    "Abrir",
                    "Cerrar");

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
            var fileName = $"Reporte-{currentView}-{DateTime.Now:yyyy-MM-dd-HHmmss}.pdf";
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var filePath = Path.Combine(documentsPath, fileName);

            using (var writer = new PdfWriter(filePath))
            using (var pdf = new PdfDocument(writer))
            using (var document = new iTextDocument(pdf))
            {
                document.SetMargins(20, 20, 20, 20);

                bool hasCol4 = currentView == ReportView.Activity;
                int totalColumns = hasCol4 ? 4 : 3;

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
                    .Add(new iTextParagraph($"Fecha de emisión: {DateTime.Now:dd MMM yyyy}").SetFontSize(7)
                        )
                    .SetBorder(iTextBorder.NO_BORDER)
                    .SetVerticalAlignment(iTextVerticalAlignment.MIDDLE)
                    .SetTextAlignment(iTextTextAlignment.RIGHT));

                document.Add(headerTable);

                // Espacio
                document.Add(new iText.Layout.Element.Paragraph(" "));


                // ===== ENCABEZADO =====
                var title = new iTextParagraph("REPORTE ADMINISTRATIVO")
                    .SetFontSize(16)
                    .SetBold()
                    .SetTextAlignment(iTextTextAlignment.CENTER);
                document.Add(title);

                var period = new iTextParagraph(
                    $"Período: {(StartDatePicker.Date ?? DateTime.Today):dd/MM/yyyy} - {(EndDatePicker.Date ?? DateTime.Today):dd/MM/yyyy}")
                    .SetFontSize(10)
                    .SetBold()
                    .SetTextAlignment(iTextTextAlignment.CENTER);
                document.Add(period);

                var viewName = currentView switch
                {
                    ReportView.Activity => "Por Actividad",
                    ReportView.Worker => "Por Trabajador",
                    ReportView.Period => "Por Período",
                    _ => "Reporte"
                };

                var view = new iTextParagraph($"Vista: {viewName}")
                    .SetFontSize(10)
                    .SetBold()
                    .SetTextAlignment(iTextTextAlignment.CENTER);
                document.Add(view);

                document.Add(new iTextParagraph("\n"));

                // ===== TABLA PRINCIPAL =====
                var table = new iTextTable(UnitValue.CreatePercentArray(totalColumns))
                    .UseAllAvailableWidth();

                // ===== HEADER CON COLOR =====
                var headerColor = new iText.Kernel.Colors.DeviceRgb(30, 136, 229); // Azul corporativo


                // ===== HEADERS =====
                table.AddHeaderCell(new iTextCell()
                    .Add(new iTextParagraph(HeaderCol1.Text).SetBold().SetFontSize(9).SetBackgroundColor(headerColor)));

                table.AddHeaderCell(new iTextCell()
                    .Add(new iTextParagraph(HeaderCol2.Text).SetBold().SetFontSize(9).SetBackgroundColor(headerColor)));

                table.AddHeaderCell(new iTextCell()
                    .Add(new iTextParagraph(HeaderCol3.Text).SetBold().SetFontSize(9).SetBackgroundColor(headerColor)));

                if (hasCol4)
                {
                    table.AddHeaderCell(new iTextCell()
                        .Add(new iTextParagraph(HeaderCol4.Text).SetBold().SetFontSize(9).SetBackgroundColor(headerColor)));
                }

                // ===== DATOS =====
                foreach (var item in reportItems)
                {
                    table.AddCell(new iTextCell()
                        .Add(new iTextParagraph(item.Col1).SetFontSize(9)));

                    table.AddCell(new iTextCell()
                        .Add(new iTextParagraph(item.Col2).SetFontSize(9)));

                    table.AddCell(new iTextCell()
                        .Add(new iTextParagraph(item.Col3).SetFontSize(9)));

                    if (hasCol4)
                    {
                        table.AddCell(new iTextCell()
                            .Add(new iTextParagraph(item.Col4).SetFontSize(9)));
                    }

                    if (item.HasCol3Sub)
                    {
                        table.AddCell(new iTextCell().Add(new iTextParagraph("")));
                        table.AddCell(new iTextCell().Add(new iTextParagraph("")));
                        table.AddCell(new iTextCell()
                            .Add(new iTextParagraph(item.Col3Sub).SetFontSize(8)));

                        if (hasCol4)
                            table.AddCell(new iTextCell().Add(new iTextParagraph("")));
                    }
                }
                // ===== FILA DE TOTALES DENTRO DE LA MISMA TABLA =====

                // Celda 1 - TOTALES
                table.AddCell(new iTextCell()
                    .Add(new iTextParagraph("TOTALES").SetBold())
                    .SetBorderTop(new SolidBorder(1)));

                // Si es vista por Actividad (4 columnas)
                if (hasCol4)
                {
                    // Columna 2 (TARIFA vacía)
                    table.AddCell(new iTextCell()
                        .Add(new iTextParagraph(""))
                        .SetBorderTop(new SolidBorder(1)));

                    // Columna 3 (HORAS)
                    table.AddCell(new iTextCell()
                        .Add(new iTextParagraph(TotalHoursLabel.Text).SetBold())
                        
                        .SetBorderTop(new SolidBorder(1)));

                    // Columna 4 (TOTAL DINERO)
                    table.AddCell(new iTextCell()
                        .Add(new iTextParagraph(TotalAmountLabel.Text).SetBold())
                        
                        .SetBorderTop(new SolidBorder(1)));
                }
                else
                {
                    // Columna 2 (HORAS)
                    table.AddCell(new iTextCell()
                        .Add(new iTextParagraph(TotalHoursLabel.Text).SetBold())
                        
                        .SetBorderTop(new SolidBorder(1)));

                    // Columna 3 (TOTAL DINERO)
                    table.AddCell(new iTextCell()
                        .Add(new iTextParagraph(TotalAmountLabel.Text).SetBold())
                        
                        .SetBorderTop(new SolidBorder(1)));
                }

                document.Add(table);

                document.Add(new iTextParagraph("\n"));

                

                /*var timestamp = new iTextParagraph(
                    $"\nGenerado: {DateTime.Now:dd/MM/yyyy HH:mm:ss}")
                    .SetFontSize(8)
                    .SetTextAlignment(iTextTextAlignment.RIGHT);

                document.Add(timestamp);*/
            }

            var openFile = await DisplayAlertAsync(
                "Éxito",
                $"PDF guardado en:\n{filePath}\n\n¿Desea abrirlo?",
                "Abrir",
                "Cerrar");

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
            await DisplayAlertAsync("Error",
                $"No se pudo exportar a PDF: {ex.Message}",
                "OK");
        }
    }

    private enum ReportView
    {
        Activity,
        Worker,
        Period
    }

    private sealed class ReportRow
    {
        public string Col1 { get; set; } = string.Empty;
        public string Col2 { get; set; } = string.Empty;
        public string Col3 { get; set; } = string.Empty;
        public string Col3Sub { get; set; } = string.Empty;

        public string Col4 { get; set; } = string.Empty;

        public bool HasCol4 { get; set; }
        public bool HasCol3Sub => !string.IsNullOrWhiteSpace(Col3Sub);
        public decimal TotalHours { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime SortDate { get; set; } = DateTime.MinValue;
    }

    private class ReportItem
    {
        public ReportItem(string activityName, decimal hours, decimal rate)
        {
            ActivityName = activityName;
            Hours = hours;
            Rate = rate;
            Amount = hours * rate;
        }

        public string ActivityName { get; }
        public decimal Hours { get; }
        public decimal Rate { get; }
        public decimal Amount { get; }
    }

    public static string? FindLogoPath()
    {
        string[] candidates =
        [
            Path.Combine(AppContext.BaseDirectory, "logo_santa_cecilia.png"),
            Path.Combine(AppContext.BaseDirectory, "logo_santa_cecilia.scale-100.png"),
            Path.Combine(AppContext.BaseDirectory, "Resources", "Images", "logo_santa_cecilia.png"),
        ];

        foreach (var path in candidates)
        {
            if (File.Exists(path))
                return path;
        }

        return null;
    }
}
