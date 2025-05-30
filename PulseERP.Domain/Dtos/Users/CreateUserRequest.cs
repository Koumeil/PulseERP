namespace PulseERP.Domain.Dtos.Users;

public record CreateUserRequest(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string Password
);
