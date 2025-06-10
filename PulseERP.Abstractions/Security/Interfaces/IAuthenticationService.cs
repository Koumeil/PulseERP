using PulseERP.Abstractions.Security.DTOs;

namespace PulseERP.Abstractions.Security.Interfaces;

/// <summary>
///  Service for managing registration, authentication, and token renewal.
/// Auth service — all requests use strongly-typed DTOs (ValueObjects inside).
/// </summary>
public interface IAuthenticationService
{
    Task<AuthResponse> LoginAsync(LoginRequest request, string ipAddress, string userAgent);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken, string ipAddress, string userAgent);
    Task ActivateAccountAsync(ActivateAccountRequest request);
    Task LogoutAsync(RefreshTokenDto refreshTokenDto);
}
