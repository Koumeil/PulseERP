using PulseERP.Domain.Entities;
using PulseERP.Domain.Pagination;
using PulseERP.Domain.Query.Users;

namespace PulseERP.Domain.Interfaces.Repositories;

public interface IUserQueryRepository
{
    Task<PaginationResult<User>> GetAllAsync(UserParams userParams);
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(Email email);
    Task<bool> ExistsAsync(Guid id);
    Task<User?> GetByRefreshTokenAsync(string refreshToken);
    Task<User?> GetByResetTokenAsync(string resetToken);
}
