using Microsoft.AspNetCore.Mvc;
using backend.DTOs;
using backend.Models;
using backend.Services;
namespace backend.Controllers;

[Route("api/workTypes")]
[ApiController]
public class WorkTypesController : ControllerBase
{
    private readonly WorkTypeService _service;

    public WorkTypesController(WorkTypeService service)
    {
        _service = service;
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
        return NoContent();
    }
}