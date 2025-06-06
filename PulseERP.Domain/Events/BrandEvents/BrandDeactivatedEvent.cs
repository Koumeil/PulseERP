namespace PulseERP.Domain.Events.BrandEvents;

using System;
using PulseERP.Domain.Interfaces;

/// <summary>
/// Event raised when a brand is deactivated (IsActive set to false).
/// </summary>
public sealed class BrandDeactivatedEvent : IDomainEvent
{
    /// <summary>
    /// UTC timestamp when the brand was deactivated.
    /// </summary>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    /// <summary>
    /// Identifier of the deactivated brand.
    /// </summary>
    public Guid BrandId { get; }

    public BrandDeactivatedEvent(Guid brandId)
    {
        BrandId = brandId;
    }
}
