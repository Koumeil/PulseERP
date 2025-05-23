namespace PulseERP.API.DTOs.Products;

public record UpdateProductRequest(
    string? Name,
    string? Description,
    decimal? Price,
    int? Quantity
);
