// AuthResponse.cs
namespace PulseERP.Abstractions.Security.DTOs;

public sealed record AuthResponse(
    UserInfo User,
    AccessToken AccessToken,
    RefreshTokenDto RefreshTokenDto
);
