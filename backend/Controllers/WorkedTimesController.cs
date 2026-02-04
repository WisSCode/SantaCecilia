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

    public WorkTimesController(WorkedTimeService service)
    {
        _service = service;
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
        return NoContent();
    }
}