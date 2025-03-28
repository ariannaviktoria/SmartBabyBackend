using SmartBaby.Core.Entities;

namespace SmartBaby.Application.Interfaces;

public interface IFeedingService
{
    Task<IEnumerable<Feeding>> GetAllByBabyIdAsync(int babyId);
    Task<Feeding> GetByIdAsync(int id);
    Task<Feeding> CreateAsync(Feeding feeding);
    Task<Feeding> UpdateAsync(Feeding feeding);
    Task DeleteAsync(int id);
    Task<IEnumerable<Feeding>> GetByDateRangeAsync(int babyId, DateTime startDate, DateTime endDate);
} 