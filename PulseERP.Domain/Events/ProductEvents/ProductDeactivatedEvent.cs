using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.ProductEvents;

public sealed class ProductDeactivatedEvent(Guid productId) : IDomainEvent
{
    public Guid ProductId { get; } = productId;
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
