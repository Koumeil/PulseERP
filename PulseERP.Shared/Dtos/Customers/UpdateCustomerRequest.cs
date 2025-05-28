using PulseERP.Shared.Dtos.Emails;
using PulseERP.Shared.Dtos.Phones;

namespace PulseERP.Shared.Dtos.Customers;

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
