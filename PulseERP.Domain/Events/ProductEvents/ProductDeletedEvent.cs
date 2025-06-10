using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.ProductEvents;

/// <summary>
/// Raised when a product is marked as discontinued.
/// </summary>
public sealed class ProductDeletedEvent(Guid productId) : IDomainEvent
{
    public Guid ProductId { get; } = productId;
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
