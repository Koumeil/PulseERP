using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.CustomerEvents;

/// <summary>
/// Event raised when a note is added to the customer.
/// </summary>
public sealed class CustomerNoteAddedEvent : IDomainEvent
{
    public Guid CustomerId { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public CustomerNoteAddedEvent(Guid customerId) => CustomerId = customerId;
}
