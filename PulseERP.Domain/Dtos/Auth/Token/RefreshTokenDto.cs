namespace PulseERP.Domain.Dtos.Auth.Token;

public record RefreshTokenDto(string Token, DateTime Expires);
