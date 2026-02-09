namespace frontend.Services;

public class WorkerDto
{
    public string Id { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Identification { get; set; } = string.Empty;
    public bool Active { get; set; }
}

public class BatchDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}

public class WorkTypeDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public double DefaultRate { get; set; }
}

public class WorkedTimeDto
{
    public string Id { get; set; } = string.Empty;
    public string WorkerId { get; set; } = string.Empty;
    public string WorkTypeId { get; set; } = string.Empty;
    public string BatchId { get; set; } = string.Empty;
    public int MinutesWorked { get; set; }
    public DateTime Date { get; set; }
}

public class PayrollDto
{
    public string Id { get; set; } = string.Empty;
    public string WorkerId { get; set; } = string.Empty;
    public DateTime WeekStart { get; set; }
    public DateTime WeekEnd { get; set; }
    public int TotalMinutes { get; set; }
    public double GrossAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? PaidAt { get; set; }
}

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool Validated { get; set; }
}
