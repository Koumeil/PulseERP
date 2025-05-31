// Models/UserDetails.cs
namespace PulseERP.Application.Users.Models;

public sealed record UserDetails(
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
