using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBaby.Application.Interfaces;
using SmartBaby.Core.Entities;

namespace SmartBaby.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SleepController : ControllerBase
{
    private readonly ISleepService _sleepService;

    public SleepController(ISleepService sleepService)
    {
        _sleepService = sleepService;
    }

    [HttpGet("baby/{babyId}")]
    public async Task<ActionResult<IEnumerable<SleepPeriod>>> GetAllByBabyId(int babyId)
    {
        var sleepPeriods = await _sleepService.GetAllByBabyIdAsync(babyId);
        return Ok(sleepPeriods);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SleepPeriod>> GetById(int id)
    {
        var sleepPeriod = await _sleepService.GetByIdAsync(id);
        if (sleepPeriod == null)
        {
            return NotFound();
        }
        return Ok(sleepPeriod);
    }

    [HttpPost]
    public async Task<ActionResult<SleepPeriod>> Create(SleepPeriod sleepPeriod)
    {
        var createdSleepPeriod = await _sleepService.CreateAsync(sleepPeriod);
        return CreatedAtAction(nameof(GetById), new { id = createdSleepPeriod.Id }, createdSleepPeriod);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<SleepPeriod>> Update(int id, SleepPeriod sleepPeriod)
    {
        if (id != sleepPeriod.Id)
        {
            return BadRequest();
        }

        var updatedSleepPeriod = await _sleepService.UpdateAsync(sleepPeriod);
        return Ok(updatedSleepPeriod);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await _sleepService.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("baby/{babyId}/range")]
    public async Task<ActionResult<IEnumerable<SleepPeriod>>> GetByDateRange(int babyId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var sleepPeriods = await _sleepService.GetByDateRangeAsync(babyId, startDate, endDate);
        return Ok(sleepPeriods);
    }
} 