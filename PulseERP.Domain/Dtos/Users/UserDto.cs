namespace PulseERP.Domain.Dtos.Users;

public record UserDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string Role
);
