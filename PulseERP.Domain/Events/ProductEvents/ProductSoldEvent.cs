using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.ProductEvents;

/// <summary>
/// Raised when a product is sold.
/// </summary>
public sealed class ProductSoldEvent(Guid productId, int quantitySold, DateTime soldAtUtc) : IDomainEvent
{
    public Guid ProductId { get; } = productId;
    public int QuantitySold { get; } = quantitySold;
    public DateTime SoldAtUtc { get; } = soldAtUtc;
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
