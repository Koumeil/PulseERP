using PulseERP.Domain.Entities;

namespace PulseERP.Domain.Events.DomainEvents;

using System;
using Interfaces;

/// <summary>
/// Standard domain event triggered when an entity is restored from soft-deletion.
/// Can be used for notifications, audit logging, or resynchronization.
/// </summary>
public sealed class EntityRestoredEvent : IDomainEvent
{
    /// <summary>
    /// The UTC timestamp of when the event occurred.
    /// </summary>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    /// <summary>
    /// Reference to the entity that was restored.
    /// </summary>
    public BaseEntity Entity { get; }

    /// <summary>
    /// Creates a new <see cref="EntityRestoredEvent"/> for the specified entity.
    /// </summary>
    /// <param name="entity">The entity that has been restored.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="entity"/> is null.</exception>
    public EntityRestoredEvent(BaseEntity entity)
    {
        Entity = entity ?? throw new ArgumentNullException(nameof(entity));
    }
}
