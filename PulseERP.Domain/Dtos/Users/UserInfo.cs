namespace PulseERP.Domain.Dtos.Users;

public record UserInfo(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string Role
);
