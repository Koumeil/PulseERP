namespace PulseERP.Shared.Dtos.Users;

public record UpdateUserRequest(
    Guid Id,
    string? FirstName,
    string? LastName,
    string? Email,
    string? Phone
);
