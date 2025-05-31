namespace PulseERP.Application.Products.Models;

public sealed record ProductDetails(
    Guid Id,
    string Name,
    string? Description,
    string BrandName,
    decimal Price,
    int Quantity,
    bool IsService,
    string Status,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
