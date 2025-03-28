using SmartBaby.Core.Entities;

namespace SmartBaby.Application.Interfaces;

public interface ISleepService
{
    Task<IEnumerable<SleepPeriod>> GetAllByBabyIdAsync(int babyId);
    Task<SleepPeriod> GetByIdAsync(int id);
    Task<SleepPeriod> CreateAsync(SleepPeriod sleepPeriod);
    Task<SleepPeriod> UpdateAsync(SleepPeriod sleepPeriod);
    Task DeleteAsync(int id);
    Task<IEnumerable<SleepPeriod>> GetByDateRangeAsync(int babyId, DateTime startDate, DateTime endDate);
} 