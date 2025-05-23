namespace PulseERP.Application.DTOs.Auth;

public record RegisterCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string ConfirmPassword,
    string? Phone = null
);
