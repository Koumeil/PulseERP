namespace PulseERP.Abstractions.Security.DTOs;

public sealed record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string Password);
