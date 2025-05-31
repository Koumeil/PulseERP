namespace PulseERP.Application.Dtos.Customer;

public class UpdateCustomerRequest
{
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? CompanyName { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Street { get; init; }
    public string? City { get; init; }
    public string? ZipCode { get; init; }
    public string? Country { get; init; }
    public string? Type { get; init; }
    public string? Status { get; init; }
    public string? Industry { get; init; }
    public string? Source { get; init; }
    public bool? IsVIP { get; init; }
}
