using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBaby.Application.Interfaces;
using SmartBaby.Core.Entities;

namespace SmartBaby.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DailyRoutineController : ControllerBase
{
    private readonly IDailyRoutineService _dailyRoutineService;

    public DailyRoutineController(IDailyRoutineService dailyRoutineService)
    {
        _dailyRoutineService = dailyRoutineService;
    }

    [HttpGet("baby/{babyId}")]
    public async Task<ActionResult<IEnumerable<DailyRoutine>>> GetAllByBabyId(int babyId)
    {
        var routines = await _dailyRoutineService.GetAllByBabyIdAsync(babyId);
        return Ok(routines);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DailyRoutine>> GetById(int id)
    {
        var routine = await _dailyRoutineService.GetByIdAsync(id);
        if (routine == null)
        {
            return NotFound();
        }
        return Ok(routine);
    }

    [HttpPost]
    public async Task<ActionResult<DailyRoutine>> Create(DailyRoutine routine)
    {
        var createdRoutine = await _dailyRoutineService.CreateAsync(routine);
        return CreatedAtAction(nameof(GetById), new { id = createdRoutine.Id }, createdRoutine);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<DailyRoutine>> Update(int id, DailyRoutine routine)
    {
        if (id != routine.Id)
        {
            return BadRequest();
        }

        var updatedRoutine = await _dailyRoutineService.UpdateAsync(routine);
        return Ok(updatedRoutine);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await _dailyRoutineService.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("baby/{babyId}/default")]
    public async Task<ActionResult<DailyRoutine>> GetDefaultRoutine(int babyId)
    {
        var defaultRoutine = await _dailyRoutineService.GetDefaultRoutineAsync(babyId);
        if (defaultRoutine == null)
        {
            return NotFound();
        }
        return Ok(defaultRoutine);
    }

    [HttpGet("baby/{babyId}/date")]
    public async Task<ActionResult<DailyRoutine>> GetRoutineForDate(int babyId, [FromQuery] DateTime date)
    {
        var routine = await _dailyRoutineService.GetRoutineForDateAsync(babyId, date);
        if (routine == null)
        {
            return NotFound();
        }
        return Ok(routine);
    }

    [HttpPost("baby/{babyId}/default")]
    public async Task<ActionResult<DailyRoutine>> SetDefaultRoutine(int babyId, DailyRoutine routine)
    {
        if (babyId != routine.BabyId)
        {
            return BadRequest();
        }

        var defaultRoutine = await _dailyRoutineService.SetDefaultRoutineAsync(babyId, routine);
        return Ok(defaultRoutine);
    }
} 