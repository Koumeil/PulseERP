using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.CustomerEvents;

/// <summary>
/// Event raised when a new interaction is recorded for the customer.
/// </summary>
public sealed class CustomerInteractionRecordedEvent : IDomainEvent
{
    public Guid CustomerId { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public CustomerInteractionRecordedEvent(Guid customerId) => CustomerId = customerId;
}
