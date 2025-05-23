namespace PulseERP.API.DTOs.Users;

public record CreateUserRequest(string FirstName, string LastName, string Email, string? Phone);
