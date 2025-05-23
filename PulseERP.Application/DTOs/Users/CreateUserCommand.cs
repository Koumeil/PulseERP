namespace PulseERP.Application.DTOs.Users;

public record CreateUserCommand(string FirstName, string LastName, string Email, string? Phone);
