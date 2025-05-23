namespace PulseERP.Application.DTOs.Users;

public record UpdateUserCommand(
    Guid Id,
    string? FirstName,
    string? LastName,
    string? Email,
    string? Phone
);
