using PulseERP.Domain.ValueObjects.Adresses;
using PulseERP.Domain.ValueObjects.Passwords;

namespace PulseERP.Domain.Security.DTOs;

/// <summary>
/// Login request with strong typing (Value Objects).
/// </summary>
public record LoginRequest(string Email, string Password);
