using System;

namespace PulseERP.Domain.Query.Customers;

public class CustomerParams
{
    public string? Search { get; set; }
    public string? Status { get; set; }
    public string? Type { get; set; }
    public bool? IsVIP { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
