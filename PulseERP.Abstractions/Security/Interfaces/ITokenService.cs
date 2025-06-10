using PulseERP.Abstractions.Security.DTOs;

namespace PulseERP.Abstractions.Security.Interfaces;

public interface ITokenService
{
    AccessToken GenerateAccessToken(Guid userId, string email, string role);
    Task<RefreshTokenDto> GenerateRefreshTokenAsync(
        Guid userId,
        string ipAddress,
        string userAgent
    );
    Task<RefreshTokenValidationResult> ValidateAndRevokeRefreshTokenAsync(string token);
    Guid? ValidateAccessToken(string token);
    Task<string> GenerateActivationTokenAsync(Guid userId);

    Task<ActivationTokenValidationResult> ValidateAndRevokeActivationTokenAsync(string token);

}
