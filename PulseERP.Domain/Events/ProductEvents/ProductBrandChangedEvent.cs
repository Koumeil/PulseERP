using PulseERP.Domain.Entities;
using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.ProductEvents;

/// <summary>
/// Raised when the brand of a product changes.
/// </summary>
public sealed class ProductBrandChangedEvent(Guid productId, Brand newBrand) : IDomainEvent
{
    public Guid ProductId { get; } = productId;
    public Brand NewBrand { get; } = newBrand;
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
