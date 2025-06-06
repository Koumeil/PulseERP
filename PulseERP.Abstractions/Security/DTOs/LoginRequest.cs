namespace PulseERP.Abstractions.Security.DTOs;

/// <summary>
/// Login request with strong typing (Value Objects).
/// </summary>
public record LoginRequest(string Email, string Password);
