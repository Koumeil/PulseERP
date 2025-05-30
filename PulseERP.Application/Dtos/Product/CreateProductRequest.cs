namespace PulseERP.Application.Dtos.Product;

public class CreateProductRequest
{
    public string Name { get; init; }
    public string? Description { get; init; }
    public string Brand { get; init; }
    public decimal Price { get; init; }
    public int Quantity { get; init; }
    public bool IsService { get; init; }
}
