namespace PulseERP.Shared.Dtos.Auth.Token;

public record AccessTokenDto(string Token, DateTime ExpiresAt);
