using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.UserEvents;

/// <summary>
/// Event triggered when the user’s name changes.
/// </summary>
public sealed class UserNameChangedEvent : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid UserId { get; }
    public string NewFirstName { get; }
    public string NewLastName { get; }

    public UserNameChangedEvent(Guid userId, string newFirstName, string newLastName)
    {
        UserId = userId;
        NewFirstName = newFirstName;
        NewLastName = newLastName;
    }
}
