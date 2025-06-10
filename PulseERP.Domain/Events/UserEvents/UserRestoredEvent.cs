using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.UserEvents;

/// <summary>
/// Event triggered when the user is restored from soft-delete.
/// </summary>
public sealed class UserRestoredEvent(Guid userId, string firstName, string lastName, string email) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid UserId { get; } = userId;
    public string FirstName { get; } = firstName;
    public string LastName { get; } = lastName;
    public string Email { get; } = email;
}
