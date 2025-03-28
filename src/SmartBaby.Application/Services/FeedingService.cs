using Microsoft.EntityFrameworkCore;
using SmartBaby.Application.Interfaces;
using SmartBaby.Core.Entities;
using SmartBaby.Infrastructure.Data;

namespace SmartBaby.Application.Services;

public class FeedingService : IFeedingService
{
    private readonly ApplicationDbContext _context;

    public FeedingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Feeding>> GetAllByBabyIdAsync(int babyId)
    {
        return await _context.Feedings
            .Where(f => f.BabyId == babyId)
            .OrderByDescending(f => f.Time)
            .ToListAsync();
    }

    public async Task<Feeding> GetByIdAsync(int id)
    {
        return await _context.Feedings.FindAsync(id);
    }

    public async Task<Feeding> CreateAsync(Feeding feeding)
    {
        _context.Feedings.Add(feeding);
        await _context.SaveChangesAsync();
        return feeding;
    }

    public async Task<Feeding> UpdateAsync(Feeding feeding)
    {
        _context.Entry(feeding).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return feeding;
    }

    public async Task DeleteAsync(int id)
    {
        var feeding = await _context.Feedings.FindAsync(id);
        if (feeding != null)
        {
            _context.Feedings.Remove(feeding);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Feeding>> GetByDateRangeAsync(int babyId, DateTime startDate, DateTime endDate)
    {
        return await _context.Feedings
            .Where(f => f.BabyId == babyId && f.Time >= startDate && f.Time <= endDate)
            .OrderByDescending(f => f.Time)
            .ToListAsync();
    }
} 