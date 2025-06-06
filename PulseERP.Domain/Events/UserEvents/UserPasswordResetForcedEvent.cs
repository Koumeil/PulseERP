using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.UserEvents;

/// <summary>
/// Event triggered when the user is forced to reset password.
/// </summary>
public sealed class UserPasswordResetForcedEvent : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid UserId { get; }

    public UserPasswordResetForcedEvent(Guid userId) => UserId = userId;
}
