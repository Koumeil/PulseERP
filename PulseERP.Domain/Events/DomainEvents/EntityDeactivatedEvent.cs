namespace PulseERP.Domain.Events.DomainEvents;

using System;
using PulseERP.Domain.Common;
using PulseERP.Domain.Interfaces;

/// <summary>
/// Domain event raised when an entity is deactivated (IsActive set to false).
/// </summary>
public sealed class EntityDeactivatedEvent : IDomainEvent
{
    /// <summary>
    /// UTC timestamp when the entity was deactivated.
    /// </summary>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    /// <summary>
    /// Reference to the entity that was deactivated.
    /// </summary>
    public BaseEntity Entity { get; }

    public EntityDeactivatedEvent(BaseEntity entity)
    {
        Entity = entity ?? throw new ArgumentNullException(nameof(entity));
    }
}
