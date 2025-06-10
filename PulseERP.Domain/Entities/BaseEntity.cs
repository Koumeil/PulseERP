using PulseERP.Domain.Events.DomainEvents;
using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Entities;

public abstract class BaseEntity : IEquatable<BaseEntity>
{
    private readonly List<IDomainEvent> _domainEvents = [];
    public Guid Id { get; protected init; } = Guid.NewGuid();
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
    public bool IsActive { get; private set; } = false;
    public DateTime? UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; protected set; }
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void MarkAsUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    protected void SetIsActive(bool isActive)
    {
        if (IsActive == isActive)
            return;
        IsActive = isActive;
        AddDomainEvent(new EntityActivatedEvent(this));

    }

    public virtual void MarkAsDeleted()
    {
        if (IsDeleted) return;
        IsDeleted = true;
        IsActive = false;
        DeletedAt = DateTime.UtcNow;
        AddDomainEvent(new EntitySoftDeletedEvent(this));
    }

    public virtual void MarkAsRestored()
    {
        if (!IsDeleted) return;
        IsDeleted = false;
        IsActive = true;
        DeletedAt = null;
        AddDomainEvent(new EntityRestoredEvent(this));
    }

    public virtual void MarkAsDeactivate()
    {
        if (!IsActive) return;
        IsActive = false;
        MarkAsUpdated();
        AddDomainEvent(new EntityDeactivatedEvent(this));
    }

    public virtual void MarkAsActivate()
    {
        if (IsActive || IsDeleted) return;
        SetIsActive(true);
        MarkAsUpdated();
        AddDomainEvent(new EntityActivatedEvent(this));
    }
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

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
