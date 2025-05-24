using PulseERP.Domain.ValueObjects;

namespace PulseERP.Application.DTOs.Customers;

public record CreateCustomerCommand(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    Address? Address
);
