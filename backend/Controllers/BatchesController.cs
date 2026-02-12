using Microsoft.AspNetCore.Mvc;
using backend.DTOs;
using backend.Models;
using backend.Services;
using Google.Cloud.Firestore;
namespace backend.Controllers;

[Route("api/batches")]
[ApiController]
public class BatchesController : ControllerBase
{
    private readonly BatchService _service;
    private readonly AuditLogService _audit;

    public BatchesController(BatchService service, AuditLogService audit)
    {
        _service = service;
        _audit = audit;
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
        await LogAsync("create", "batch", id, $"Creado lote {dto.Name}");
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

    // GET api/batches
    [HttpGet]
    public async Task<ActionResult<List<BatchDto>>> GetAll()
    {
        var batches = await _service.GetAllAsync();
        var list = batches.Select(b => new BatchDto
        {
            Id = b.Id,
            Name = b.Batch.Name,
            Location = b.Batch.Location
        }).ToList();
        return Ok(list);
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
        await LogAsync("update", "batch", id, $"Actualizado lote {dto.Name}");
        return NoContent();
    }

    //DELETE api/batches/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var batch = await _service.GetAsync(id);
        if (batch == null)
            return NotFound();

        await _service.DeleteAsync(id);
        await LogAsync("delete", "batch", id, $"Eliminado lote {batch.Name}");
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