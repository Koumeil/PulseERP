namespace PulseERP.Contracts.Dtos.Products;

public record CreateProductCommand(
    string Name,
    string? Description,
    string Brand,
    decimal Price,
    int Quantity,
    bool IsService = false
);
