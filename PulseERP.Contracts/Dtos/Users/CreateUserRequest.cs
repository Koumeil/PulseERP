namespace PulseERP.Contracts.Dtos.Users;

public record CreateUserRequest(string FirstName, string LastName, string Email, string? Phone);
