using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.UserEvents;

/// <summary>
/// Event triggered when the user is locked out due to failed login attempts.
/// </summary>
public sealed class UserLockedOutEvent : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid UserId { get; }
    public DateTime? LockoutEnd { get; }

    public UserLockedOutEvent(Guid userId, DateTime? lockoutEnd)
    {
        UserId = userId;
        LockoutEnd = lockoutEnd;
    }
}
