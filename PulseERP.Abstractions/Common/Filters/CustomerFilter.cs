namespace PulseERP.Abstractions.Common.Filters;

public sealed record CustomerFilter(
    string? Search,
    string? Status,
    string? Type,
    bool? IsVip,
    Guid? AssignedToUserId,
    int PageNumber = 1,
    int PageSize = 12
);
