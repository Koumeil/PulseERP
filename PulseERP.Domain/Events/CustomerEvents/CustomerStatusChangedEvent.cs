using PulseERP.Domain.Enums.Customer;
using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.CustomerEvents;

/// <summary>
/// Event raised when customer status is changed.
/// </summary>
public sealed class CustomerStatusChangedEvent : IDomainEvent
{
    public Guid CustomerId { get; }
    public CustomerStatus NewStatus { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public CustomerStatusChangedEvent(Guid customerId, CustomerStatus newStatus)
    {
        CustomerId = customerId;
        NewStatus = newStatus;
    }
}
