using PulseERP.Domain.Entities;

namespace PulseERP.Domain.Interfaces.Repositories;

public interface IUserCommandRepository
{
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(User user);
    Task<int> SaveChangesAsync();
}
