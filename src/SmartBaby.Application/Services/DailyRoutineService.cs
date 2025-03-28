using Microsoft.EntityFrameworkCore;
using SmartBaby.Application.Interfaces;
using SmartBaby.Core.Entities;
using SmartBaby.Infrastructure.Data;

namespace SmartBaby.Application.Services;

public class DailyRoutineService : IDailyRoutineService
{
    private readonly ApplicationDbContext _context;

    public DailyRoutineService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DailyRoutine>> GetAllByBabyIdAsync(int babyId)
    {
        return await _context.DailyRoutines
            .Where(d => d.BabyId == babyId)
            .OrderByDescending(d => d.Date)
            .ToListAsync();
    }

    public async Task<DailyRoutine> GetByIdAsync(int id)
    {
        return await _context.DailyRoutines.FindAsync(id);
    }

    public async Task<DailyRoutine> CreateAsync(DailyRoutine dailyRoutine)
    {
        _context.DailyRoutines.Add(dailyRoutine);
        await _context.SaveChangesAsync();
        return dailyRoutine;
    }

    public async Task<DailyRoutine> UpdateAsync(DailyRoutine dailyRoutine)
    {
        _context.Entry(dailyRoutine).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return dailyRoutine;
    }

    public async Task DeleteAsync(int id)
    {
        var dailyRoutine = await _context.DailyRoutines.FindAsync(id);
        if (dailyRoutine != null)
        {
            _context.DailyRoutines.Remove(dailyRoutine);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<DailyRoutine> GetDefaultRoutineAsync(int babyId)
    {
        return await _context.DailyRoutines
            .FirstOrDefaultAsync(d => d.BabyId == babyId && d.IsDefault);
    }

    public async Task<DailyRoutine> GetRoutineForDateAsync(int babyId, DateTime date)
    {
        var routine = await _context.DailyRoutines
            .FirstOrDefaultAsync(d => d.BabyId == babyId && d.Date.Date == date.Date);

        if (routine == null)
        {
            var defaultRoutine = await GetDefaultRoutineAsync(babyId);
            if (defaultRoutine != null)
            {
                routine = new DailyRoutine
                {
                    BabyId = babyId,
                    Date = date,
                    WakeUpTime = defaultRoutine.WakeUpTime,
                    BedTime = defaultRoutine.BedTime,
                    NapCount = defaultRoutine.NapCount,
                    FeedingCount = defaultRoutine.FeedingCount,
                    Notes = defaultRoutine.Notes,
                    IsDefault = false
                };
                await CreateAsync(routine);
            }
        }

        return routine;
    }

    public async Task<DailyRoutine> SetDefaultRoutineAsync(int babyId, DailyRoutine dailyRoutine)
    {
        var existingDefault = await GetDefaultRoutineAsync(babyId);
        if (existingDefault != null)
        {
            existingDefault.IsDefault = false;
            await UpdateAsync(existingDefault);
        }

        dailyRoutine.IsDefault = true;
        return await CreateAsync(dailyRoutine);
    }
} 