using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.ProductEvents;

public sealed class ProductDeactivatedEvent : IDomainEvent
{
    public Guid ProductId { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public ProductDeactivatedEvent(Guid productId)
    {
        ProductId = productId;
    }
}
