using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.UserEvents;

/// <summary>
/// Event triggered when the user’s lockout is reset.
/// </summary>
public sealed class UserLockoutResetEvent : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid UserId { get; }

    public UserLockoutResetEvent(Guid userId) => UserId = userId;
}
