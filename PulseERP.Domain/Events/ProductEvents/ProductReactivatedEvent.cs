using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.ProductEvents;

/// <summary>
/// Raised when a previously discontinued product is reactivated.
/// </summary>
public sealed class ProductReactivatedEvent : IDomainEvent
{
    public Guid ProductId { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public ProductReactivatedEvent(Guid productId)
    {
        ProductId = productId;
    }
}
