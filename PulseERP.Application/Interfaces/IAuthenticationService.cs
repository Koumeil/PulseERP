using PulseERP.Shared.Dtos.Auth;

namespace PulseERP.Application.Interfaces;

public interface IAuthenticationService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, string ipAddress, string userAgent);
    Task<AuthResponse> LoginAsync(LoginRequest request, string ipAddress, string userAgent);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken, string ipAddress, string userAgent);
    Task LogoutAsync(string refreshToken);
}
