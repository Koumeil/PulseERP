using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.UserEvents;

/// <summary>
/// Event triggered when the user’s name changes.
/// </summary>
public sealed class UserNameChangedEvent(Guid userId, string newFirstName, string newLastName) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid UserId { get; } = userId;
    public string NewFirstName { get; } = newFirstName;
    public string NewLastName { get; } = newLastName;
}
