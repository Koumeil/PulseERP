namespace PulseERP.Contracts.Dtos.Users;

public record CreateUserCommand(string FirstName, string LastName, string Email, string? Phone);
