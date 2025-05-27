using PulseERP.Domain.Entities;
using PulseERP.Domain.Pagination;

namespace PulseERP.Domain.Interfaces.Repositories;

public interface IUserRepository
{
    Task<PaginationResult<User>> GetAllAsync(PaginationParams paginationParams);
    Task<User?> GetByIdAsync(Guid id);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(User user);
    Task<bool> ExistsAsync(Guid id);
}
