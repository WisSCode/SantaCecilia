using System;

public class WorkerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class TimeEntryDto
{
    public int Id { get; set; }
    public int WorkerId { get; set; }
    public string WorkerName { get; set; } = string.Empty;
    public string ActivityId { get; set; } = string.Empty;
    public string ActivityName { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public string Lote { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int Hours { get; set; }
    public int Minutes { get; set; }
}

// Shape returned by backend for worked times
public class BackendWorkedTimeDto
{
    public string WorkerId { get; set; } = string.Empty;
    public string WorkTypeId { get; set; } = string.Empty;
    public string BatchId { get; set; } = string.Empty;
    public int MinutesWorked { get; set; }
    public DateTime date { get; set; }
}
