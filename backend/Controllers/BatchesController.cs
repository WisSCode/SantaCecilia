using Microsoft.AspNetCore.Mvc;
using backend.DTOs;
using backend.Models;
using backend.Services;
namespace backend.Controllers;

[Route("api/batcches")]
[ApiController]
public class BatchesController : ControllerBase
{
    private readonly BatchService _service;

    public BatchesController(BatchService service)
    {
        _service = service;
    }
    //POST api/batches/{id}
    [HttpPost("{id}")]
    public async Task<IActionResult> Create(string id, [FromBody] BatchDto dto)
    {
        var batch = new Batches
        {
            Name = dto.Name,
            Location = dto.Location
        };

        await _service.CreateAsync(id, batch);
        return CreatedAtAction(nameof(Get), new { id }, dto);
    }
    //GET api/batches/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<BatchDto>> Get(string id) 
    {
        var batch = await _service.GetAsync(id);
        if (batch == null)
            return NotFound();
        var dto = new BatchDto
        {
            Name = batch.Name,
            Location = batch.Location
        };
        return Ok(dto);
    }

    //PUT api/batches/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(String id, [FromBody] BatchDto dto) 
    {
        var batch = await _service.GetAsync(id);

        if (batch == null)
            return NotFound();

        batch.Name = dto.Name;
        batch.Location = dto.Location;

        await _service.UpdateAsync(id, batch);
        return NoContent();
    }
}