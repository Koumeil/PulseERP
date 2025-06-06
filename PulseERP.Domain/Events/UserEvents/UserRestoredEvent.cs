using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.UserEvents;

/// <summary>
/// Event triggered when the user is restored from soft-delete.
/// </summary>
public sealed class UserRestoredEvent : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid UserId { get; }

    public UserRestoredEvent(Guid userId) => UserId = userId;
}
