namespace PulseERP.Application.DTOs.Products;

public record CreateProductCommand(string Name, string? Description, decimal Price, int Quantity);
