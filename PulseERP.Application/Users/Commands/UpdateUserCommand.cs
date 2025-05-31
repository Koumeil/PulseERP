// Commands/UpdateUserCommand.cs
namespace PulseERP.Application.Users.Commands;

public sealed record UpdateUserCommand(
    string? FirstName = null,
    string? LastName = null,
    string? Email = null,
    string? Phone = null,
    string? Role = null
);
