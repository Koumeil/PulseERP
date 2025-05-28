using PulseERP.Shared.Dtos.Brands;

namespace PulseERP.Shared.Dtos.Products;

public record ProductDto(
    Guid Id,
    string Name,
    string? Description,
    string? BrandName,
    decimal Price,
    int Quantity
);
