using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.ProductEvents;

/// <summary>
/// Domain event raised when a product is marked as out of stock.
/// Used for triggering stock alerts, notifications, or analytics.
/// </summary>
public sealed class ProductMarkedOutOfStockEvent : IDomainEvent
{
    /// <inheritdoc/>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    /// <summary>
    /// Unique identifier of the product.
    /// </summary>
    public Guid ProductId { get; }

    /// <summary>
    /// Creates a new <see cref="ProductMarkedOutOfStockEvent"/>.
    /// </summary>
    /// <param name="productId">The unique ID of the product marked as out of stock.</param>
    public ProductMarkedOutOfStockEvent(Guid productId)
    {
        ProductId = productId;
    }
}
