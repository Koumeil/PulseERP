namespace PulseERP.Application.Dtos.Customer;

public record CustomerDto(
    Guid Id,
    string FirstName,
    string LastName,
    string CompanyName,
    string Email,
    string Phone,
    string Address,
    string Type, // Lead, Prospect, Client, Former
    string Status, // Active, Inactive, Pending, InDispute
    DateTime FirstContactDate,
    DateTime? LastInteractionDate,
    Guid? AssignedToUserId,
    string? Industry,
    string? Source,
    bool IsVIP,
    bool IsActive,
    List<string> Notes,
    List<string> Tags
);
