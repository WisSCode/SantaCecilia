using Microsoft.AspNetCore.Mvc;
using backend.DTOs;
using backend.Models;
using backend.Services;
using Google.Cloud.Firestore;
namespace backend.Controllers;

[Route("api/workTimes")]
[ApiController]
public class WorkTimesController : ControllerBase
{
    private readonly WorkedTimeService _service;
    private readonly AuditLogService _audit;

    public WorkTimesController(WorkedTimeService service, AuditLogService audit)
    {
        _service = service;
        _audit = audit;
    }
    //POST api/workTimes/{id}
    [HttpPost("{id}")]
    public async Task<IActionResult> Create(string id, [FromBody] WorkedTimeDto dto)
    {
        var workedTime = new WorkedTimes
        {
            WorkerId = dto.WorkerId,
            WorkTypeId = dto.WorkTypeId,
            BatchId = dto.BatchId,
            MinutesWorked = dto.MinutesWorked,
            date = Timestamp.FromDateTime(dto.date.ToUniversalTime())
        };

        await _service.CreateAsync(id, workedTime);
        await LogAsync("create", "workedTime", id, $"Registro creado para trabajador {dto.WorkerId}");
        return CreatedAtAction(nameof(Get), new { id }, dto);
    }
    //GET api/workedTimes/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<WorkedTimeDto>> Get(string id) 
    {
        var workedTime = await _service.GetAsync(id);
        if (workedTime == null)
            return NotFound();
        var dto = new WorkedTimeDto
        {
            WorkerId = workedTime.WorkerId,
            WorkTypeId = workedTime.WorkTypeId,
            BatchId = workedTime.BatchId,
            MinutesWorked = workedTime.MinutesWorked,
            date = workedTime.date.ToDateTime(),
        };
        return Ok(dto);
    }

    // GET api/workTimes
    [HttpGet]
    public async Task<ActionResult<List<WorkedTimeDto>>> GetAll()
    {
        var items = await _service.GetAllAsync();
        var list = items.Select(wt => new WorkedTimeDto
        {
            Id = wt.Id,
            WorkerId = wt.WorkedTime.WorkerId,
            WorkTypeId = wt.WorkedTime.WorkTypeId,
            BatchId = wt.WorkedTime.BatchId,
            MinutesWorked = wt.WorkedTime.MinutesWorked,
            date = wt.WorkedTime.date.ToDateTime()
        }).ToList();
        return Ok(list);
    }

    //PUT api/workedTimes/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(String id, [FromBody] WorkedTimeDto dto) 
    {
        var workedTime = await _service.GetAsync(id);

        if (workedTime == null)
            return NotFound();

        workedTime.WorkerId = dto.WorkerId;
        workedTime.WorkTypeId = dto.WorkTypeId;
        workedTime.BatchId = dto.BatchId;
        workedTime.MinutesWorked = dto.MinutesWorked;
        workedTime.date = Timestamp.FromDateTime(dto.date.ToUniversalTime());

        await _service.UpdateAsync(id, workedTime);
        await LogAsync("update", "workedTime", id, $"Registro actualizado para trabajador {dto.WorkerId}");
        return NoContent();
    }

    //DELETE api/workedTimes/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var workedTime = await _service.GetAsync(id);
        
        if (workedTime == null)
            return NotFound();

        await _service.DeleteAsync(id);
        await LogAsync("delete", "workedTime", id, $"Registro eliminado para trabajador {workedTime.WorkerId}");
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