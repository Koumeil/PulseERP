namespace PulseERP.Contracts.Dtos.Products;

public record UpdateProductRequest(
    string? Name,
    string? Description,
    string? Brand,
    decimal? Price,
    int? Quantity
);
