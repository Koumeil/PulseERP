using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.UserEvents;

/// <summary>
/// Event triggered when the user successfully logs in.
/// </summary>
public sealed class UserLoginSuccessfulEvent : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid UserId { get; }
    public DateTime LoginTime { get; }

    public UserLoginSuccessfulEvent(Guid userId, DateTime loginTime)
    {
        UserId = userId;
        LoginTime = loginTime;
    }
}
