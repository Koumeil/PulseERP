using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.ProductEvents;

public sealed class ProductStockReturnedEvent : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public Guid ProductId { get; }

    public int QuantityReturned { get; }


    public ProductStockReturnedEvent(Guid productId, int quantityReturned)
    {
        ProductId = productId;
        QuantityReturned = quantityReturned;
    }
}
