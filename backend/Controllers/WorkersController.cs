using Microsoft.AspNetCore.Mvc;
using backend.DTOs;
using backend.Models;
using backend.Services;
using Google.Cloud.Firestore;
namespace backend.Controllers;

[Route("api/workers")]
[ApiController]
public class WorkersController : ControllerBase
{
    private readonly WorkerService _service;
    private readonly AuditLogService _audit;

    public WorkersController(WorkerService service, AuditLogService audit)
    {
        _service = service;
        _audit = audit;
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
        await LogAsync("create", "worker", id, $"Creado trabajador {dto.Name} {dto.LastName}");
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

    // GET api/workers
    [HttpGet]
    public async Task<ActionResult<List<WorkerDto>>> GetAll()
    {
        var workers = await _service.GetAllAsync();
        var list = workers.Select(w => new WorkerDto
        {
            Id = w.Id,
            UserId = w.Worker.UserId,
            Name = w.Worker.Name,
            LastName = w.Worker.LastName,
            Identification = w.Worker.Identification,
            Active = w.Worker.Active
        }).ToList();
        return Ok(list);
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
        await LogAsync("update", "worker", id, $"Actualizado trabajador {dto.Name} {dto.LastName}");
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