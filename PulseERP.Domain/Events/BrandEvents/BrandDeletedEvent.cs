namespace PulseERP.Domain.Events.BrandEvents;

using System;
using PulseERP.Domain.Interfaces;

/// <summary>
/// Event raised when a brand is soft-deleted.
/// </summary>
public sealed class BrandDeletedEvent : IDomainEvent
{
    /// <summary>
    /// UTC timestamp when the brand was deleted.
    /// </summary>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    /// <summary>
    /// Identifier of the deleted brand.
    /// </summary>
    public Guid BrandId { get; }

    public BrandDeletedEvent(Guid brandId) => BrandId = brandId;
}
