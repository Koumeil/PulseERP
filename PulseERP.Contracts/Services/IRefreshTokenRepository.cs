using PulseERP.Contracts.Dtos.Auth.Token;
using PulseERP.Domain.Entities;

namespace PulseERP.Contracts.Services;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token);
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task UpdateAsync(RefreshToken token);
}
