using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBaby.Application.Interfaces;
using SmartBaby.Core.Entities;

namespace SmartBaby.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CryingController : ControllerBase
{
    private readonly ICryingService _cryingService;

    public CryingController(ICryingService cryingService)
    {
        _cryingService = cryingService;
    }

    [HttpGet("baby/{babyId}")]
    public async Task<ActionResult<IEnumerable<Crying>>> GetAllByBabyId(int babyId)
    {
        var cryings = await _cryingService.GetAllByBabyIdAsync(babyId);
        return Ok(cryings);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Crying>> GetById(int id)
    {
        var crying = await _cryingService.GetByIdAsync(id);
        if (crying == null)
        {
            return NotFound();
        }
        return Ok(crying);
    }

    [HttpPost]
    public async Task<ActionResult<Crying>> Create(Crying crying)
    {
        var createdCrying = await _cryingService.CreateAsync(crying);
        return CreatedAtAction(nameof(GetById), new { id = createdCrying.Id }, createdCrying);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Crying>> Update(int id, Crying crying)
    {
        if (id != crying.Id)
        {
            return BadRequest();
        }

        var updatedCrying = await _cryingService.UpdateAsync(crying);
        return Ok(updatedCrying);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await _cryingService.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("baby/{babyId}/range")]
    public async Task<ActionResult<IEnumerable<Crying>>> GetByDateRange(int babyId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var cryings = await _cryingService.GetByDateRangeAsync(babyId, startDate, endDate);
        return Ok(cryings);
    }
} 