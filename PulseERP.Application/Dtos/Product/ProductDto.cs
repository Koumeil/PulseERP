namespace PulseERP.Application.Dtos.Product;

public record ProductDto(
    Guid Id,
    string Name,
    string? Description,
    string BrandName,
    decimal Price,
    int Quantity,
    bool IsService,
    string Status, // InStock, LowStock, OutOfStock, Discontinued
    bool IsActive
);
