using Api.Models;

namespace Api.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<bool> IsEmailTakenAsync(string email, Guid? excludeUserId = null);
    Task<User> CreateAsync(User user);
    Task<IEnumerable<User>> GetAllAsync();
    Task<User> UpdateAsync(User user);
    Task DeleteAsync(User user);
}
