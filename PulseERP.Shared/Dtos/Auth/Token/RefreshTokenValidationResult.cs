namespace PulseERP.Shared.Dtos.Auth.Token;

public record RefreshTokenValidationResult(bool IsValid, Guid? UserId);
