namespace PulseERP.Domain.Events.BrandEvents;

using System;
using Interfaces;

/// <summary>
/// Event raised when a brand is activated (IsActive set to true).
/// </summary>
public sealed class BrandActivatedEvent : IDomainEvent
{
    /// <summary>
    /// UTC timestamp when the brand was activated.
    /// </summary>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    /// <summary>
    /// Identifier of the activated brand.
    /// </summary>
    public Guid BrandId { get; }

    public BrandActivatedEvent(Guid brandId)
    {
        BrandId = brandId;
    }
}
