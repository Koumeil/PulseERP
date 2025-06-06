using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.StockEvents;

public sealed class ProductStockDecreasedEvent : IDomainEvent
{
    public Guid ProductId { get; }
    public int Quantity { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public ProductStockDecreasedEvent(Guid productId, int quantity)
    {
        ProductId = productId;
        Quantity = quantity;
    }
}
