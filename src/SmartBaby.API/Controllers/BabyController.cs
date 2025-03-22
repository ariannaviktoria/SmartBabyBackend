using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBaby.Core.DTOs;
using SmartBaby.Core.Entities;
using SmartBaby.Core.Interfaces;

namespace SmartBaby.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BabyController : ControllerBase
{
    private readonly IBabyService _babyService;

    public BabyController(IBabyService babyService)
    {
        _babyService = babyService;
    }

    [HttpGet]
    public async Task<IActionResult> GetBabies()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var babies = await _babyService.GetBabiesByUserIdAsync(userId);
        return Ok(babies);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBaby(int id)
    {
        var baby = await _babyService.GetBabyByIdAsync(id);
        if (baby == null)
            return NotFound();

        return Ok(baby);
    }

    [HttpPost]
    public async Task<IActionResult> CreateBaby([FromBody] CreateBabyDto babyDto)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var baby = new Baby
        {
            Name = babyDto.Name,
            DateOfBirth = babyDto.DateOfBirth,
            Gender = babyDto.Gender,
            UserId = userId
        };

        var result = await _babyService.CreateBabyAsync(baby);
        return CreatedAtAction(nameof(GetBaby), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBaby(int id, [FromBody] UpdateBabyDto babyDto)
    {
        var baby = await _babyService.GetBabyByIdAsync(id);
        if (baby == null)
            return NotFound();

        if (babyDto.Name != null)
            baby.Name = babyDto.Name;
        if (babyDto.DateOfBirth.HasValue)
            baby.DateOfBirth = babyDto.DateOfBirth.Value;
        if (babyDto.Gender.HasValue)
            baby.Gender = babyDto.Gender.Value;
        if (babyDto.LastMood != null)
            baby.LastMood = babyDto.LastMood;

        var result = await _babyService.UpdateBabyAsync(baby);
        if (!result)
            return BadRequest();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBaby(int id)
    {
        var result = await _babyService.DeleteBabyAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }
} 