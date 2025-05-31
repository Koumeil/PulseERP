namespace PulseERP.Application.Products.Commands;

public sealed record CreateProductCommand(
    string Name,
    string? Description,
    string Brand, 
    decimal Price,
    int Quantity,
    bool IsService
);
