using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.CustomerEvents;

/// <summary>
/// Event raised when customer details are updated.
/// </summary>
public sealed class CustomerDetailsUpdatedEvent : IDomainEvent
{
    public Guid CustomerId { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public CustomerDetailsUpdatedEvent(Guid customerId) => CustomerId = customerId;
}
