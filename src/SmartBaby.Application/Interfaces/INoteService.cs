using SmartBaby.Core.Entities;

namespace SmartBaby.Application.Interfaces;

public interface INoteService
{
    Task<IEnumerable<Note>> GetAllByBabyIdAsync(int babyId);
    Task<Note> GetByIdAsync(int id);
    Task<Note> CreateAsync(Note note);
    Task<Note> UpdateAsync(Note note);
    Task DeleteAsync(int id);
    Task<IEnumerable<Note>> GetByDateRangeAsync(int babyId, DateTime startDate, DateTime endDate);
} 