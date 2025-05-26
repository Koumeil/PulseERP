namespace PulseERP.Contracts.Dtos.Users;

public record UpdateUserRequest(
    Guid Id,
    string? FirstName,
    string? LastName,
    string? Email,
    string? Phone
);
