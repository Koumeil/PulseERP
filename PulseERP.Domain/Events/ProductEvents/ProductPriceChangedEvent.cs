using PulseERP.Domain.Interfaces;
using PulseERP.Domain.VO;

namespace PulseERP.Domain.Events.ProductEvents;

/// <summary>
/// Raised when a product's price changes.
/// </summary>
public sealed class ProductPriceChangedEvent : IDomainEvent
{
    public Guid ProductId { get; }
    public Money NewPrice { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public ProductPriceChangedEvent(Guid productId, Money newPrice)
    {
        ProductId = productId;
        NewPrice = newPrice;
    }
}
