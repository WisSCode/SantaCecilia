using frontend.Services;
using ClosedXML.Excel;
using iText.Kernel.Pdf;
using iText.Layout;
using iTextDocument = iText.Layout.Document;
using iTextCell = iText.Layout.Element.Cell;
using iTextTable = iText.Layout.Element.Table;
using iTextParagraph = iText.Layout.Element.Paragraph;
using iTextTextAlignment = iText.Layout.Properties.TextAlignment;
using iText.Layout.Properties;

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
                var totalAmount = group.Sum(e => (decimal)e.MinutesWorked / 60m * rate);

                return new ReportRow
                {
                    Col1 = name,
                    Col2 = $"B/.{rate:F4}",
                    Col3 = $"{totalHours:F1}h",
                    Col3Sub = $"B/.{totalAmount:F2}",
                    TotalHours = totalHours,
                    TotalAmount = totalAmount
                };
            })
            .OrderByDescending(r => r.TotalHours)
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
                    Col3Sub = string.Empty,
                    TotalHours = totalHours,
                    TotalAmount = totalAmount
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
                    Col3Sub = string.Empty,
                    TotalHours = totalHours,
                    TotalAmount = totalAmount,
                    SortDate = group.Key
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
                HeaderCol1.Text = "ACTIVIDAD";
                HeaderCol2.Text = "TARIFA";
                HeaderCol3.Text = "TOTAL";
                break;
            case ReportView.Worker:
                HeaderCol1.Text = "TRABAJADOR";
                HeaderCol2.Text = "HORAS";
                HeaderCol3.Text = "TOTAL";
                break;
            case ReportView.Period:
                HeaderCol1.Text = "FECHA";
                HeaderCol2.Text = "HORAS";
                HeaderCol3.Text = "TOTAL";
                break;
        }
    }

    private void UpdatePeriodLabel()
    {
        var start = StartDatePicker.Date ?? DateTime.Today;
        var end = EndDatePicker.Date ?? DateTime.Today;
        ReportPeriodLabel.Text = $"{start:dd 'de' MMMM 'de' yyyy} - {end:dd 'de' MMMM 'de' yyyy}";
    }

    private DateTime GetWeekStart(DateTime date)
    {
        return date.Date.AddDays(-(int)date.DayOfWeek);
    }

    private async void OnApplyClicked(object sender, EventArgs e)
    {
        await BuildReportAsync();
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

                // Encabezado
                int row = 1;
                worksheet.Cell(row, 1).Value = "REPORTE SANTA CECILIA";
                worksheet.Cell(row, 1).Style.Font.Bold = true;
                worksheet.Cell(row, 1).Style.Font.FontSize = 14;
                worksheet.Range(row, 1, row, 3).Merge();

                row += 2;
                worksheet.Cell(row, 1).Value = $"Período: {(StartDatePicker.Date ?? DateTime.Today):dd/MM/yyyy} - {(EndDatePicker.Date ?? DateTime.Today):dd/MM/yyyy}";
                worksheet.Cell(row, 1).Style.Font.FontSize = 11;

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

                // Encabezados de columna
                worksheet.Cell(row, 1).Value = HeaderCol1.Text;
                worksheet.Cell(row, 2).Value = HeaderCol2.Text;
                worksheet.Cell(row, 3).Value = HeaderCol3.Text;

                var headerRange = worksheet.Range(row, 1, row, 3);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#F5F5F5");
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                row++;

                // Datos
                foreach (var item in reportItems)
                {
                    worksheet.Cell(row, 1).Value = item.Col1;
                    worksheet.Cell(row, 2).Value = item.Col2;
                    worksheet.Cell(row, 3).Value = item.Col3;

                    if (item.HasCol3Sub)
                    {
                        row++;
                        worksheet.Cell(row, 1).Value = "";
                        worksheet.Cell(row, 2).Value = "";
                        worksheet.Cell(row, 3).Value = item.Col3Sub;
                        worksheet.Cell(row, 3).Style.Font.FontSize = 9;
                    }

                    row++;
                }

                row++;

                // Totales
                worksheet.Cell(row, 1).Value = "TOTALES";
                worksheet.Cell(row, 1).Style.Font.Bold = true;
                worksheet.Cell(row, 2).Value = TotalHoursLabel.Text;
                worksheet.Cell(row, 2).Style.Font.Bold = true;
                worksheet.Cell(row, 3).Value = TotalAmountLabel.Text;
                worksheet.Cell(row, 3).Style.Font.Bold = true;

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
            var fileName = $"Reporte-{currentView}-{DateTime.Now:yyyy-MM-dd-HHmmss}.pdf";
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var filePath = Path.Combine(documentsPath, fileName);

            using (var writer = new PdfWriter(filePath))
            using (var pdf = new PdfDocument(writer))
            using (var document = new iTextDocument(pdf))
            {
                document.SetMargins(20, 20, 20, 20);

                // Encabezado
                var title = new iTextParagraph("REPORTE SANTA CECILIA")
                    .SetFontSize(16)
                    .SetBold()
                    .SetTextAlignment(iTextTextAlignment.CENTER);
                document.Add(title);

                var period = new iTextParagraph($"Período: {(StartDatePicker.Date ?? DateTime.Today):dd/MM/yyyy} - {(EndDatePicker.Date ?? DateTime.Today):dd/MM/yyyy}")
                    .SetFontSize(10);
                document.Add(period);

                var viewName = currentView switch
                {
                    ReportView.Activity => "Por Actividad",
                    ReportView.Worker => "Por Trabajador",
                    ReportView.Period => "Por Período",
                    _ => "Reporte"
                };
                var view = new iTextParagraph($"Vista: {viewName}")
                    .SetFontSize(10);
                document.Add(view);

                document.Add(new iTextParagraph("\n"));

                // Tabla
                var table = new iTextTable(UnitValue.CreatePercentArray(3)).UseAllAvailableWidth();

                // Encabezados
                table.AddHeaderCell(new iTextCell().Add(new iTextParagraph(HeaderCol1.Text).SetBold().SetFontSize(9)));
                table.AddHeaderCell(new iTextCell().Add(new iTextParagraph(HeaderCol2.Text).SetBold().SetFontSize(9)));
                table.AddHeaderCell(new iTextCell().Add(new iTextParagraph(HeaderCol3.Text).SetBold().SetFontSize(9)));

                // Datos
                foreach (var item in reportItems)
                {
                    table.AddCell(new iTextCell().Add(new iTextParagraph(item.Col1).SetFontSize(9)));
                    table.AddCell(new iTextCell().Add(new iTextParagraph(item.Col2).SetFontSize(9)));
                    table.AddCell(new iTextCell().Add(new iTextParagraph(item.Col3).SetFontSize(9)));

                    if (item.HasCol3Sub)
                    {
                        table.AddCell(new iTextCell().Add(new iTextParagraph("")));
                        table.AddCell(new iTextCell().Add(new iTextParagraph("")));
                        table.AddCell(new iTextCell().Add(new iTextParagraph(item.Col3Sub).SetFontSize(8)));
                    }
                }

                document.Add(table);

                document.Add(new iTextParagraph("\n"));

                // Totales
                var totalsTable = new iTextTable(UnitValue.CreatePercentArray(3)).UseAllAvailableWidth();
                totalsTable.AddCell(new iTextCell().Add(new iTextParagraph("TOTALES").SetBold()));
                totalsTable.AddCell(new iTextCell().Add(new iTextParagraph(TotalHoursLabel.Text).SetBold()));
                totalsTable.AddCell(new iTextCell().Add(new iTextParagraph(TotalAmountLabel.Text).SetBold()));
                document.Add(totalsTable);

                var timestamp = new iTextParagraph($"\nGenerado: {DateTime.Now:dd/MM/yyyy HH:mm:ss}")
                    .SetFontSize(8)
                    .SetTextAlignment(iTextTextAlignment.RIGHT);
                document.Add(timestamp);
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
}
