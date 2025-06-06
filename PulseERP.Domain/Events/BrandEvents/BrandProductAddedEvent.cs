namespace PulseERP.Domain.Events.BrandEvents;

using System;
using PulseERP.Domain.Interfaces;

/// <summary>
/// Event raised when a product is added to a brand.
/// </summary>
public sealed class BrandProductAddedEvent : IDomainEvent
{
    /// <summary>
    /// UTC timestamp when the product was added.
    /// </summary>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    /// <summary>
    /// Identifier of the brand.
    /// </summary>
    public Guid BrandId { get; }

    /// <summary>
    /// Identifier of the product that was added.
    /// </summary>
    public Guid ProductId { get; }

    public BrandProductAddedEvent(Guid brandId, Guid productId)
    {
        BrandId = brandId;
        ProductId = productId;
    }
}
