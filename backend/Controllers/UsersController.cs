using Microsoft.AspNetCore.Mvc;
using backend.DTOs;
using backend.Models;
using backend.Services;
namespace backend.Controllers;

[Route("api/users")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly UserService _service;

    public UsersController(UserService service)
    {
        _service = service;
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
        return NoContent();
    }
}
