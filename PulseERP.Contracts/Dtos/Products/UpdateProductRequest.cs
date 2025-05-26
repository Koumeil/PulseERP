namespace PulseERP.Contracts.Dtos.Products;

public record UpdateProductRequest(
    Guid Id,
    string? Name,
    string? Description,
    decimal? Price,
    int? Quantity
);
