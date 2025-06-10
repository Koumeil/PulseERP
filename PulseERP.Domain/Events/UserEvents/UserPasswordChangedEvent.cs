using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.UserEvents;

/// <summary>
/// Event triggered when the user’s password is changed.
/// </summary>
public sealed class UserPasswordChangedEvent(Guid userId, string firstName, string lastName, string email)
    : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid UserId { get; } = userId;
    public string FirstName { get; } = firstName;
    public string LastName { get; } = lastName;
    public string Email { get; } = email;
}