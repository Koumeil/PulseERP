using PulseERP.Domain.Entities;

namespace PulseERP.Domain.Interfaces.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string tokenHash);
    Task<RefreshToken?> GetActiveByUserIdAsync(Guid userId);
    Task AddAsync(RefreshToken token);
    Task UpdateAsync(RefreshToken token);
    Task RevokeForUserAsync(Guid userId);
}
