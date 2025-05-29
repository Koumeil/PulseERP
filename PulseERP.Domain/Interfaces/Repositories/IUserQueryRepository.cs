using PulseERP.Domain.Entities;
using PulseERP.Domain.Pagination;
using PulseERP.Domain.ValueObjects;

namespace PulseERP.Domain.Interfaces.Repositories;

public interface IUserQueryRepository
{
    Task<PaginationResult<User>> GetAllAsync(PaginationParams pagination);
    Task<User?> GetByIdAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<User?> GetByEmailAsync(Email email);
    Task<User?> GetByRefreshTokenAsync(string refreshToken);
    Task<User?> GetByResetTokenAsync(string resetToken);
}