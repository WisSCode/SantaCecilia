namespace backend.DTOs;
public class WorkerDto
{
    public string Id { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public required string Name { get; set; }
    public required string LastName { get; set; }
    public required string Identification { get; set; }
    public required bool Active { get; set; }
}