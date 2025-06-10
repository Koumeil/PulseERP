using PulseERP.Abstractions.Common.Filters;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Domain.Entities;
using PulseERP.Domain.VO;

namespace PulseERP.Abstractions.Contracts.Repositories;
public interface IUserRepository
{
    Task<PagedResult<User>> GetAllAsync(UserFilter filter);
    Task<User?> FindByIdAsync(Guid id, bool bypassCache = false);
    Task<User?> FindByEmailAsync(EmailAddress email, bool bypassCache = false);
    Task<bool> ExistsAsync(Guid id);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(User user);
    Task<int> SaveChangesAsync();
}
