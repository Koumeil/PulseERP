namespace PulseERP.Abstractions.Common.DTOs.Products.Models;

public sealed record ProductDetails
{
    public Guid Id { get; init; }
    public string Name { get; init; } = default!;
    public string? Description { get; init; }
    public string BrandName { get; init; } = default!;
    public decimal Price { get; init; }
    public int Quantity { get; init; }
    public bool IsService { get; init; }
    public string Status { get; init; } = default!;
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public DateTime? LastSoldAt { get; init; }
}
