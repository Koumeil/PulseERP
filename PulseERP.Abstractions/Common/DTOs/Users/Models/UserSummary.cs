// Models/UserSummary.cs
namespace PulseERP.Abstractions.Common.DTOs.Users.Models;

public sealed record UserSummary(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string Role,
    bool IsActive,
    bool RequirePasswordChange,
    bool IsDeleted,
    DateTime? LastLoginDate,
    int FailedLoginAttempts,
    DateTime? LockoutEnd
);
