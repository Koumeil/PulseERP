namespace PulseERP.Domain.Dtos.Products;

public record ProductDto(
    Guid Id,
    string Name,
    string? Description,
    string? BrandName,
    decimal Price,
    int Quantity
);
