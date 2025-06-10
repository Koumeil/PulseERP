using PulseERP.Domain.Entities;

namespace PulseERP.Domain.Events.DomainEvents;

using System;
using Interfaces;

/// <summary>
/// Domain event raised when an entity is activated (IsActive set to true).
/// </summary>
public sealed class EntityActivatedEvent : IDomainEvent
{
    /// <summary>
    /// UTC timestamp when the entity was activated.
    /// </summary>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    /// <summary>
    /// Reference to the entity that was activated.
    /// </summary>
    public BaseEntity Entity { get; }

    public EntityActivatedEvent(BaseEntity entity)
    {
        Entity = entity ?? throw new ArgumentNullException(nameof(entity));
    }
}
