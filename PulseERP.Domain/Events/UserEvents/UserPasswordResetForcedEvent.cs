using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.UserEvents;

/// <summary>
/// Event triggered when the user is forced to reset password.
/// </summary>
public sealed class UserPasswordResetForcedEvent(Guid userId, string firstName, string lastName, string email, DateTime lockoutEnd) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid UserId { get; } = userId;
    public string FirstName { get; } = firstName;
    public string LastName { get; } = lastName;
    public string Email { get; } = email;
    public DateTime LockoutEnd { get; } = lockoutEnd;

}
