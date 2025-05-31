namespace PulseERP.Abstractions.Common.Filters;

public sealed record UserFilter(
    string? Search,
    string? Role,
    bool? IsActive,
    string? Sort,
    int PageNumber = 1,
    int PageSize = 12
);
