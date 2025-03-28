using Microsoft.EntityFrameworkCore;
using SmartBaby.Application.Interfaces;
using SmartBaby.Core.Entities;
using SmartBaby.Infrastructure.Data;

namespace SmartBaby.Application.Services;

public class CryingService : ICryingService
{
    private readonly ApplicationDbContext _context;

    public CryingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Crying>> GetAllByBabyIdAsync(int babyId)
    {
        return await _context.Cryings
            .Where(c => c.BabyId == babyId)
            .OrderByDescending(c => c.StartTime)
            .ToListAsync();
    }

    public async Task<Crying> GetByIdAsync(int id)
    {
        return await _context.Cryings.FindAsync(id);
    }

    public async Task<Crying> CreateAsync(Crying crying)
    {
        _context.Cryings.Add(crying);
        await _context.SaveChangesAsync();
        return crying;
    }

    public async Task<Crying> UpdateAsync(Crying crying)
    {
        _context.Entry(crying).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return crying;
    }

    public async Task DeleteAsync(int id)
    {
        var crying = await _context.Cryings.FindAsync(id);
        if (crying != null)
        {
            _context.Cryings.Remove(crying);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Crying>> GetByDateRangeAsync(int babyId, DateTime startDate, DateTime endDate)
    {
        return await _context.Cryings
            .Where(c => c.BabyId == babyId && c.StartTime >= startDate && c.StartTime <= endDate)
            .OrderByDescending(c => c.StartTime)
            .ToListAsync();
    }
} 