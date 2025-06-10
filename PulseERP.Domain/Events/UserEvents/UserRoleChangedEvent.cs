using PulseERP.Domain.Interfaces;
using PulseERP.Domain.ValueObjects;

namespace PulseERP.Domain.Events.UserEvents;

public sealed class UserRoleChangedEvent(Guid userId, string newRole) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid UserId { get; } = userId;
    public string NewRole { get; } = newRole;
}
