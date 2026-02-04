namespace backend.DTOs;
public class WorkedTimeDto
{
    public required string WorkerId { get; set; }
    public required string WorkTypeId { get; set; }
    public required string BatchId { get; set; }
    public required int MinutesWorked { get; set; }
    public required DateTime date { get; set; }
}
