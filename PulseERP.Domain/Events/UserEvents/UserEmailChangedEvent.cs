using PulseERP.Domain.Interfaces;
using PulseERP.Domain.VO;

namespace PulseERP.Domain.Events.UserEvents;

/// <summary>
/// Event triggered when the user’s email changes.
/// </summary>
public sealed class UserEmailChangedEvent : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid UserId { get; }
    public EmailAddress NewEmail { get; }

    public UserEmailChangedEvent(Guid userId, EmailAddress newEmail)
    {
        UserId = userId;
        NewEmail = newEmail ?? throw new ArgumentNullException(nameof(newEmail));
    }
}
