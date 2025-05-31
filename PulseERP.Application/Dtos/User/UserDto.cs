namespace PulseERP.Application.Dtos.User;

public record UserDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string Role,
    bool IsActive,
    bool RequirePasswordChange,
    DateTime? LastLoginDate,
    int FailedLoginAttempts,
    DateTime? LockoutEnd
);
