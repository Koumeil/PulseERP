namespace PulseERP.Application.Interfaces;

using PulseERP.Application.DTOs.Auth;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterCommand command);
    Task<AuthResult> LoginAsync(LoginCommand command);
    Task<AuthResult> RefreshTokenAsync(string token, string refreshToken);
    Task LogoutAsync(string refreshToken);
}
