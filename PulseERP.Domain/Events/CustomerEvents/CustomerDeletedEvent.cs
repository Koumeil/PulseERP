using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.CustomerEvents;

public sealed class CustomerDeletedEvent : IDomainEvent
{
    public Guid CustomerId { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public CustomerDeletedEvent(Guid customerId) => CustomerId = customerId;
}
