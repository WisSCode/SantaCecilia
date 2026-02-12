namespace frontend.Models;

public class TimeEntry
{
    public string Id { get; set; } = string.Empty;
    public string WorkerId { get; set; } = string.Empty;
    public string WorkerName { get; set; } = string.Empty;
    public string ActivityName { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public string Lote { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int Hours { get; set; }
    public int Minutes { get; set; }

    public string Initial => string.IsNullOrEmpty(WorkerName) ? "?" : WorkerName[..1].ToUpper();
    public string TimeDisplay => $"{Hours}h {Minutes}m";
    public string RateDisplay => $"B/.{Rate:F4}";
    public string DateShort => Date.ToString("dd MMM", new System.Globalization.CultureInfo("es-ES"));
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
}
