using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.ProductEvents;

/// <summary>
/// Event raised when a brand is restored from soft-delete.
/// </summary>
public sealed class ProductRestoredEvent(Guid productId) : IDomainEvent
{
    /// <summary>
    /// UTC timestamp when the product was restored.
    /// </summary>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    /// <summary>
    /// Identifier of the restored product.
    /// </summary>
    public Guid ProductId { get; } = productId;
}
