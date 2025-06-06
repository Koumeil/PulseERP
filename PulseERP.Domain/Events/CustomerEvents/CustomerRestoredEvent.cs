using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.CustomerEvents;

public sealed class CustomerRestoredEvent : IDomainEvent
{
    public Guid CustomerId { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public CustomerRestoredEvent(Guid customerId) => CustomerId = customerId;
}
