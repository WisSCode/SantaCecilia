using System.Linq;

namespace frontend.Models;

public class Payroll
{
    public const decimal DefaultSocialSecurityRate = 0.0975m;
    public const decimal DefaultEducationalInsuranceRate = 0.0125m;
    public const decimal DefaultUnionFee = 1.00m;

    public string Id { get; set; } = string.Empty;
    public string WorkerId { get; set; } = string.Empty;
    public string WorkerName { get; set; } = string.Empty;
    public string WorkerType { get; set; } = string.Empty;
    public DateTime WeekStart { get; set; }
    public DateTime WeekEnd { get; set; }
    public decimal TotalHours { get; set; }
    public int TotalMinutes { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal SocialSecurity { get; set; }
    public decimal EducationalInsurance { get; set; }
    public decimal UnionFee { get; set; }
    public decimal NetAmount { get; set; }
    public PayrollStatus Status { get; set; } = PayrollStatus.Pending;
    public DateTime? PaidAt { get; set; }
    public List<DailyWork> DailyWork { get; set; } = new();

    public string StatusText => Status switch
    {
        PayrollStatus.Pending => "Pendiente",
        PayrollStatus.Paid => "Pagado",
        PayrollStatus.Cancelled => "Cancelado",
        _ => "Desconocido"
    };

    public Microsoft.Maui.Graphics.Color StatusColor => Status switch
    {
        PayrollStatus.Pending => Microsoft.Maui.Graphics.Color.FromArgb("#F4A261"),
        PayrollStatus.Paid => Microsoft.Maui.Graphics.Color.FromArgb("#9BC7B3"),
        PayrollStatus.Cancelled => Microsoft.Maui.Graphics.Color.FromArgb("#E76F51"),
        _ => Microsoft.Maui.Graphics.Color.FromArgb("#E2DDD3")
    };

    public bool CanPay => Status == PayrollStatus.Pending;

    public void ApplyDeductions(decimal? unionFee = null, decimal? socialSecurityRate = null, decimal? educationalInsuranceRate = null)
    {
        var ssRate = socialSecurityRate ?? DefaultSocialSecurityRate;
        var eduRate = educationalInsuranceRate ?? DefaultEducationalInsuranceRate;
        var fee = unionFee ?? DefaultUnionFee;

        SocialSecurity = decimal.Round(GrossAmount * ssRate, 2, MidpointRounding.AwayFromZero);
        EducationalInsurance = decimal.Round(GrossAmount * eduRate, 2, MidpointRounding.AwayFromZero);
        UnionFee = fee;
        NetAmount = decimal.Round(GrossAmount - SocialSecurity - EducationalInsurance - UnionFee, 2, MidpointRounding.AwayFromZero);
    }
}

public class DailyWork
{
    public DateTime Date { get; set; }
    public List<WorkActivity> Activities { get; set; } = new();

    public decimal GetDayTotal()
    {
        return Activities.Sum(a => a.Amount);
    }
}

public class WorkActivity
{
    public string ActivityName { get; set; } = string.Empty;
    public string Lote { get; set; } = string.Empty;
    public decimal Hours { get; set; }
    public decimal Rate { get; set; }
    public decimal Amount { get; set; }
}

public enum PayrollStatus
{
    Pending = 0,
    Paid = 1,
    Cancelled = 2
}
