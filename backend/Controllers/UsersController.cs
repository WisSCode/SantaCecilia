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

    public UsersController(UserService service)
    {
        _service = service;
    }
    //POST api/users/{id}
    [HttpPost("{id}")]
    public async Task<IActionResult> Create(string id, [FromBody] UserDto dto)
    {
        var user = new Users
        {
            Email = dto.Mail,
            Role = dto.Role,
            Validated = dto.Validated,
            CreatedAt = Timestamp.GetCurrentTimestamp()
        };

        await _service.CreateAsync(id, user);
        return CreatedAtAction(nameof(Get), new { id }, dto);
    }
    //GET api/users/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> Get(string id) 
    {
        var user = await _service.GetAsync(id);
        if (user == null)
            return NotFound();
        var dto = new UserDto
        {
            Mail = user.Email,
            Role = user.Role,
            Validated = user.Validated,
            CreatedAt = user.CreatedAt.ToDateTime(),
        };
        return Ok(dto);
    }

    // GET api/users
    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetAll()
    {
        var users = await _service.GetAllAsync();
        var list = users.Select(u => new UserDto
        {
            Mail = u.Email,
            Role = u.Role,
            Validated = u.Validated,
            CreatedAt = u.CreatedAt.ToDateTime()
        }).ToList();
        return Ok(list);
    }

    //PUT api/users/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(String id, [FromBody] UserDto dto) 
    {
        var user = await _service.GetAsync(id);

        if (user == null)
            return NotFound();

        user.Email = dto.Mail;
        user.Role = dto.Role;
        user.Validated = dto.Validated;

        await _service.UpdateAsync(id, user);
        return NoContent();
    }
}