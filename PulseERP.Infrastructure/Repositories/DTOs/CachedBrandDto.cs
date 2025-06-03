namespace PulseERP.Infrastructure.Repositories.DTOs;

public sealed record CachedBrandDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = default!;
    public bool IsActive { get; init; }
    public List<Guid> ProductIds { get; init; } = new();
    public int ProductCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
