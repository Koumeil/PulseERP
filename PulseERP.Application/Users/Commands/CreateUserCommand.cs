// Commands/CreateUserCommand.cs
namespace PulseERP.Application.Users.Commands;

public sealed record CreateUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string Password
);
