namespace PulseERP.Abstractions.Common.DTOs.Products.Commands;

public sealed record CreateProductCommand(
    string Name,
    string Description,
    string BrandName,
    decimal Price,
    int Quantity,
    bool IsService
);
