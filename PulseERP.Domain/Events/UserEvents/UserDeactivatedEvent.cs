using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.UserEvents;

/// <summary>
/// Event triggered when the user is deactivated.
/// </summary>
public sealed class UserDeactivatedEvent : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid UserId { get; }

    public UserDeactivatedEvent(Guid userId) => UserId = userId;
}
