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
            var workedTimes = await _api.GetWorkedTimesAsync();
            var payrollDtos = await _api.GetPayrollsAsync();

            workerNameMap = workers.ToDictionary(w => w.Id, w => $"{w.Name} {w.LastName}");
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

        var sb = new StringBuilder();
        sb.Append("<!doctype html><html><head><meta charset='utf-8'>");
        sb.Append("<style>");
        sb.Append("body{font-family:Segoe UI,Arial;padding:20px;color:#2f3e3a;background:#F6F5F0;}");
        sb.Append(".card{background:#fff;border:1px solid #E2DDD3;border-radius:0;padding:0;overflow:hidden;}");
        sb.Append(".title{font-size:16px;font-weight:700;color:#2f3e3a;}");
        sb.Append(".sub{font-size:11px;color:#6b7f78;}");
        sb.Append(".content{padding:16px;}");
        sb.Append(".section{margin-bottom:14px;}");
        sb.Append(".section-title{font-size:11px;letter-spacing:.04em;color:#6b7f78;text-transform:uppercase;margin-bottom:8px;}");
        sb.Append(".meta{display:grid;grid-template-columns:1fr 1fr;gap:10px;}");
        sb.Append(".meta .item{border:1px solid #EEE7DC;padding:10px;}");
        sb.Append(".label{font-size:10px;color:#9fb2ab;text-transform:uppercase;letter-spacing:.03em;}");
        sb.Append(".value{font-size:14px;font-weight:600;margin-top:4px;}");
        sb.Append(".list{border:1px solid #EEE7DC;}");
        sb.Append(".row{display:flex;justify-content:space-between;padding:8px 10px;border-bottom:1px solid #EEE7DC;font-size:12px;}");
        sb.Append(".row:last-child{border-bottom:none;}");
        sb.Append(".neg{color:#c05858;font-weight:600;}");
        sb.Append(".total-bar{background:#9BC7B3;color:#fff;font-weight:700;text-align:center;padding:10px 8px;}");
        sb.Append(".muted{color:#6b7f78;font-size:11px;margin-top:10px;}");
        sb.Append("</style></head><body>");

        sb.Append("<div class='card'>");
        sb.Append("<div class='content'>");
        sb.Append("<div class='section'>");
        sb.Append("<div class='title'>Detalle de Boleta de Pago</div>");
        sb.Append($"<div class='sub'>Semana {week}</div>");
        sb.Append("</div>");
        sb.Append("<div class='section'>");
        sb.Append("<div class='section-title'>Trabajador</div>");
        sb.Append($"<div class='value'>{payroll.WorkerName}</div>");
        sb.Append($"<div class='muted'>{payroll.WorkerType}</div>");
        sb.Append("</div>");

        sb.Append("<div class='section'>");
        sb.Append("<div class='section-title'>Resumen</div>");
        sb.Append("<div class='meta'>");
        sb.Append($"<div class='item'><div class='label'>Horas</div><div class='value'>{payroll.TotalHours:F2}h</div></div>");
        sb.Append($"<div class='item'><div class='label'>Bruto</div><div class='value'>B/.{payroll.GrossAmount:F2}</div></div>");
        sb.Append($"<div class='item'><div class='label'>Neto</div><div class='value'>B/.{payroll.NetAmount:F2}</div></div>");
        sb.Append($"<div class='item'><div class='label'>Descuentos</div><div class='value'>B/.{(payroll.GrossAmount - payroll.NetAmount):F2}</div></div>");
        sb.Append("</div>");
        sb.Append("</div>");

        sb.Append("<div class='section'>");
        sb.Append("<div class='section-title'>Descuentos</div>");
        sb.Append("<div class='list'>");
        sb.Append($"<div class='row'><span>Seguro Social</span><span class='neg'>-B/.{payroll.SocialSecurity:F2}</span></div>");
        sb.Append($"<div class='row'><span>Seguro Educativo</span><span class='neg'>-B/.{payroll.EducationalInsurance:F2}</span></div>");
        sb.Append($"<div class='row'><span>Sindicato Bananero</span><span class='neg'>-B/.{payroll.UnionFee:F2}</span></div>");
        sb.Append("</div>");
        sb.Append("</div>");
        sb.Append("</div>");

        sb.Append($"<div class='total-bar'>Total a pagar  B/.{payroll.NetAmount:F2}</div>");
        sb.Append("</div>");
        sb.Append("</body></html>");

        return sb.ToString();
    }
}
