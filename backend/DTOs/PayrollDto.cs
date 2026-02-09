namespace backend.DTOs;
public class PayrollDto
{
    public string Id { get; set; } = string.Empty;
    public required string WorkerId { get; set; }
    public required DateTime WeekStart { get; set; }
    public required DateTime WeekEnd { get; set; }
    public required int TotalMinutes { get; set; }
    public required double GrossAmount { get; set; }
    public required string Status { get; set; }
    public DateTime? PaidAt { get; set; }
}