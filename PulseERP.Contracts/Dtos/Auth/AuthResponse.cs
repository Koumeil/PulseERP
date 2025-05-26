namespace PulseERP.Contracts.Dtos.Auth;

public record AuthResponse(
    string UserId,
    string FirstName,
    string LastName,
    string Email,
    string Token,
    string RefreshToken,
    DateTime ExpiresAt
);
