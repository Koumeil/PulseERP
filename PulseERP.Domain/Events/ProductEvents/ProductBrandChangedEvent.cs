using PulseERP.Domain.Entities;
using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.ProductEvents;

/// <summary>
/// Raised when the brand of a product changes.
/// </summary>
public sealed class ProductBrandChangedEvent : IDomainEvent
{
    public Guid ProductId { get; }
    public Brand NewBrand { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public ProductBrandChangedEvent(Guid productId, Brand newBrand)
    {
        ProductId = productId;
        NewBrand = newBrand;
    }
}
