namespace PulseERP.Domain.Dtos.Auth.Token;

public record RefreshTokenValidationResult(bool IsValid, Guid? UserId);
