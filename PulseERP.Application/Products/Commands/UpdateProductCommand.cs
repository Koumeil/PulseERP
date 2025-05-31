namespace PulseERP.Application.Products.Commands;

public sealed record UpdateProductCommand(
    string? Name = null,
    string? Description = null,
    Guid? BrandId = null,
    decimal? Price = null,
    int? Quantity = null,
    bool? IsService = null
);
