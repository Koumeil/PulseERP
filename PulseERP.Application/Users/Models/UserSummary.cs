// Models/UserSummary.cs
namespace PulseERP.Application.Users.Models;

public sealed record UserSummary(
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
