namespace PulseERP.Application.Products.Models;

public sealed record ProductSummary(
    Guid Id,
    string Name,
    string? Description,
    string BrandName,
    decimal Price,
    int Quantity,
    bool IsService,
    string Status, // InStock, LowStock…
    bool IsActive
);
