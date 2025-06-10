namespace PulseERP.Domain.Events.BrandEvents;

using System;
using Interfaces;

/// <summary>
/// Event raised when a new brand is created.
/// </summary>
public sealed class BrandCreatedEvent : IDomainEvent
{
    /// <summary>
    /// UTC timestamp when the brand was created.
    /// </summary>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    /// <summary>
    /// Identifier of the newly created brand.
    /// </summary>
    public Guid BrandId { get; }

    /// <summary>
    /// Initial name of the brand.
    /// </summary>
    public string Name { get; }

    public BrandCreatedEvent(Guid brandId, string name)
    {
        BrandId = brandId;
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }
}
