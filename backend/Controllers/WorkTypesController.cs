using Microsoft.AspNetCore.Mvc;
using backend.DTOs;
using backend.Models;
using backend.Services;
using Google.Cloud.Firestore;
namespace backend.Controllers;

[Route("api/workTypes")]
[ApiController]
public class WorkTypesController : ControllerBase
{
    private readonly WorkTypeService _service;
    private readonly AuditLogService _audit;

    public WorkTypesController(WorkTypeService service, AuditLogService audit)
    {
        _service = service;
        _audit = audit;
    }
    //POST api/WorkTypes/{id}
    [HttpPost("{id}")]
    public async Task<IActionResult> Create(string id, [FromBody] WorkTypes dto)
    {
        var workTypes = new WorkTypes
        {
            Name = dto.Name,
            DefaultRate = dto.DefaultRate,
        };

        await _service.CreateAsync(id, workTypes);
        await LogAsync("create", "workType", id, $"Creado tipo de trabajo {dto.Name}");
        return CreatedAtAction(nameof(Get), new { id }, dto);
    }
    //GET api/WorkTypes/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<WorkTypeDto>> Get(string id) 
    {
        var workTypes= await _service.GetAsync(id);
        if (workTypes == null)
            return NotFound();
        var dto = new WorkTypeDto
        {
            Name= workTypes.Name,
            DefaultRate = workTypes.DefaultRate
        };
        return Ok(dto);
    }

    // GET api/workTypes
    [HttpGet]
    public async Task<ActionResult<List<WorkTypeDto>>> GetAll()
    {
        var workTypes = await _service.GetAllAsync();
        var list = workTypes.Select(wt => new WorkTypeDto
        {
            Id = wt.Id,
            Name = wt.WorkType.Name,
            DefaultRate = wt.WorkType.DefaultRate
        }).ToList();
        return Ok(list);
    }

    //PUT api/WorkTypes/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(String id, [FromBody] WorkTypeDto dto) 
    {
        var workTypes = await _service.GetAsync(id);

        if (workTypes == null)
            return NotFound();

        workTypes.Name = dto.Name;
        workTypes.DefaultRate = dto.DefaultRate;

        await _service.UpdateAsync(id, workTypes);
        await LogAsync("update", "workType", id, $"Actualizado tipo de trabajo {dto.Name}");
        return NoContent();
    }

    // DELETE api/workTypes/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var workTypes = await _service.GetAsync(id);
        if (workTypes == null)
            return NotFound();

        await _service.DeleteAsync(id);
        await LogAsync("delete", "workType", id, $"Eliminado tipo de trabajo {workTypes.Name}");
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