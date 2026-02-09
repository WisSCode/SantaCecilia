namespace backend.DTOs;
public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public required string Email { get; set; }
    public required string Role { get; set; }
    public required bool Validated { get; set; }
}
