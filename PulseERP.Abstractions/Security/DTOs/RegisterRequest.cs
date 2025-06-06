namespace PulseERP.Abstractions.Security.DTOs;

/// <summary>
/// Registration request with strong typing (Value Objects).
/// </summary>
public record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string PhoneNumber
);
