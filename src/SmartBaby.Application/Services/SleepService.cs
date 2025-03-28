using Microsoft.EntityFrameworkCore;
using SmartBaby.Application.Interfaces;
using SmartBaby.Core.Entities;
using SmartBaby.Infrastructure.Data;

namespace SmartBaby.Application.Services;

public class SleepService : ISleepService
{
    private readonly ApplicationDbContext _context;

    public SleepService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SleepPeriod>> GetAllByBabyIdAsync(int babyId)
    {
        return await _context.SleepPeriods
            .Where(s => s.BabyId == babyId)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<SleepPeriod> GetByIdAsync(int id)
    {
        return await _context.SleepPeriods.FindAsync(id);
    }

    public async Task<SleepPeriod> CreateAsync(SleepPeriod sleepPeriod)
    {
        _context.SleepPeriods.Add(sleepPeriod);
        await _context.SaveChangesAsync();
        return sleepPeriod;
    }

    public async Task<SleepPeriod> UpdateAsync(SleepPeriod sleepPeriod)
    {
        _context.Entry(sleepPeriod).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return sleepPeriod;
    }

    public async Task DeleteAsync(int id)
    {
        var sleepPeriod = await _context.SleepPeriods.FindAsync(id);
        if (sleepPeriod != null)
        {
            _context.SleepPeriods.Remove(sleepPeriod);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<SleepPeriod>> GetByDateRangeAsync(int babyId, DateTime startDate, DateTime endDate)
    {
        return await _context.SleepPeriods
            .Where(s => s.BabyId == babyId && s.StartTime >= startDate && s.StartTime <= endDate)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();
    }
} 