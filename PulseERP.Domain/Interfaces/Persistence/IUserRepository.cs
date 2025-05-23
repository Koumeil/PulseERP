using PulseERP.Domain.Entities;

namespace PulseERP.Domain.Interfaces.Persistence;

public interface IUserRepository
{
    Task AddAsync(User user);
    Task DeleteAsync(User user);
    Task<bool> ExistsAsync(Guid id);
    Task<IReadOnlyList<User>> GetAllAsync();
    Task<User?> GetByIdAsync(Guid id);
    Task UpdateAsync(User user);
}
