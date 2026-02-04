namespace backend.DTOs;
public class UserDto
{
    public required string Mail { get; set; }
    public required string Role { get; set; }
    public required bool Validated { get; set; }
    public required  DateTime CreatedAt { get; set; }
}