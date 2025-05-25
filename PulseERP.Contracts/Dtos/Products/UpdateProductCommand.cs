namespace PulseERP.Contracts.Dtos.Products;

public record UpdateProductCommand(
    Guid Id,
    string? Name,
    string? Description,
    decimal? Price,
    int? Quantity
);
