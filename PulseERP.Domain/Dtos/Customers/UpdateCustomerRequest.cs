using PulseERP.Domain.Dtos.Emails;
using PulseERP.Domain.Dtos.Phones;

namespace PulseERP.Domain.Dtos.Customers;

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
