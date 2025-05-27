using PulseERP.Contracts.Dtos.Auth;

namespace PulseERP.Application.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest command);
    Task<AuthResponse> RegisterAsync(RegisterRequest command);
    Task<AuthResponse> RefreshTokenAsync(string token, string refreshToken);
    Task LogoutAsync(string refreshToken);
}
