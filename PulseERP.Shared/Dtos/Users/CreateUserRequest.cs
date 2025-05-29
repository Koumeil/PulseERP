namespace PulseERP.Shared.Dtos.Users;

public record CreateUserRequest(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string Password
);
