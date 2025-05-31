using PulseERP.Abstractions.Common.Filters;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Domain.Entities;
using PulseERP.Domain.ValueObjects;

namespace PulseERP.Domain.Interfaces;

public interface IUserQueryRepository
{
    Task<PagedResult<User>> GetAllAsync(UserFilter userFilter);
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(EmailAddress email);
    Task<bool> ExistsAsync(Guid id);
    Task<User?> GetByRefreshTokenAsync(string refreshToken);
    Task<User?> GetByResetTokenAsync(string resetToken);
}
