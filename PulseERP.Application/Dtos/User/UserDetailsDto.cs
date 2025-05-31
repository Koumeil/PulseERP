namespace PulseERP.Application.Dtos.User;

public record UserDetailsDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string Role,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLogin,
    int FailedLoginAttempts
);
