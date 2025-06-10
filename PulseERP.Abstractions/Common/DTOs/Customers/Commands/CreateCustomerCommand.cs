// Commands/CreateCustomerCommand.cs
namespace PulseERP.Abstractions.Common.DTOs.Customers.Commands;

public sealed record CreateCustomerCommand(
    string FirstName,
    string LastName,
    string CompanyName,
    string Email,
    string Phone,
    string Street,
    string City,
    string ZipCode,
    string Country,
    string Type, // Lead, Prospect, Client, Former
    string Status, // Active, Inactive…
    string? Industry,
    string? Source,
    bool IsVip
);
