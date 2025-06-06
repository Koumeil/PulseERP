// Commands/CreateUserCommand.cs
namespace PulseERP.Abstractions.Common.DTOs.Users.Commands;

public sealed record CreateUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string Password
);
