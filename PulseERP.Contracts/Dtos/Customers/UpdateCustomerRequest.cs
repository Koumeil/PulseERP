using PulseERP.Contracts.Dtos.Emails;
using PulseERP.Contracts.Dtos.Phones;

namespace PulseERP.Contracts.Dtos.Customers;

public record UpdateCustomerRequest(
    string? FirstName,
    string? LastName,
    EmailDto? Email,
    PhoneNumberDto? Phone,
    string? Street,
    string? City,
    string? ZipCode,
    string? Country
);
