using PulseERP.Contracts.Dtos.Address;

namespace PulseERP.Contracts.Dtos.Customers;

public record CreateCustomerRequest(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    AddressDto Address
);
