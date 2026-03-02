namespace frontend.Models;

public sealed class ReportRow
{
    public string Col1 { get; set; } = string.Empty;
    public string Col2 { get; set; } = string.Empty;
    public string Col3 { get; set; } = string.Empty;
    public string Col4 { get; set; } = string.Empty;
    public string Col3Sub { get; set; } = string.Empty;
    public bool HasCol3Sub => !string.IsNullOrWhiteSpace(Col3Sub);
    public decimal TotalHours { get; set; }
    public decimal TotalAmount { get; set; }
}
