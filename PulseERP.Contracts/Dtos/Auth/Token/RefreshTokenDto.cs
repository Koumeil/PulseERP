namespace PulseERP.Contracts.Dtos.Auth.Token;

public record RefreshTokenDto(
    string Token,
    Guid UserId,
    DateTime Expires,
    DateTime? Revoked,
    string? ReplacedByToken
);
