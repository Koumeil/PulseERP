using PulseERP.Contracts.Dtos.Brands;

namespace PulseERP.Contracts.Dtos.Products;

public record ProductDto(
    Guid Id,
    string Name,
    string? Description,
    string? BrandName,
    decimal Price,
    int Quantity
);
