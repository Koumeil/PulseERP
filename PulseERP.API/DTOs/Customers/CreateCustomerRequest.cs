using PulseERP.Contracts.Dtos.Address;

namespace PulseERP.API.DTOs.Customers;

public record CreateCustomerRequest(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    AddressDto AddressDto
);
