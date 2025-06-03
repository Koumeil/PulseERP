using PulseERP.Abstractions.Security.DTOs;
using PulseERP.Domain.Security.DTOs;

namespace PulseERP.Domain.Security.Interfaces;

/// <summary>
///  Service for managing registration, authentication, and token renewal.
/// Auth service — all requests use strongly-typed DTOs (ValueObjects inside).
/// </summary>
public interface IAuthenticationService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, string ipAddress, string userAgent);
    Task<AuthResponse> LoginAsync(LoginRequest request, string ipAddress, string userAgent);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken, string ipAddress, string userAgent);
    Task LogoutAsync(RefreshTokenDto refreshTokenDto);
}
