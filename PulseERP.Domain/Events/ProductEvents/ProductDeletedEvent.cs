using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.ProductEvents;

/// <summary>
/// Raised when a product is marked as discontinued.
/// </summary>
public sealed class ProductDeletedEvent : IDomainEvent
{
    public Guid ProductId { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public ProductDeletedEvent(Guid productId)
    {
        ProductId = productId;
    }
}
