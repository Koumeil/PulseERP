namespace PulseERP.Domain.Events.BrandEvents;

using System;
using PulseERP.Domain.Interfaces;

/// <summary>
/// Event raised when a brand’s name is updated.
/// </summary>
public sealed class BrandNameUpdatedEvent : IDomainEvent
{
    /// <summary>
    /// UTC timestamp when the name was updated.
    /// </summary>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    /// <summary>
    /// Identifier of the brand whose name was updated.
    /// </summary>
    public Guid BrandId { get; }

    /// <summary>
    /// The new name of the brand.
    /// </summary>
    public string NewName { get; }

    public BrandNameUpdatedEvent(Guid brandId, string newName)
    {
        BrandId = brandId;
        NewName = newName ?? throw new ArgumentNullException(nameof(newName));
    }
}
