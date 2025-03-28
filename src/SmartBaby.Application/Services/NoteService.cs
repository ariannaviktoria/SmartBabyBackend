using Microsoft.EntityFrameworkCore;
using SmartBaby.Application.Interfaces;
using SmartBaby.Core.Entities;
using SmartBaby.Infrastructure.Data;

namespace SmartBaby.Application.Services;

public class NoteService : INoteService
{
    private readonly ApplicationDbContext _context;

    public NoteService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Note>> GetAllByBabyIdAsync(int babyId)
    {
        return await _context.Notes
            .Where(n => n.BabyId == babyId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<Note> GetByIdAsync(int id)
    {
        return await _context.Notes.FindAsync(id);
    }

    public async Task<Note> CreateAsync(Note note)
    {
        _context.Notes.Add(note);
        await _context.SaveChangesAsync();
        return note;
    }

    public async Task<Note> UpdateAsync(Note note)
    {
        _context.Entry(note).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return note;
    }

    public async Task DeleteAsync(int id)
    {
        var note = await _context.Notes.FindAsync(id);
        if (note != null)
        {
            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Note>> GetByDateRangeAsync(int babyId, DateTime startDate, DateTime endDate)
    {
        return await _context.Notes
            .Where(n => n.BabyId == babyId && n.CreatedAt >= startDate && n.CreatedAt <= endDate)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }
} 