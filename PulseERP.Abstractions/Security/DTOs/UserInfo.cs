namespace PulseERP.Abstractions.Security.DTOs;

public sealed record UserInfo(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string Role
);
