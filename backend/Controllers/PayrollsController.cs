using Microsoft.AspNetCore.Mvc;
using backend.DTOs;
using backend.Models;
using backend.Services;
using Google.Cloud.Firestore;
namespace backend.Controllers;

[Route("api/payrolls")]
[ApiController]
public class PayrollsController : ControllerBase
{
    private readonly PayrollService _service;

    public PayrollsController(PayrollService service)
    {
        _service = service;
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
        return NoContent();
    }
}