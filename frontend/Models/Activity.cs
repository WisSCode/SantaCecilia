namespace frontend.Models;

public class Activity
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal HourlyRate { get; set; }
}

public class TimeEntry
{
    public string Id { get; set; } = string.Empty;
    public string WorkerId { get; set; } = string.Empty;
    public string WorkerName { get; set; } = string.Empty;
    public string ActivityId { get; set; } = string.Empty;
    public string ActivityName { get; set; } = string.Empty;
    public string BatchId { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public string Lote { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int Hours { get; set; }
    public int Minutes { get; set; }
    public int MinutesWorked => Hours * 60 + Minutes;
    public decimal GetTotalAmount()
    {
        var totalMinutes = Hours * 60 + Minutes;
        var totalHours = totalMinutes / 60m;
        return totalHours * Rate;
    }

    public string FormatTime()
    {
        return $"{Hours}h {Minutes}m";
    }
}
