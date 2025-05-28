using PulseERP.Domain.Entities;
using PulseERP.Shared.Dtos.Auth.Token;

namespace PulseERP.Application.Interfaces.Services;

public interface ITokenService
{
    AccessTokenDto GenerateAccessToken(Guid userId, string email, IList<string> roles);
    Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId);
    Guid? ValidateAccessToken(string token);
    Task<bool> ValidateRefreshTokenAsync(string refreshToken, Guid userId);
    Task RevokeRefreshTokenAsync(string refreshToken);
    Task<RefreshToken?> GetRefreshTokenInfoAsync(string token);
}
