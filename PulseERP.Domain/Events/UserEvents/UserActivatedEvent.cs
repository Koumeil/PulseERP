using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.UserEvents;

/// <summary>
/// Event triggered when the user is activated.
/// </summary>
public sealed class UserActivatedEvent : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid UserId { get; }

    public UserActivatedEvent(Guid userId) => UserId = userId;
}
