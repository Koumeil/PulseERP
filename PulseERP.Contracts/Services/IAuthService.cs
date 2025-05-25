using PulseERP.Contracts.Dtos.Auth;

namespace PulseERP.Contracts.Services;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterCommand command);
    Task<AuthResult> LoginAsync(LoginCommand command);
    Task<AuthResult> RefreshTokenAsync(string token, string refreshToken);
    Task LogoutAsync(string refreshToken);
}
