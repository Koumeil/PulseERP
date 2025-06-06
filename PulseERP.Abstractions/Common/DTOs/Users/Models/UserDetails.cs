// Models/UserDetails.cs
namespace PulseERP.Abstractions.Common.DTOs.Users.Models;

public sealed record UserDetails(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string Role,
    bool IsActive,
    bool IsDeleted,
    DateTime CreatedAt,
    DateTime? LastLogin,
    int FailedLoginAttempts
);
