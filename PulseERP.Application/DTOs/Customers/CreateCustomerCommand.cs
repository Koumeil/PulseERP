namespace PulseERP.Application.DTOs.Customers;

public record CreateCustomerCommand(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string Street,
    string City,
    string ZipCode,
    string Country
);
