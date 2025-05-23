namespace PulseERP.API.DTOs.Users;

public record UserResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    bool IsActive
);
