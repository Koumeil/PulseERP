using PulseERP.Shared.Dtos.Auth.Token;

namespace PulseERP.Application.Interfaces;

public interface ITokenService
{
    AccessTokenDto GenerateAccessToken(Guid userId, string email, string role);
    Task<RefreshTokenDto> GenerateRefreshTokenAsync(
        Guid userId,
        string ipAddress,
        string userAgent
    );
    Task<RefreshTokenValidationResult> ValidateAndRevokeRefreshTokenAsync(string token);
    Guid? ValidateAccessToken(string token);
}
