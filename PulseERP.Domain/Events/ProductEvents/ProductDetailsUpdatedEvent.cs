using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.ProductEvents;

/// <summary>
/// Raised when product details like name, description, price or type are updated.
/// </summary>
public sealed class ProductDetailsUpdatedEvent : IDomainEvent
{
    public Guid ProductId { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public ProductDetailsUpdatedEvent(Guid productId)
    {
        ProductId = productId;
    }
}
