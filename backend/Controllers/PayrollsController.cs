using Microsoft.AspNetCore.Mvc;
using backend.DTOs;
using backend.Models;
using backend.Services;
using Google.Cloud.Firestore;
using System.Linq;
namespace backend.Controllers;

[Route("api/payrolls")]
[ApiController]
public class PayrollsController : ControllerBase
{
    private readonly PayrollService _service;
    private readonly WorkTypeService _workTypeService;
    private readonly WorkedTimeService _workedTimeService;
    private readonly AuditLogService _audit;

    public PayrollsController(PayrollService service, WorkTypeService workTypeService, WorkedTimeService workedTimeService, AuditLogService audit)
    {
        _service = service;
        _workTypeService = workTypeService;
        _workedTimeService = workedTimeService;
        _audit = audit;
    }
    //POST api/payrolls/{id}
    [HttpPost("{id}")]
    public async Task<IActionResult> Create(string id, [FromBody] PayrollDto dto)
    {
        var payroll = new Payrolls
        {
            WorkerId = dto.WorkerId,
            WeekStart = Timestamp.FromDateTime(dto.WeekStart.ToUniversalTime()),
            WeekEnd = Timestamp.FromDateTime(dto.WeekEnd.ToUniversalTime()),
            TotalMinutes = dto.TotalMinutes,
            GrossAmount = dto.GrossAmount,
            Status = dto.Status,
            PaidAt = dto.PaidAt != null? Timestamp.FromDateTime(dto.PaidAt.Value.ToUniversalTime()) : null,
        };

        await _service.CreateAsync(id, payroll);
        await LogAsync("create", "payroll", id, $"Creada nomina para trabajador {dto.WorkerId}");
        return CreatedAtAction(nameof(Get), new { id }, dto);
    }
    //GET api/payrolls/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Payrolls>> Get(string id) 
    {
        var payroll= await _service.GetAsync(id);
        if (payroll == null)
            return NotFound();
        var dto = new PayrollDto
        {
            WorkerId = payroll.WorkerId,
            WeekStart = payroll.WeekStart.ToDateTime(),
            WeekEnd = payroll.WeekEnd.ToDateTime(),
            TotalMinutes = payroll.TotalMinutes,
            GrossAmount = payroll.GrossAmount,
            Status = payroll.Status,
            PaidAt = payroll.PaidAt?.ToDateTime()
        };
        return Ok(dto);
    }

    // GET api/payrolls
    [HttpGet]
    public async Task<ActionResult<List<PayrollDto>>> GetAll()
    {
        var payrolls = await _service.GetAllAsync();
        var list = payrolls.Select(p => new PayrollDto
        {
            Id = p.Id,
            WorkerId = p.Payroll.WorkerId,
            WeekStart = p.Payroll.WeekStart.ToDateTime(),
            WeekEnd = p.Payroll.WeekEnd.ToDateTime(),
            TotalMinutes = p.Payroll.TotalMinutes,
            GrossAmount = p.Payroll.GrossAmount,
            Status = p.Payroll.Status,
            PaidAt = p.Payroll.PaidAt?.ToDateTime()
        }).ToList();
        return Ok(list);
    }

    //PUT api/payrolls/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(String id, [FromBody] PayrollDto dto) 
    {
        var payroll = await _service.GetAsync(id);

        if (payroll == null)
            return NotFound();

        payroll.WorkerId = dto.WorkerId;
        payroll.WeekStart = Timestamp.FromDateTime(dto.WeekStart.ToUniversalTime());
        payroll.WeekEnd = Timestamp.FromDateTime(dto.WeekEnd.ToUniversalTime());
        payroll.TotalMinutes = dto.TotalMinutes;
        payroll.GrossAmount = dto.GrossAmount;
        payroll.Status = dto.Status;
        payroll.PaidAt = dto.PaidAt != null? Timestamp.FromDateTime(dto.PaidAt.Value.ToUniversalTime()) : null;

        await _service.UpdateAsync(id, payroll);
        await LogAsync("update", "payroll", id, $"Actualizada nomina para trabajador {dto.WorkerId}");
        return NoContent();
    }

    // POST api/payrolls/process
    [HttpPost("process")]
    public async Task<IActionResult> Process([FromBody] PayrollProcessRequest request)
    {
        var weekStart = request.WeekStart.Date;
        var weekEnd = weekStart.AddDays(6);

        var workTypes = await _workTypeService.GetAllAsync();
        var workTypeRateMap = workTypes.ToDictionary(wt => wt.Id, wt => wt.WorkType.DefaultRate);

        var workedTimes = await _workedTimeService.GetAllAsync();
        var weekEntries = workedTimes
            .Where(wt => wt.WorkedTime.date.ToDateTime().Date >= weekStart && wt.WorkedTime.date.ToDateTime().Date <= weekEnd)
            .ToList();

        var payrollCount = 0;
        foreach (var group in weekEntries.GroupBy(wt => wt.WorkedTime.WorkerId))
        {
            var totalMinutes = group.Sum(wt => wt.WorkedTime.MinutesWorked);
            var gross = group.Sum(wt =>
            {
                var minutes = wt.WorkedTime.MinutesWorked;
                var rate = workTypeRateMap.GetValueOrDefault(wt.WorkedTime.WorkTypeId, 0);
                return (minutes / 60.0) * rate;
            });

            var payroll = new Payrolls
            {
                WorkerId = group.Key,
                WeekStart = Timestamp.FromDateTime(weekStart.ToUniversalTime()),
                WeekEnd = Timestamp.FromDateTime(weekEnd.ToUniversalTime()),
                TotalMinutes = totalMinutes,
                GrossAmount = gross,
                Status = "Pending",
                PaidAt = null
            };

            var payrollId = $"{group.Key}_{weekStart:yyyyMMdd}";
            await _service.CreateAsync(payrollId, payroll);
            payrollCount++;
        }

        await LogAsync("process", "payroll", weekStart.ToString("yyyy-MM-dd"), $"Procesada nomina semanal. Registros: {payrollCount}");
        return Ok(new { Count = payrollCount });
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