// Models/BrandSummary.cs
namespace PulseERP.Application.Brands.Models;

public sealed record BrandSummary
{
    public Guid Id { get; init; }
    public string Name { get; init; } = default!;
    public bool IsActive { get; init; }
    public IReadOnlyCollection<Guid> ProductIds { get; init; } = new List<Guid>();
    public int ProductCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
