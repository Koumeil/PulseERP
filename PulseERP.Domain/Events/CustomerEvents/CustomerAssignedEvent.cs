using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.CustomerEvents;

/// <summary>
/// Event raised when a customer is assigned to a user.
/// </summary>
public sealed class CustomerAssignedEvent : IDomainEvent
{
    public Guid CustomerId { get; }
    public Guid UserId { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public CustomerAssignedEvent(Guid customerId, Guid userId)
    {
        CustomerId = customerId;
        UserId = userId;
    }
}
