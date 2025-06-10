namespace PulseERP.Domain.Events.BrandEvents;

using System;
using Interfaces;

/// <summary>
/// Event raised when a brand is restored from soft-delete.
/// </summary>
public sealed class BrandRestoredEvent : IDomainEvent
{
    /// <summary>
    /// UTC timestamp when the brand was restored.
    /// </summary>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    /// <summary>
    /// Identifier of the restored brand.
    /// </summary>
    public Guid BrandId { get; }

    public BrandRestoredEvent(Guid brandId) => BrandId = brandId;
}
