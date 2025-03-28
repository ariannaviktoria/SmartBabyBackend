using SmartBaby.Core.Entities;

namespace SmartBaby.Application.Interfaces;

public interface ICryingService
{
    Task<IEnumerable<Crying>> GetAllByBabyIdAsync(int babyId);
    Task<Crying> GetByIdAsync(int id);
    Task<Crying> CreateAsync(Crying crying);
    Task<Crying> UpdateAsync(Crying crying);
    Task DeleteAsync(int id);
    Task<IEnumerable<Crying>> GetByDateRangeAsync(int babyId, DateTime startDate, DateTime endDate);
} 