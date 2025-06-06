// Commands/UpdateUserCommand.cs
namespace PulseERP.Abstractions.Common.DTOs.Users.Commands;

public sealed record UpdateUserCommand(
    string? FirstName = null,
    string? LastName = null,
    string? Email = null,
    string? Phone = null,
    string? Role = null
);
