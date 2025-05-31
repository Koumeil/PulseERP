namespace PulseERP.Abstractions.Common.Filters;

public sealed record ProductFilter(
    string? Brand,
    string? Sort,
    string? Search,
    int PageNumber = 1,
    int PageSize = 12
);
