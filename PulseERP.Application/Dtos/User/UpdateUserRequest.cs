namespace PulseERP.Application.Dtos.User;

public record UpdateUserRequest(
    string? FirstName,
    string? LastName,
    string? Email,
    string? Phone,
    string? Role
);
