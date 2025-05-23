namespace PulseERP.Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(Guid userId, string email, IList<string> roles);
    Task<string> GenerateRefreshTokenAsync(Guid userId);
    Guid? ValidateAccessToken(string token);
    Task<bool> ValidateRefreshTokenAsync(string refreshToken, Guid userId);
    Task RevokeRefreshTokenAsync(string refreshToken);
}
