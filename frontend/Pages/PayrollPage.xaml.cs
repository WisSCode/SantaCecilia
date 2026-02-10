using System.Globalization;
using System.Text;
using frontend.Models;
using frontend.Services;

namespace frontend.Pages;

public partial class PayrollPage : ContentPage
{
    private readonly ApiService _api;
    private List<Payroll> payrolls = new();
    private Dictionary<string, string> workerNameMap = new();
    private Dictionary<string, string> workerTypeMap = new();
    private List<WorkedTimeDto> workedTimesCache = new();
    private Dictionary<string, WorkTypeDto> workTypeMap = new();
    private Dictionary<string, string> batchMap = new();

    public PayrollPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var weekStart = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek);
        PayrollWeekPicker.Date = weekStart.Date;
        await LoadPayrollsAsync();
    }

    private async Task LoadPayrollsAsync()
    {
        try
        {
            var workers = await _api.GetWorkersAsync();
            var workTypes = await _api.GetWorkTypesAsync();
            var batches = await _api.GetBatchesAsync();
            var workedTimes = await _api.GetWorkedTimesAsync();
            var payrollDtos = await _api.GetPayrollsAsync();

            workerNameMap = workers.ToDictionary(w => w.Id, w => $"{w.Name} {w.LastName}");
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

            payrolls = payrollDtos.Select((p, index) =>
            {
                var payroll = new Payroll
                {
                    Id = p.Id,
                    WorkerId = p.WorkerId,
                    WorkerName = workerNameMap.GetValueOrDefault(p.WorkerId, p.WorkerId),
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

            PayrollList.ItemsSource = payrolls;
            UpdateStats();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudo cargar la nomina: {ex.Message}", "OK");
        }
    }

    private void UpdateStats()
    {
        var weekStart = PayrollWeekPicker.Date;
        PayrollWeekLabel.Text = $"Semana del {weekStart:dd MMM yyyy}";

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

    private async void OnViewTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is Payroll payroll)
        {
            var html = BuildReceiptHtml(payroll);
            await Navigation.PushModalAsync(new PayrollReceiptPage(html));
        }
    }

    private async void OnPrintTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is Payroll payroll)
        {
            var html = BuildReceiptHtml(payroll);
            await Navigation.PushModalAsync(new PayrollReceiptPage(html));
        }
    }

    private async void OnProcessPayrollClicked(object sender, EventArgs e)
    {
        await DisplayAlertAsync("Nomina", "Procesando nomina completa.", "OK");
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
        sb.Append("@page{size:5.5in 8.5in;margin:0.35in;}");
        sb.Append("body{font-family:Segoe UI,Arial;padding:0;margin:0;color:#1f2c27;background:#fff;}");
        sb.Append(".page{width:5.2in;margin:0 auto;border:1px solid #1f2c27;padding:10px;box-sizing:border-box;}");
        sb.Append(".header{display:flex;align-items:center;gap:8px;margin-bottom:6px;}");
        sb.Append(".logo{width:34px;height:34px;border:1px solid #1f2c27;display:flex;align-items:center;justify-content:center;font-size:10px;font-weight:700;}");
        sb.Append(".title{font-size:12px;font-weight:700;text-align:center;flex:1;}");
        sb.Append(".subtitle{font-size:9px;text-align:center;color:#3c4b45;}");
        sb.Append(".divider{border-top:1px solid #1f2c27;margin:6px 0;}");
        sb.Append(".section-title{font-size:9px;font-weight:700;text-transform:uppercase;margin:6px 0 4px 0;}");
        sb.Append(".info{border:1px solid #1f2c27;border-collapse:collapse;width:100%;font-size:9px;margin-bottom:6px;}");
        sb.Append(".info td{border:1px solid #1f2c27;padding:4px;}");
        sb.Append(".table{border:1px solid #1f2c27;border-collapse:collapse;width:100%;font-size:9px;}");
        sb.Append(".table th,.table td{border:1px solid #1f2c27;padding:4px;}");
        sb.Append(".table th{background:#f4f2ed;text-transform:uppercase;font-size:8px;}");
        sb.Append(".right{text-align:right;}");
        sb.Append(".summary{border:1px solid #1f2c27;border-collapse:collapse;width:100%;font-size:9px;margin-top:6px;}");
        sb.Append(".summary td{border:1px solid #1f2c27;padding:4px;}");
        sb.Append(".neg{color:#c05858;font-weight:700;}");
        sb.Append(".total-row td{font-weight:700;}");
        sb.Append(".sign{margin-top:10px;display:flex;gap:10px;font-size:9px;}");
        sb.Append(".sign .line{flex:1;border-top:1px solid #1f2c27;text-align:center;padding-top:3px;}");
        sb.Append(".footer{margin-top:6px;font-size:8px;color:#5b6a64;text-align:center;}");
        sb.Append("</style></head><body>");

        sb.Append("<div class='page'>");
        sb.Append("<div class='header'>");
        sb.Append("<div class='logo'>SC</div>");
        sb.Append("<div>");
        sb.Append("<div class='title'>FINCA BANANERA SANTA CECILIA</div>");
        sb.Append("<div class='subtitle'>Sistema de Gestion de Nomina</div>");
        sb.Append("</div>");
        sb.Append("</div>");
        sb.Append("<div class='divider'></div>");
        sb.Append("<div class='section-title' style='text-align:center;'>BOLETA DE PAGO SEMANAL</div>");
        sb.Append("<div class='divider'></div>");

        sb.Append("<table class='info'>");
        sb.Append("<tr><td><strong>TRABAJADOR</strong></td>");
        sb.Append($"<td>{payroll.WorkerName}</td></tr>");
        sb.Append("<tr><td><strong>CODIGO</strong></td>");
        sb.Append($"<td>{payroll.DisplayId}</td></tr>");
        sb.Append("<tr><td><strong>TIPO DE LABOR</strong></td>");
        sb.Append($"<td>{payroll.WorkerType}</td></tr>");
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
        sb.Append("<tr><td><strong>DESCUENTOS DE LEY</strong></td><td></td></tr>");
        sb.Append($"<tr><td>&nbsp;&nbsp;Seguro Social (9.75%)</td><td class='right neg'>-B/.{payroll.SocialSecurity:F2}</td></tr>");
        sb.Append($"<tr><td>&nbsp;&nbsp;Seguro Educativo (1.25%)</td><td class='right neg'>-B/.{payroll.EducationalInsurance:F2}</td></tr>");
        sb.Append($"<tr><td>&nbsp;&nbsp;Aporte Sindical (Sindicato Bananero de Chiriqui)</td><td class='right neg'>-B/.{payroll.UnionFee:F2}</td></tr>");
        sb.Append($"<tr class='total-row'><td>TOTAL NETO A PAGAR</td><td class='right'>B/.{payroll.NetAmount:F2}</td></tr>");
        sb.Append("</table>");

        sb.Append("<div class='sign'>");
        sb.Append("<div class='line'>Firma del Trabajador</div>");
        sb.Append("<div class='line'>Firma Autorizada</div>");
        sb.Append("</div>");
        sb.Append("<div class='footer'>Documento generado electronicamente - Finca Bananera Santa Cecilia</div>");
        sb.Append($"<div class='footer'>Fecha de emision: {DateTime.Now:dd MMM yyyy}</div>");

        sb.Append("</div>");
        sb.Append("</body></html>");

        return sb.ToString();
    }
}
