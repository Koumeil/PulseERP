namespace PulseERP.Domain.Dtos.Products;

public record CreateProductRequest(
    string Name,
    string? Description,
    string Brand,
    decimal Price,
    int Quantity,
    bool IsService = false
);
