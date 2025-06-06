using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.StockEvents;

public sealed class ProductRestockedEvent : IDomainEvent
{
    public Guid ProductId { get; }
    public int Quantity { get; }

    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public ProductRestockedEvent(Guid productId, int quantity)
    {
        ProductId = productId;
        Quantity = quantity;
    }
}
