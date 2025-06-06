// Commands/UpdateCustomerCommand.cs
namespace PulseERP.Abstractions.Common.DTOs.Customers.Commands;

public sealed record UpdateCustomerCommand(
    string? FirstName = null,
    string? LastName = null,
    string? CompanyName = null,
    string? Email = null,
    string? Phone = null,
    string? Street = null,
    string? City = null,
    string? ZipCode = null,
    string? Country = null,
    string? Type = null,
    string? Status = null,
    string? Industry = null,
    string? Source = null,
    bool? IsVIP = null
);
