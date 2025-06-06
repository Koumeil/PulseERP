namespace PulseERP.Domain.Common;

using System;
using System.Collections.Generic;
using PulseERP.Domain.Events;
using PulseERP.Domain.Events.DomainEvents;
using PulseERP.Domain.Interfaces;

/// <summary>
/// Base class for all aggregate roots and entities in the domain.
/// Provides:
///  - Unique identifier generation (Guid)
///  - Creation/Update timestamps (UTC)
///  - Soft delete capability
///  - Domain event tracking
///  - Equality comparison by identifier
/// </summary>
public abstract class BaseEntity : IEquatable<BaseEntity>
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Unique identifier of the entity.
    /// Assigned at creation and used for equality comparison.
    /// </summary>
    public Guid Id { get; protected set; } = Guid.NewGuid();

    /// <summary>
    /// UTC timestamp when the entity was created.
    /// </summary>
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    /// <summary>
    /// Indicates whether the entity is active.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// UTC timestamp when the entity was last updated.
    /// Null if never updated.
    /// </summary>
    public DateTime? UpdatedAt { get; protected set; }

    /// <summary>
    /// Indicates whether the entity has been soft-deleted.
    /// </summary>
    public bool IsDeleted { get; protected set; }

    /// <summary>
    /// UTC timestamp when the entity was soft-deleted.
    /// Null if not deleted.
    /// </summary>
    public DateTime? DeletedAt { get; protected set; }

    /// <summary>
    /// Read-only collection of domain events that have been added to this entity.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Marks the entity as updated by setting the UpdatedAt timestamp to current UTC.
    /// Should be called in business methods that modify state.
    /// </summary>
    public void MarkAsUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the entity as soft-deleted, setting IsDeleted to true and DeletedAt to current UTC.
    /// </summary>
    public virtual void MarkAsDeleted()
    {
        if (!IsDeleted)
        {
            IsDeleted = true;
            IsActive = false;
            DeletedAt = DateTime.UtcNow;
            AddDomainEvent(new EntitySoftDeletedEvent(this));
        }
    }

    /// <summary>
    /// Restores the entity by clearing soft-delete flags: sets IsDeleted to false and DeletedAt to null.
    /// </summary>
    public virtual void MarkAsRestored()
    {
        if (IsDeleted)
        {
            IsDeleted = false;
            IsActive = true;
            DeletedAt = null;
            AddDomainEvent(new EntityRestoredEvent(this));
        }
    }

    public virtual void MarkAsDeactivate()
    {
        if (IsActive)
        {
            IsActive = false;
            MarkAsUpdated();
            AddDomainEvent(new EntityDeactivatedEvent(this));
        }
    }

    public virtual void MarkAsActivate()
    {
        if (!IsActive && !IsDeleted)
        {
            IsActive = true;
            MarkAsUpdated();
            AddDomainEvent(new EntityActivatedEvent(this));
        }
    }

    /// <summary>
    /// Adds a domain event to the internal collection.
    /// Protected: called by derived entities to signal a domain event.
    /// </summary>
    /// <param name="domainEvent">Instance of an event implementing IDomainEvent.</param>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent is null)
            throw new ArgumentNullException(nameof(domainEvent));

        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all domain events (after dispatch/publish).
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Equality comparison based on <see cref="Id"/>.
    /// Two entities are equal if they share the same Id and the same CLR type.
    /// </summary>
    /// <param name="other">The other entity to compare.</param>
    /// <returns>True if same Id and same type.</returns>
    public bool Equals(BaseEntity? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (Id == Guid.Empty || other.Id == Guid.Empty)
            return false;

        return Id == other.Id && GetType() == other.GetType();
    }

    public override bool Equals(object? obj) => Equals(obj as BaseEntity);

    public override int GetHashCode() => (GetType().ToString(), Id).GetHashCode();

    public static bool operator ==(BaseEntity? left, BaseEntity? right)
    {
        if (left is null && right is null)
            return true;
        if (left is null || right is null)
            return false;
        return left.Equals(right);
    }

    public static bool operator !=(BaseEntity? left, BaseEntity? right) => !(left == right);
}
