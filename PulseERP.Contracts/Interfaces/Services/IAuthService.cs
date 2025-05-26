using PulseERP.Contracts.Dtos.Auth;
using PulseERP.Contracts.Dtos.Services;

namespace PulseERP.Contracts.Interfaces.Services;

public interface IAuthService
{
    Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest command);
    Task<ServiceResult<AuthResponse>> RegisterAsync(RegisterRequest command);
    Task<ServiceResult<AuthResponse>> RefreshTokenAsync(string token, string refreshToken);
    Task LogoutAsync(string refreshToken);
}
