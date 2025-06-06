using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.ProductEvents;

/// <summary>
/// Raised when a product is sold.
/// </summary>
public sealed class ProductSoldEvent : IDomainEvent
{
    public Guid ProductId { get; }
    public int QuantitySold { get; }
    public DateTime SoldAtUtc { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public ProductSoldEvent(Guid productId, int quantitySold, DateTime soldAtUtc)
    {
        ProductId = productId;
        QuantitySold = quantitySold;
        SoldAtUtc = soldAtUtc;
    }
}
