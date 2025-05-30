namespace PulseERP.Application.Dtos.Product;

public record UpdateProductRequest(
    string? Name,
    string? Description,
    Guid? BrandId,
    decimal? Price,
    int? Quantity,
    bool? IsService
);
