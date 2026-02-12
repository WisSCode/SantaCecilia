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
    public int SequentialId { get; set; }

    public string DisplayId => $"TRB-{SequentialId:D3}";
    public string HoursDisplay => $"{TotalHours:F1}h";
    public string GrossDisplay => $"B/.{GrossAmount:F2}";
    public decimal DeductionsTotal => SocialSecurity + EducationalInsurance + UnionFee;
    public string DeductionsDisplay => $"-B/.{DeductionsTotal:F2}";
    public string NetDisplay => $"B/.{NetAmount:F2}";

    public string Initial => string.IsNullOrEmpty(WorkerName) ? "?" : WorkerName[..1].ToUpper();
    public Microsoft.Maui.Graphics.Color InitialColor
    {
        get
        {
            if (string.IsNullOrEmpty(WorkerName)) return Microsoft.Maui.Graphics.Color.FromArgb("#9EC9B4");
            var hash = WorkerName.GetHashCode();
            var colors = new[] { "#9EC9B4", "#F4A261", "#7BAFD4", "#D4A0C0", "#A8C97E", "#C9A86C" };
            return Microsoft.Maui.Graphics.Color.FromArgb(colors[Math.Abs(hash) % colors.Length]);
        }
    }

    public string StatusText => Status switch
    {
        PayrollStatus.Pending => "Pendiente",
        PayrollStatus.Paid => "Pagado",
        PayrollStatus.Cancelled => "Cancelado",
        _ => "Desconocido"
    };

    public bool IsPending => Status == PayrollStatus.Pending;

    public Microsoft.Maui.Graphics.Color StatusBgColor => Status switch
    {
        PayrollStatus.Pending => Microsoft.Maui.Graphics.Color.FromArgb("#FFF4E1"),
        PayrollStatus.Paid => Microsoft.Maui.Graphics.Color.FromArgb("#E8F5EE"),
        PayrollStatus.Cancelled => Microsoft.Maui.Graphics.Color.FromArgb("#FDECEA"),
        _ => Microsoft.Maui.Graphics.Color.FromArgb("#F5F0E8")
    };

    public Microsoft.Maui.Graphics.Color StatusTextColor => Status switch
    {
        PayrollStatus.Pending => Microsoft.Maui.Graphics.Color.FromArgb("#B87333"),
        PayrollStatus.Paid => Microsoft.Maui.Graphics.Color.FromArgb("#2E7D5B"),
        PayrollStatus.Cancelled => Microsoft.Maui.Graphics.Color.FromArgb("#C0392B"),
        _ => Microsoft.Maui.Graphics.Color.FromArgb("#7A6E5D")
    };

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

public enum PayrollStatus
{
    Pending = 0,
    Paid = 1,
    Cancelled = 2
}

public class PayrollActivityEntry
{
    public DateTime Date { get; set; }
    public string ActivityName { get; set; } = string.Empty;
    public string BatchName { get; set; } = string.Empty;
    public decimal Hours { get; set; }
    public decimal Rate { get; set; }
    public decimal Amount { get; set; }
}
