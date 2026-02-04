namespace backend.DTOs;
public class WorkerDto
{
    public required string? UserId { get; set; }
    public required string Name { get; set; }
    public required string LastName { get; set; }
    public required string Identification { get; set; }
    public required bool Active { get; set; }
}