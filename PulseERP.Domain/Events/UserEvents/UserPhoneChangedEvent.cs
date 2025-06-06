using PulseERP.Domain.Interfaces;
using PulseERP.Domain.VO;

namespace PulseERP.Domain.Events.UserEvents;

/// <summary>
/// Event triggered when the user’s phone changes.
/// </summary>
public sealed class UserPhoneChangedEvent : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid UserId { get; }
    public Phone NewPhone { get; }

    public UserPhoneChangedEvent(Guid userId, Phone newPhone)
    {
        UserId = userId;
        NewPhone = newPhone ?? throw new ArgumentNullException(nameof(newPhone));
    }
}
