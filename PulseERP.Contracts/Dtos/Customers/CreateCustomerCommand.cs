using PulseERP.Contracts.Dtos.Address;

namespace PulseERP.Contracts.Dtos.Customers;

public record CreateCustomerCommand(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    AddressDto Address
);
