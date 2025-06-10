namespace PulseERP.Abstractions.Security.DTOs;

public record ActivationTokenValidationResult(
    bool IsValid,
    Guid? UserId);
