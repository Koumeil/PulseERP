namespace PulseERP.Abstractions.Common.Filters;

public sealed record ProductFilter(
    string? Brand = null,
    string? Status = null,
    int? MinStockLevel = null,
    int? MinPrice = null,
    int? MaxPrice = null,
    int? MaxStockLevel = null,
    bool? IsService = null,
    string? Sort = null,
    string? Search = null,
    int PageNumber = 1,
    int PageSize = 12
);
