namespace PulseERP.Application.Dtos.Token;

public record RefreshTokenValidationResult(bool IsValid, Guid? UserId);
