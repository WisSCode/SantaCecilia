using Microsoft.AspNetCore.Mvc;
using backend.DTOs;
using backend.Services;

namespace backend.Controllers;

[Route("api/auditLogs")]
[ApiController]
public class AuditLogsController : ControllerBase
{
    private readonly AuditLogService _service;

    public AuditLogsController(AuditLogService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuditLogDto>>> GetAll([FromQuery] int limit = 200)
    {
        var logs = await _service.GetAllAsync(limit);
        var list = logs.Select(l => new AuditLogDto
        {
            Id = l.Id,
            Action = l.Log.Action,
            Entity = l.Log.Entity,
            EntityId = l.Log.EntityId,
            ActorId = l.Log.ActorId,
            Message = l.Log.Message,
            CreatedAt = l.Log.CreatedAt.ToDateTime()
        }).ToList();

        return Ok(list);
    }
}
