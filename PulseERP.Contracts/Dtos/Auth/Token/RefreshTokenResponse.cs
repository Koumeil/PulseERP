namespace PulseERP.Contracts.Dtos.Auth.Token;

public record RefreshTokenResponse(string Token, string RefreshToken, Guid UserId);
