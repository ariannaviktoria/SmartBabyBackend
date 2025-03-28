using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBaby.Application.Interfaces;
using SmartBaby.Core.Entities;

namespace SmartBaby.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FeedingController : ControllerBase
{
    private readonly IFeedingService _feedingService;

    public FeedingController(IFeedingService feedingService)
    {
        _feedingService = feedingService;
    }

    [HttpGet("baby/{babyId}")]
    public async Task<ActionResult<IEnumerable<Feeding>>> GetAllByBabyId(int babyId)
    {
        var feedings = await _feedingService.GetAllByBabyIdAsync(babyId);
        return Ok(feedings);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Feeding>> GetById(int id)
    {
        var feeding = await _feedingService.GetByIdAsync(id);
        if (feeding == null)
        {
            return NotFound();
        }
        return Ok(feeding);
    }

    [HttpPost]
    public async Task<ActionResult<Feeding>> Create(Feeding feeding)
    {
        var createdFeeding = await _feedingService.CreateAsync(feeding);
        return CreatedAtAction(nameof(GetById), new { id = createdFeeding.Id }, createdFeeding);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Feeding>> Update(int id, Feeding feeding)
    {
        if (id != feeding.Id)
        {
            return BadRequest();
        }

        var updatedFeeding = await _feedingService.UpdateAsync(feeding);
        return Ok(updatedFeeding);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await _feedingService.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("baby/{babyId}/range")]
    public async Task<ActionResult<IEnumerable<Feeding>>> GetByDateRange(int babyId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var feedings = await _feedingService.GetByDateRangeAsync(babyId, startDate, endDate);
        return Ok(feedings);
    }
} 