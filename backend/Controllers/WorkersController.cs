using Microsoft.AspNetCore.Mvc;
using backend.DTOs;
using backend.Models;
using backend.Services;
namespace backend.Controllers;

[Route("api/workers")]
[ApiController]
public class WorkersController : ControllerBase
{
    private readonly WorkerService _service;

    public WorkersController(WorkerService service)
    {
        _service = service;
    }
    //POST api/workers/{id}
    [HttpPost("{id}")]
    public async Task<IActionResult> Create(string id, [FromBody] WorkerDto dto)
    {
        var worker = new Workers
        {
            UserId = dto.UserId != null ? dto.UserId : null,
            Name = dto.Name,
            LastName = dto.LastName,
            Identification = dto.Identification,
            Active = dto.Active
        };

        await _service.CreateAsync(id, worker);
        return CreatedAtAction(nameof(Get), new { id }, dto);
    }
    //GET api/workers/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<WorkerDto>> Get(string id) 
    {
        var worker = await _service.GetAsync(id);
        if (worker == null)
            return NotFound();
        var dto = new WorkerDto
        {
            UserId = worker.UserId,
            Name = worker.Name,
            LastName = worker.LastName,
            Identification = worker.Identification,
            Active = worker.Active
        };
        return Ok(dto);
    }

    //PUT api/workers/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(String id, [FromBody] WorkerDto dto) 
    {
        var worker = await _service.GetAsync(id);

        if (worker == null)
            return NotFound();

        worker.UserId = dto.UserId;
        worker.Name = dto.Name;
        worker.LastName = dto.LastName;
        worker.Identification = dto.Identification;
        worker.Active = dto.Active;

        await _service.UpdateAsync(id, worker);
        return NoContent();
    }
}