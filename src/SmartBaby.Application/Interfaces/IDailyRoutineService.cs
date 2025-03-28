using SmartBaby.Core.Entities;

namespace SmartBaby.Application.Interfaces;

public interface IDailyRoutineService
{
    Task<IEnumerable<DailyRoutine>> GetAllByBabyIdAsync(int babyId);
    Task<DailyRoutine> GetByIdAsync(int id);
    Task<DailyRoutine> CreateAsync(DailyRoutine dailyRoutine);
    Task<DailyRoutine> UpdateAsync(DailyRoutine dailyRoutine);
    Task DeleteAsync(int id);
    Task<DailyRoutine> GetDefaultRoutineAsync(int babyId);
    Task<DailyRoutine> GetRoutineForDateAsync(int babyId, DateTime date);
    Task<DailyRoutine> SetDefaultRoutineAsync(int babyId, DailyRoutine dailyRoutine);
} 