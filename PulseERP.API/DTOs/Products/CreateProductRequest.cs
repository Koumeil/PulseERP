namespace PulseERP.API.DTOs.Products;

public record CreateProductRequest(string Name, string? Description, decimal Price, int Quantity);
