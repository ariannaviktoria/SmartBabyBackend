using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartBaby.Core.DTOs;
using SmartBaby.Core.Entities;
using SmartBaby.Core.Interfaces;
using SmartBaby.Infrastructure.Data;

namespace SmartBaby.Application.Services;

public class BabyService : IBabyService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public BabyService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Baby?> GetBabyByIdAsync(int id)
    {
        return await _context.Babies
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<IEnumerable<Baby>> GetBabiesByUserIdAsync(string userId)
    {
        return await _context.Babies
            .Where(b => b.UserId == userId)
            .ToListAsync();
    }

    public async Task<Baby> CreateBabyAsync(Baby baby)
    {
        await _context.Babies.AddAsync(baby);
        await _context.SaveChangesAsync();
        return baby;
    }

    public async Task<bool> UpdateBabyAsync(Baby baby)
    {
        var existingBaby = await GetBabyByIdAsync(baby.Id);
        if (existingBaby == null) return false;

        existingBaby.Name = baby.Name;
        existingBaby.DateOfBirth = baby.DateOfBirth;
        existingBaby.Gender = baby.Gender;
        existingBaby.LastMood = baby.LastMood;
        existingBaby.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteBabyAsync(int id)
    {
        var baby = await GetBabyByIdAsync(id);
        if (baby == null) return false;

        _context.Babies.Remove(baby);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<SleepPeriod>> GetSleepPeriodsAsync(int babyId)
    {
        return await _context.SleepPeriods
            .Where(s => s.BabyId == babyId)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Feeding>> GetFeedingsAsync(int babyId)
    {
        return await _context.Feedings
            .Where(f => f.BabyId == babyId)
            .OrderByDescending(f => f.Time)
            .ToListAsync();
    }

    public async Task<IEnumerable<Crying>> GetCryingsAsync(int babyId)
    {
        return await _context.Cryings
            .Where(c => c.BabyId == babyId)
            .OrderByDescending(c => c.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Note>> GetNotesAsync(int babyId)
    {
        return await _context.Notes
            .Where(n => n.BabyId == babyId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }
} 