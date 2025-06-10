using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.UserEvents;

/// <summary>
/// Event triggered when the user is activated.
/// </summary>
public sealed class UserActivatedEvent(Guid userId, string firstname, string lastName, string email) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid UserId { get; } = userId;
    public string FirstName { get; } = firstname;
    public string LastName { get; } = lastName;
    public string Email { get; } = email;
}
