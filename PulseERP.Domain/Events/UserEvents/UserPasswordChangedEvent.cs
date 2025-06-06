using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.UserEvents;

/// <summary>
/// Event triggered when the user’s password is changed.
/// </summary>
public sealed class UserPasswordChangedEvent : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid UserId { get; }

    public UserPasswordChangedEvent(Guid userId) => UserId = userId;
}
