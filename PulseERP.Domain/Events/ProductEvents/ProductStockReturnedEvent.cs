using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.ProductEvents;

public sealed class ProductStockReturnedEvent(Guid productId, int quantityReturned) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public Guid ProductId { get; } = productId;

    public int QuantityReturned { get; } = quantityReturned;
}
