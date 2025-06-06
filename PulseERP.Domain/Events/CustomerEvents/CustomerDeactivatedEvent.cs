using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.CustomerEvents;

/// <summary>
/// Event raised when a customer is deactivated.
/// </summary>
public sealed class CustomerDeactivatedEvent : IDomainEvent
{
    public Guid CustomerId { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public CustomerDeactivatedEvent(Guid customerId) => CustomerId = customerId;
}
