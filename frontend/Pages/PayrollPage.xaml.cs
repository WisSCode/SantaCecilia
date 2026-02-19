using System.Globalization;
using System.Text;
using frontend.Helpers;
using frontend.Models;
using frontend.Services;

namespace frontend.Pages;

public partial class PayrollPage : ContentPage
{
    private readonly ApiService _api;
    private List<Payroll> payrolls = new();
    private List<Payroll> allPayrolls = new();
    private Dictionary<string, string> workerNameMap = new();
    private Dictionary<string, string> workerIdentificationMap = new();
    private Dictionary<string, string> workerTypeMap = new();
    private List<WorkedTimeDto> workedTimesCache = new();
    private Dictionary<string, WorkTypeDto> workTypeMap = new();
    private Dictionary<string, string> batchMap = new();
    private DateTime _currentWeekStart;

    public PayrollPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _currentWeekStart = GetWeekStart(DateTime.Now);
        PayrollWeekPicker.Date = _currentWeekStart;
        await LoadPayrollsAsync(_currentWeekStart);
    }

    private async Task LoadPayrollsAsync(DateTime weekStart)
    {
        try
        {
            var workers = await _api.GetWorkersAsync();
            var workTypes = await _api.GetWorkTypesAsync();
            var batches = await _api.GetBatchesAsync();
            var workedTimes = await _api.GetWorkedTimesAsync();
            var payrollDtos = await _api.GetPayrollsAsync();

            workerNameMap = workers.ToDictionary(w => w.Id, w => $"{w.Name} {w.LastName}");
            workerIdentificationMap = workers.ToDictionary(w => w.Id, w => w.Identification);
            workTypeMap = workTypes.ToDictionary(wt => wt.Id, wt => wt);
            workedTimesCache = workedTimes;
            batchMap = batches.ToDictionary(b => b.Id, b => b.Name);
            var workTypeNameMap = workTypes.ToDictionary(wt => wt.Id, wt => wt.Name);

            // Map each worker to their most recent work type
            var latestByWorker = workedTimes
                .GroupBy(wt => wt.WorkerId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(wt => wt.Date).First().WorkTypeId);

            workerTypeMap = latestByWorker.ToDictionary(
                kv => kv.Key,
                kv => workTypeNameMap.GetValueOrDefault(kv.Value, "-"));

            allPayrolls = payrollDtos.Select((p, index) =>
            {
                var payroll = new Payroll
                {
                    Id = p.Id,
                    WorkerId = p.WorkerId,
                    WorkerName = workerNameMap.GetValueOrDefault(p.WorkerId, p.WorkerId),
                    WorkerIdentification = workerIdentificationMap.GetValueOrDefault(p.WorkerId, "-"),
                    WorkerType = workerTypeMap.GetValueOrDefault(p.WorkerId, "-"),
                    WeekStart = p.WeekStart,
                    WeekEnd = p.WeekEnd,
                    TotalMinutes = p.TotalMinutes,
                    TotalHours = p.TotalMinutes / 60m,
                    GrossAmount = (decimal)p.GrossAmount,
                    Status = p.Status switch
                    {
                        "Paid" => PayrollStatus.Paid,
                        "Cancelled" => PayrollStatus.Cancelled,
                        _ => PayrollStatus.Pending
                    },
                    PaidAt = p.PaidAt,
                    SequentialId = index + 1
                };
                payroll.ApplyDeductions();
                return payroll;
            }).ToList();

            ApplyWeekFilter(weekStart);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudo cargar la nomina: {ex.Message}", "OK");
        }
    }

    private void ApplyWeekFilter(DateTime weekStart)
    {
        payrolls = allPayrolls
            .Where(p => p.WeekStart.Date == weekStart.Date)
            .OrderBy(p => p.WorkerName)
            .ToList();

        PayrollList.ItemsSource = payrolls;
        UpdateStats();
    }

    private void UpdateStats()
    {
        PayrollWeekLabel.Text = $"Semana del {_currentWeekStart:dd MMM yyyy}";

        var totalGross = payrolls.Sum(p => p.GrossAmount);
        var totalDeductions = payrolls.Sum(p => p.DeductionsTotal);
        var totalNet = payrolls.Sum(p => p.NetAmount);

        GrossTotal.Text = $"B/.{totalGross:F2}";
        DeductionsTotal.Text = $"B/.{totalDeductions:F2}";
        NetTotal.Text = $"B/.{totalNet:F2}";
        WorkerCountLabel.Text = $"{payrolls.Count} trabajadores";

        TotalGrossLabel.Text = $"B/.{totalGross:F2}";
        TotalDeductionsLabel.Text = $"-B/.{totalDeductions:F2}";
        TotalNetLabel.Text = $"B/.{totalNet:F2}";
    }

    private DateTime GetWeekStart(DateTime date)
    {
        return WeekHelper.GetWeekStart(date);
    }

    private async void OnWeekChanged(object sender, DateChangedEventArgs e)
    {
        var selected = e.NewDate ?? DateTime.Today;
        _currentWeekStart = GetWeekStart(selected);
        if (PayrollWeekPicker.Date != _currentWeekStart)
            PayrollWeekPicker.Date = _currentWeekStart;

        await LoadPayrollsAsync(_currentWeekStart);
    }

    private async void OnViewTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is Payroll payroll)
        {
            var html = BuildReceiptHtml(payroll);
            var entries = BuildActivityEntries(payroll);
            await Navigation.PushModalAsync(new PayrollReceiptPage(html, payroll, entries));
        }
    }

    private async void OnDownloadTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is not Payroll payroll)
            return;

        try
        {
            var entries = BuildActivityEntries(payroll);
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var safeWorkerName = string.Join("_", payroll.WorkerName.Split(Path.GetInvalidFileNameChars()));
            var fileName = $"Recibo_{safeWorkerName}_{payroll.WeekStart:yyyy-MM-dd}.pdf";
            var filePath = Path.Combine(documentsPath, fileName);

            if (File.Exists(filePath))
            {
                try { File.Delete(filePath); }
                catch { filePath = Path.Combine(documentsPath, $"Recibo_{safeWorkerName}_{payroll.WeekStart:yyyy-MM-dd}_{DateTime.Now:HHmmss}.pdf"); }
            }

            PayrollReceiptPage.GenerateReceiptPdf(filePath, payroll, entries);

            if (!File.Exists(filePath))
            {
                await DisplayAlertAsync("Error", "No se pudo generar el archivo PDF", "OK");
                return;
            }

            var openFile = await DisplayAlertAsync("\u00c9xito", $"Recibo guardado en:\n{filePath}\n\n\u00bfDesea abrirlo?", "Abrir", "Cerrar");

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
            await DisplayAlertAsync("Error", $"No se pudo descargar el recibo: {ex.Message}", "OK");
        }
    }

    private async void OnExportClicked(object sender, TappedEventArgs e)
    {
        if (payrolls.Count == 0)
        {
            await DisplayAlertAsync("Exportar", "No hay boletas para exportar en esta semana.", "OK");
            return;
        }

        try
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var weekStart = payrolls[0].WeekStart;
            var folderName = $"Nomina_{weekStart:yyyy-MM-dd}";
            var folderPath = Path.Combine(documentsPath, folderName);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var count = 0;
            foreach (var payroll in payrolls)
            {
                var entries = BuildActivityEntries(payroll);
                var safeWorkerName = string.Join("_", payroll.WorkerName.Split(Path.GetInvalidFileNameChars()));
                var fileName = $"Recibo_{safeWorkerName}_{weekStart:yyyy-MM-dd}.pdf";
                var filePath = Path.Combine(folderPath, fileName);

                if (File.Exists(filePath))
                {
                    try { File.Delete(filePath); }
                    catch { filePath = Path.Combine(folderPath, $"Recibo_{safeWorkerName}_{weekStart:yyyy-MM-dd}_{DateTime.Now:HHmmss}.pdf"); }
                }

                PayrollReceiptPage.GenerateReceiptPdf(filePath, payroll, entries);
                count++;
            }

            var openFolder = await DisplayAlertAsync("Éxito", $"Se exportaron {count} boletas en:\n{folderPath}\n\n¿Desea abrir la carpeta?", "Abrir", "Cerrar");

            if (openFolder)
            {
#if WINDOWS
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = folderPath,
                    UseShellExecute = true
                });
#endif
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudieron exportar las boletas: {ex.Message}", "OK");
        }
    }

    private async void OnProcessPayrollClicked(object sender, EventArgs e)
    {
        var weekStart = _currentWeekStart;
        var confirm = await DisplayAlertAsync(
            "Procesar nomina",
            $"Se generara la nomina de la semana del {weekStart:dd MMM yyyy}. Continuar?",
            "Procesar",
            "Cancelar");

        if (!confirm)
            return;

        try
        {
            var count = await _api.ProcessPayrollAsync(weekStart);
            await DisplayAlertAsync("Nomina", $"Nomina procesada. Registros: {count}", "OK");
            await LoadPayrollsAsync(weekStart);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudo procesar la nomina: {ex.Message}", "OK");
        }
    }

    private async void OnMarkPaidTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is not Payroll payroll)
            return;

        if (!payroll.IsPending)
            return;

        var confirm = await DisplayAlertAsync(
            "Marcar pagado",
            $"Marcar como pagada la nomina de {payroll.WorkerName}?",
            "Marcar",
            "Cancelar");

        if (!confirm)
            return;

        try
        {
            payroll.Status = PayrollStatus.Paid;
            payroll.PaidAt = DateTime.Now;

            await _api.UpdatePayrollAsync(payroll.Id, new PayrollDto
            {
                Id = payroll.Id,
                WorkerId = payroll.WorkerId,
                WeekStart = payroll.WeekStart,
                WeekEnd = payroll.WeekEnd,
                TotalMinutes = payroll.TotalMinutes,
                GrossAmount = (double)payroll.GrossAmount,
                Status = "Paid",
                PaidAt = payroll.PaidAt
            });

            ApplyWeekFilter(_currentWeekStart);
        }
        catch (Exception ex)
        {
            payroll.Status = PayrollStatus.Pending;
            payroll.PaidAt = null;
            await DisplayAlertAsync("Error", $"No se pudo actualizar la nomina: {ex.Message}", "OK");
        }
    }

    private List<PayrollActivityEntry> BuildActivityEntries(Payroll payroll)
    {
        var weekStart = payroll.WeekStart.Date;
        var weekEnd = payroll.WeekEnd.Date;
        var entries = workedTimesCache
            .Where(wt => wt.WorkerId == payroll.WorkerId && wt.Date.Date >= weekStart && wt.Date.Date <= weekEnd)
            .OrderBy(wt => wt.Date)
            .ToList();

        return entries.Select(entry =>
        {
            var hours = entry.MinutesWorked / 60m;
            var workType = workTypeMap.GetValueOrDefault(entry.WorkTypeId);
            var activityName = workType?.Name ?? entry.WorkTypeId;
            var rate = (decimal)(workType?.DefaultRate ?? 0);
            var gross = hours * rate;
            var lote = batchMap.GetValueOrDefault(entry.BatchId, entry.BatchId);

            return new PayrollActivityEntry
            {
                Date = entry.Date,
                ActivityName = activityName,
                BatchName = lote,
                Hours = hours,
                Rate = rate,
                Amount = gross
            };
        }).ToList();
    }

    private string BuildReceiptHtml(Payroll payroll)
    {
        var week = $"{payroll.WeekStart:dd MMM} - {payroll.WeekEnd:dd MMM yyyy}";

        var weekStart = payroll.WeekStart.Date;
        var weekEnd = payroll.WeekEnd.Date;
        var entries = workedTimesCache
            .Where(wt => wt.WorkerId == payroll.WorkerId && wt.Date.Date >= weekStart && wt.Date.Date <= weekEnd)
            .OrderBy(wt => wt.Date)
            .ToList();

        var sb = new StringBuilder();
        sb.Append("<!doctype html><html><head><meta charset='utf-8'>");
        sb.Append("<style>");
        sb.Append("@page{size:8.5in 11in;margin:0.5in;}");
        sb.Append("*{box-sizing:border-box;}");
        sb.Append("body{font-family:'Segoe UI',Arial,sans-serif;margin:0;padding:0;color:#1f2c27;background:#fff;font-size:8px;}");
        sb.Append(".page{width:100%;padding:14px;}");
        sb.Append("@media screen{");
        sb.Append("  body{background:#e8e5de;padding:20px;}");
        sb.Append("  .page{max-width:7.5in;margin:0 auto;border:1px solid #1f2c27;background:#fff;}");
        sb.Append("}");
        sb.Append("@media print{");
        sb.Append("  body{background:#fff;margin:0;padding:0;}");
        sb.Append("  .page{max-width:none;border:none;margin:0;padding:0;}");
        sb.Append("}");
        sb.Append(".header{display:flex;align-items:center;gap:8px;margin-bottom:6px;}");
        sb.Append(".logo{width:30px;height:30px;object-fit:contain;}");
        sb.Append(".title{font-size:10px;font-weight:700;}");
        sb.Append(".subtitle{font-size:8px;color:#3c4b45;}");
        sb.Append(".header-right{margin-left:auto;text-align:right;font-size:7px;color:#3c4b45;}");
        sb.Append(".divider{border-top:1px solid #1f2c27;margin:5px 0;}");
        sb.Append(".section-title{font-size:8px;font-weight:700;text-transform:uppercase;margin:5px 0 3px 0;}");
        sb.Append(".info{border:1px solid #1f2c27;border-collapse:collapse;width:100%;font-size:8px;margin-bottom:5px;}");
        sb.Append(".info td{border:1px solid #1f2c27;padding:3px;}");
        sb.Append(".table{border:1px solid #1f2c27;border-collapse:collapse;width:100%;font-size:8px;}");
        sb.Append(".table th,.table td{border:1px solid #1f2c27;padding:3px;}");
        sb.Append(".table th{background:#f4f2ed;text-transform:uppercase;font-size:7px;}");
        sb.Append(".right{text-align:right;}");
        sb.Append(".summary{border:1px solid #1f2c27;border-collapse:collapse;width:100%;font-size:8px;margin-top:5px;}");
        sb.Append(".summary td{border:1px solid #1f2c27;padding:3px;}");
        sb.Append(".neg{color:#c05858;font-weight:700;}");
        sb.Append(".total-row td{font-weight:700;}");
        sb.Append(".sign{margin-top:12px;display:flex;gap:60px;font-size:8px;justify-content:center;}");
        sb.Append(".sign .line{flex:0 1 40%;border-top:1px solid #1f2c27;text-align:center;padding-top:3px;}");
        sb.Append("</style></head><body>");

        sb.Append("<div class='page'>");
        sb.Append("<div class='header'>");

        var logoPath = PayrollReceiptPage.FindLogoPath();
        if (logoPath != null)
        {
            var logoBytes = File.ReadAllBytes(logoPath);
            var logoBase64 = Convert.ToBase64String(logoBytes);
            sb.Append($"<img class='logo' src='data:image/png;base64,{logoBase64}' />");
        }
        else
        {
            sb.Append("<div style='width:30px;height:30px;border:1px solid #1f2c27;display:flex;align-items:center;justify-content:center;font-size:9px;font-weight:700;'>SC</div>");
        }

        sb.Append("<div>");
        sb.Append("<div class='title'>FINCA BANANERA SANTA CECILIA</div>");
        sb.Append("<div class='subtitle'>Sistema de Gestion de Nomina</div>");
        sb.Append("</div>");
        sb.Append("<div class='header-right'>");
        sb.Append($"<div>Fecha de emision: {DateTime.Now:dd MMM yyyy}</div>");
        sb.Append($"<div>ID: {payroll.Id}</div>");
        sb.Append("</div>");
        sb.Append("</div>");
        sb.Append("<div class='divider'></div>");
        sb.Append("<div class='section-title' style='text-align:center;'>BOLETA DE PAGO SEMANAL</div>");
        sb.Append("<div class='divider'></div>");

        sb.Append("<table class='info'>");
        sb.Append("<tr><td><strong>TRABAJADOR</strong></td>");
        sb.Append($"<td>{payroll.WorkerName}</td></tr>");
        sb.Append("<tr><td><strong>DOCUMENTO</strong></td>");
        sb.Append($"<td>{payroll.WorkerIdentification}</td></tr>");
        sb.Append("<tr><td><strong>PERIODO</strong></td>");
        sb.Append($"<td>{week}</td></tr>");
        sb.Append("</table>");

        sb.Append("<div class='section-title'>DETALLE DE ACTIVIDADES SEMANALES</div>");
        sb.Append("<table class='table'>");
        sb.Append("<tr><th>Fecha</th><th>Actividad</th><th>Lote</th><th class='right'>Horas</th><th class='right'>Tarifa</th><th class='right'>Monto</th></tr>");

        if (entries.Count == 0)
        {
            sb.Append("<tr><td colspan='6' style='text-align:center;'>Sin registros</td></tr>");
        }
        else
        {
            foreach (var entry in entries)
            {
                var hours = entry.MinutesWorked / 60m;
                var workType = workTypeMap.GetValueOrDefault(entry.WorkTypeId);
                var activityName = workType?.Name ?? entry.WorkTypeId;
                var rate = (decimal)(workType?.DefaultRate ?? 0);
                var gross = hours * rate;
                var lote = batchMap.GetValueOrDefault(entry.BatchId, entry.BatchId);

                sb.Append("<tr>");
                sb.Append($"<td>{entry.Date:dd MMM}</td>");
                sb.Append($"<td>{activityName}</td>");
                sb.Append($"<td>{lote}</td>");
                sb.Append($"<td class='right'>{hours:F2}</td>");
                sb.Append($"<td class='right'>B/.{rate:F4}</td>");
                sb.Append($"<td class='right'>B/.{gross:F2}</td>");
                sb.Append("</tr>");
            }
        }

        sb.Append("</table>");

        sb.Append("<table class='summary'>");
        sb.Append("<tr><td><strong>DEVENGADO BRUTO</strong></td>");
        sb.Append($"<td class='right'>B/.{payroll.GrossAmount:F2}</td></tr>");
        sb.Append($"<tr><td>&nbsp;&nbsp;Seguro Social (9.75%)</td><td class='right neg'>-B/.{payroll.SocialSecurity:F2}</td></tr>");
        sb.Append($"<tr><td>&nbsp;&nbsp;Seguro Educativo (1.25%)</td><td class='right neg'>-B/.{payroll.EducationalInsurance:F2}</td></tr>");
        sb.Append($"<tr><td>&nbsp;&nbsp;Aporte Sindical (Sindicato Bananero de Chiriqui)</td><td class='right neg'>-B/.{payroll.UnionFee:F2}</td></tr>");
        sb.Append($"<tr class='total-row'><td>TOTAL NETO A PAGAR</td><td class='right'>B/.{payroll.NetAmount:F2}</td></tr>");
        sb.Append("</table>");

        sb.Append("<div class='sign'>");
        sb.Append("<div class='line'>Firma del Trabajador</div>");
        sb.Append("<div class='line'>Firma Autorizada</div>");
        sb.Append("</div>");

        sb.Append("</div>");
        sb.Append("</body></html>");

        return sb.ToString();
    }
}
