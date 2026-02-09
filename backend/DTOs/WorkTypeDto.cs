namespace backend.DTOs;
public class WorkTypeDto {
    public string Id { get; set; } = string.Empty;
    public required string Name { get; set; }
    public required double DefaultRate { get; set; }
}