namespace PulseERP.Shared.Dtos.Auth.Token;

public record RefreshTokenDto(string Token, DateTime Expires);
