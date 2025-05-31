namespace PulseERP.Application.Dtos.User;

public record UserInfo(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string Role
);
