using PulseERP.Domain.ValueObjects;
using PulseERP.Domain.ValueObjects.Adresses;
using PulseERP.Domain.ValueObjects.Passwords;

namespace PulseERP.Domain.Security.DTOs;

/// <summary>
/// Registration request with strong typing (Value Objects).
/// </summary>
public record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string Phone
);
