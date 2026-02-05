using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using frontend.Models;
using frontend.Services;
using frontend.Data;

namespace frontend;

public partial class PayrollPage : ContentPage
{
    private List<Payroll> payrolls = new();
    private bool hasLoaded;

    public PayrollPage()
    {
        InitializeComponent();
        LoadDemoPayrolls();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var weekStart = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek);
        PayrollWeekLabel.Text = $"Semana del {weekStart:dd 'de' MMMM 'de' yyyy}";
        PayrollWeekPicker.Date = weekStart.Date;
        if (!hasLoaded)
        {
            await LoadPayrollsAsync(weekStart.Date);
            hasLoaded = true;
        }
    }

    private void LoadDemoPayrolls()
    {
        payrolls = new List<Payroll>
        {
            new Payroll
            {
                Id = "1",
                WorkerId = "TRB-001",
                WorkerName = "Juan Pérez",
                WorkerType = "Control de Sigatoka",
                WeekStart = DateTime.Now.AddDays(-7),
                WeekEnd = DateTime.Now,
                TotalHours = 42.5m,
                GrossAmount = 39.81m,
                Status = PayrollStatus.Pending
            },
            new Payroll
            {
                Id = "2",
                WorkerId = "TRB-002",
                WorkerName = "María González",
                WorkerType = "Mantenimiento Semillero",
                WeekStart = DateTime.Now.AddDays(-7),
                WeekEnd = DateTime.Now,
                TotalHours = 40.0m,
                GrossAmount = 35.82m,
                Status = PayrollStatus.Pending
            },
            new Payroll
            {
                Id = "3",
                WorkerId = "TRB-003",
                WorkerName = "Carlos Ramírez",
                WorkerType = "Mecánico",
                WeekStart = DateTime.Now.AddDays(-7),
                WeekEnd = DateTime.Now,
                TotalHours = 40.0m,
                GrossAmount = 40.50m,
                Status = PayrollStatus.Pending
            },
        };

        foreach (var payroll in payrolls)
        {
            payroll.ApplyDeductions();
        }

        PayrollList.ItemsSource = payrolls;

        PayrollCount.Text = payrolls.Count.ToString();
        var total = payrolls.Sum(p => p.NetAmount);
        TotalPayroll.Text = $"B/.{total:F2}";
    }

    private async Task LoadPayrollsAsync(DateTime weekStart)
    {
        try
        {
            var api = new ApiService(new System.Net.Http.HttpClient());
            var entries = await api.GetEntriesAsync();
            if (entries == null || entries.Count == 0)
            {
                return;
            }

            var weekEnd = weekStart.AddDays(7);
            var weekEntries = entries
                .Where(e => e.Date >= weekStart && e.Date < weekEnd)
                .ToList();

            if (weekEntries.Count == 0)
            {
                return;
            }

            var minWage = Activities.ActivityList.FirstOrDefault(a => a.Id == "min-wage").Rate;

            payrolls = weekEntries
                .GroupBy(e => new { e.WorkerId, e.WorkerName })
                .Select(group =>
                {
                    var gross = group.Sum(e =>
                    {
                        var hours = e.Hours + (e.Minutes / 60m);
                        var activity = Activities.ActivityList.FirstOrDefault(a => a.Id == e.ActivityId);
                        var rate = e.Rate > 0 ? e.Rate : (activity.Rate > 0 ? activity.Rate : minWage);
                        return hours * rate;
                    });

                    var totalHours = group.Sum(e => e.Hours + (e.Minutes / 60m));
                    var workerType = group.FirstOrDefault()?.ActivityName ?? "General";

                    var payroll = new Payroll
                    {
                        Id = Guid.NewGuid().ToString(),
                        WorkerId = group.Key.WorkerId.ToString(),
                        WorkerName = group.Key.WorkerName,
                        WorkerType = workerType,
                        WeekStart = weekStart,
                        WeekEnd = weekStart.AddDays(6),
                        TotalHours = totalHours,
                        GrossAmount = decimal.Round(gross, 2, MidpointRounding.AwayFromZero),
                        Status = PayrollStatus.Pending
                    };

                    payroll.ApplyDeductions();
                    return payroll;
                })
                .ToList();

            PayrollList.ItemsSource = payrolls;
            PayrollCount.Text = payrolls.Count.ToString();
            TotalPayroll.Text = $"B/.{payrolls.Sum(p => p.NetAmount):F2}";
        }
        catch
        {
        }
    }

    private async void OnViewClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Payroll payroll)
        {
            var html = BuildReceiptHtml(payroll);
            await Navigation.PushModalAsync(new PayrollReceiptPage(html));
            return;
        }

        await DisplayAlertAsync("Ver Detalles", "No se pudo abrir la boleta.", "OK");
    }

    private async void OnPrintClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Payroll payroll)
        {
            var html = BuildReceiptHtml(payroll);
            await Navigation.PushModalAsync(new PayrollReceiptPage(html));
            return;
        }

        await DisplayAlertAsync("Imprimir", "No se pudo abrir la boleta.", "OK");
    }
    private void OnMarkPaidClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Payroll payroll)
        {
            payroll.Status = PayrollStatus.Paid;
            payroll.PaidAt = DateTime.Now;
            PayrollList.ItemsSource = null;
            PayrollList.ItemsSource = payrolls;
        }
    }
    private async void OnProcessPayrollClicked(object sender, EventArgs e)
    {
        await DisplayAlertAsync("Nomina", "Procesando nomina completa.", "OK");
    }

    private string BuildReceiptHtml(Payroll payroll)
    {
        var culture = new CultureInfo("es-PA");
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
