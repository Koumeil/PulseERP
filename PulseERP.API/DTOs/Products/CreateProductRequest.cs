namespace PulseERP.API.DTOs.Products;

public record CreateProductRequest(
    string Name,
    string? Description,
    string Brand,
    decimal Price,
    int Quantity
);
