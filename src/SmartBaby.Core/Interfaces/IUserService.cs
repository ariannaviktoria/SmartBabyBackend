using SmartBaby.Core.Entities;

namespace SmartBaby.Core.Interfaces;

public interface IUserService
{
    Task<User?> GetUserByIdAsync(string id);
    Task<User?> GetUserByEmailAsync(string email);
    Task<bool> CreateUserAsync(User user, string password);
    Task<bool> UpdateUserAsync(User user);
    Task<bool> DeleteUserAsync(string id);
    Task<bool> ValidateUserAsync(string email, string password);
    Task<string> GenerateJwtTokenAsync(User user);
} 