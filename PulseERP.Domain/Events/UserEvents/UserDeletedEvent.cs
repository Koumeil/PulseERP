using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.UserEvents;

/// <summary>
/// Event triggered when the user is soft-deleted.
/// </summary>
public sealed class UserDeletedEvent : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid UserId { get; }

    public UserDeletedEvent(Guid userId) => UserId = userId;
}
