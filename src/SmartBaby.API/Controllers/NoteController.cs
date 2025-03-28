using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBaby.Application.Interfaces;
using SmartBaby.Core.Entities;

namespace SmartBaby.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NoteController : ControllerBase
{
    private readonly INoteService _noteService;

    public NoteController(INoteService noteService)
    {
        _noteService = noteService;
    }

    [HttpGet("baby/{babyId}")]
    public async Task<ActionResult<IEnumerable<Note>>> GetAllByBabyId(int babyId)
    {
        var notes = await _noteService.GetAllByBabyIdAsync(babyId);
        return Ok(notes);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Note>> GetById(int id)
    {
        var note = await _noteService.GetByIdAsync(id);
        if (note == null)
        {
            return NotFound();
        }
        return Ok(note);
    }

    [HttpPost]
    public async Task<ActionResult<Note>> Create(Note note)
    {
        var createdNote = await _noteService.CreateAsync(note);
        return CreatedAtAction(nameof(GetById), new { id = createdNote.Id }, createdNote);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Note>> Update(int id, Note note)
    {
        if (id != note.Id)
        {
            return BadRequest();
        }

        var updatedNote = await _noteService.UpdateAsync(note);
        return Ok(updatedNote);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await _noteService.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("baby/{babyId}/range")]
    public async Task<ActionResult<IEnumerable<Note>>> GetByDateRange(int babyId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var notes = await _noteService.GetByDateRangeAsync(babyId, startDate, endDate);
        return Ok(notes);
    }
} 