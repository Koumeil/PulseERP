namespace PulseERP.Domain.Dtos.Auth.Token;

public record RefreshTokenResponse(string Token, string RefreshToken, Guid UserId);
