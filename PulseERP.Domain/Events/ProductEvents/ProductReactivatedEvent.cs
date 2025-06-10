using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.ProductEvents;

/// <summary>
/// Raised when a previously discontinued product is reactivated.
/// </summary>
public sealed class ProductReactivatedEvent(Guid productId) : IDomainEvent
{
    public Guid ProductId { get; } = productId;
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
