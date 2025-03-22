using SmartBaby.Core.Entities;

namespace SmartBaby.Core.Interfaces;

public interface IBabyService
{
    Task<Baby?> GetBabyByIdAsync(int id);
    Task<IEnumerable<Baby>> GetBabiesByUserIdAsync(string userId);
    Task<Baby> CreateBabyAsync(Baby baby);
    Task<bool> UpdateBabyAsync(Baby baby);
    Task<bool> DeleteBabyAsync(int id);
    Task<IEnumerable<SleepPeriod>> GetSleepPeriodsAsync(int babyId);
    Task<IEnumerable<Feeding>> GetFeedingsAsync(int babyId);
    Task<IEnumerable<Crying>> GetCryingsAsync(int babyId);
    Task<IEnumerable<Note>> GetNotesAsync(int babyId);
} 