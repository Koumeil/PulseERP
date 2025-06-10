// RefreshTokenValidationResult.cs
namespace PulseERP.Abstractions.Security.DTOs;

public sealed record RefreshTokenValidationResult(bool IsValid, Guid? UserId);

