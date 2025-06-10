namespace PulseERP.Domain.Events.BrandEvents;

using System;
using Interfaces;

/// <summary>
/// Event raised when a product is removed from a brand.
/// </summary>
public sealed class BrandProductRemovedEvent : IDomainEvent
{
    /// <summary>
    /// UTC timestamp when the product was removed.
    /// </summary>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    /// <summary>
    /// Identifier of the brand.
    /// </summary>
    public Guid BrandId { get; }

    /// <summary>
    /// Identifier of the product that was removed.
    /// </summary>
    public Guid ProductId { get; }

    public BrandProductRemovedEvent(Guid brandId, Guid productId)
    {
        BrandId = brandId;
        ProductId = productId;
    }
}
