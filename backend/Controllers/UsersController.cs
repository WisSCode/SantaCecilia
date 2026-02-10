using Microsoft.AspNetCore.Mvc;
using backend.DTOs;
using backend.Models;
using backend.Services;
using Google.Cloud.Firestore;
namespace backend.Controllers;

[Route("api/users")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly UserService _service;
    private readonly AuditLogService _audit;

    public UsersController(UserService service, AuditLogService audit)
    {
        _service = service;
        _audit = audit;
    }

    // GET api/users
    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetAll()
    {
        var users = await _service.GetAllAsync();
        var list = users.Select(u => new UserDto
        {
            Id = u.Id,
            Email = u.User.Email,
            Role = u.User.Role,
            Validated = u.User.Validated
        }).ToList();
        return Ok(list);
    }

    // GET api/users/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> Get(string id)
    {
        var user = await _service.GetAsync(id);
        if (user == null)
            return NotFound();
        return Ok(new UserDto
        {
            Id = id,
            Email = user.Email,
            Role = user.Role,
            Validated = user.Validated
        });
    }

    // PUT api/users/{id}/validate
    [HttpPut("{id}/validate")]
    public async Task<IActionResult> Validate(string id)
    {
        var user = await _service.GetAsync(id);
        if (user == null)
            return NotFound();

        user.Validated = true;
        await _service.UpdateAsync(id, user);
        await LogAsync("validate", "user", id, $"Usuario validado {user.Email}");
        return NoContent();
    }

    // PUT api/users/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UserDto dto)
    {
        var user = await _service.GetAsync(id);
        if (user == null)
            return NotFound();

        user.Email = dto.Email;
        user.Role = dto.Role;
        user.Validated = dto.Validated;

        await _service.UpdateAsync(id, user);
        await LogAsync("update", "user", id, $"Usuario actualizado {dto.Email}");
        return NoContent();
    }

    private string GetActorId()
    {
        if (Request.Headers.TryGetValue("X-User-Id", out var actorId))
            return actorId.ToString();

        return "system";
    }

    private Task LogAsync(string action, string entity, string entityId, string message)
    {
        return _audit.CreateAsync(new AuditLog
        {
            Action = action,
            Entity = entity,
            EntityId = entityId,
            ActorId = GetActorId(),
            Message = message,
            CreatedAt = Timestamp.GetCurrentTimestamp()
        });
    }
}
