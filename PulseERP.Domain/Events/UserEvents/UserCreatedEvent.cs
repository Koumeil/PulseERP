using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.UserEvents;

public sealed class UserCreatedEvent(
    Guid userId,
    string firstName,
    string lastName,
    string email) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid UserId { get; } = userId;
    public string Email { get; } = email;
    public string FirstName { get; } = firstName;
    public string LastName { get; } = lastName;
}