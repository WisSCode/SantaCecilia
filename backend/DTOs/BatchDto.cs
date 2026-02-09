namespace backend.DTOs;
public class BatchDto
{
    public string Id { get; set; } = string.Empty;
    public required string Name { get; set; }
    public required string Location { get; set; }
}