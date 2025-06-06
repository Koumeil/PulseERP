using PulseERP.Domain.Interfaces;
using PulseERP.Domain.ValueObjects;

namespace PulseERP.Domain.Events.UserEvents;

/// <summary>
/// Event triggered when the user’s role changes.
/// </summary>
public sealed class UserRoleChangedEvent : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid UserId { get; }
    public Role NewRole { get; }

    public UserRoleChangedEvent(Guid userId, Role newRole)
    {
        UserId = userId;
        NewRole = newRole;
    }
}
