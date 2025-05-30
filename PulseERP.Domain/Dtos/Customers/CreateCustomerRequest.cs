namespace PulseERP.Domain.Dtos.Customers;

public record CreateCustomerRequest(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string Street,
    string City,
    string ZipCode,
    string Country
);
