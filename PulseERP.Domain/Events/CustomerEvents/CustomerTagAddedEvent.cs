using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.CustomerEvents;

/// <summary>
/// Event raised when a tag is added to the customer.
/// </summary>
public sealed class CustomerTagAddedEvent : IDomainEvent
{
    public Guid CustomerId { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public CustomerTagAddedEvent(Guid customerId) => CustomerId = customerId;
}
