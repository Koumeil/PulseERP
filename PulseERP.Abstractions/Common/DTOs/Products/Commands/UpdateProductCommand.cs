namespace PulseERP.Abstractions.Common.DTOs.Products.Commands;

public sealed record UpdateProductCommand(
    string? Name = null,
    string? Description = null,
    string? BrandName = null,
    decimal? Price = null,
    int? Quantity = null,
    bool? IsService = null
);
