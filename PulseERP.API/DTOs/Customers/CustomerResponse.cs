using PulseERP.Domain.ValueObjects;

namespace PulseERP.API.DTOs.Customers;

public record CustomerResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    Address? Address,
    bool IsActive
);
