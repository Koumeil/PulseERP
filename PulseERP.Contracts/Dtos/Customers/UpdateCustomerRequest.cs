namespace PulseERP.Contracts.Dtos.Customers;

public record UpdateCustomerRequest(
    string? FirstName,
    string? LastName,
    string? Email,
    string? Phone,
    string? Street,
    string? City,
    string? ZipCode,
    string? Country
);
