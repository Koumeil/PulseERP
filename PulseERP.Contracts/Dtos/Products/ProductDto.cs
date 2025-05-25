using PulseERP.Contracts.Dtos.Brands;

namespace PulseERP.Contracts.Dtos.Products;

public record ProductDto(
    Guid Id,
    string Name,
    string? Description,
    BrandDto Brand,
    decimal Price,
    int Quantity
);
