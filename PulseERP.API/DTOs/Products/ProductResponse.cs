namespace PulseERP.API.DTOs.Products;

public record ProductResponse(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    int Quantity
);
