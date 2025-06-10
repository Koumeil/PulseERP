// Models/CustomerDetails.cs
namespace PulseERP.Abstractions.Common.DTOs.Customers.Models;

public sealed record CustomerDetails(
    Guid Id,
    string FirstName,
    string LastName,
    string CompanyName,
    string Email,
    string Phone,
    string Address,
    string Type,
    string Status,
    DateTime FirstContactDate,
    DateTime? LastInteractionDate,
    Guid? AssignedToUserId,
    string? Industry,
    string? Source,
    bool IsVip,
    bool IsActive,
    List<string> Notes,
    List<string> Tags
);
