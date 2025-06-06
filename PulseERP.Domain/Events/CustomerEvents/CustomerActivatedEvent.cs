using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.CustomerEvents;

/// <summary>
/// Event raised when a customer is activated.
/// </summary>
public sealed class CustomerActivatedEvent : IDomainEvent
{
    public Guid CustomerId { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public CustomerActivatedEvent(Guid customerId) => CustomerId = customerId;
}
