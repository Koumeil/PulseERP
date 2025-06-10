using PulseERP.Domain.Interfaces;
using PulseERP.Domain.VO;

namespace PulseERP.Domain.Events.ProductEvents;

/// <summary>
/// Raised when a product's price changes.
/// </summary>
public sealed class ProductPriceChangedEvent(Guid productId, Money newPrice) : IDomainEvent
{
    public Guid ProductId { get; } = productId;
    public Money NewPrice { get; } = newPrice;
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
