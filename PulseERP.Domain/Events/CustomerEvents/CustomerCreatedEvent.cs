using System;
using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.CustomerEvents;

/// <summary>
/// Event raised when a new customer is created.
/// </summary>
public sealed class CustomerCreatedEvent : IDomainEvent
{
    public Guid CustomerId { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public CustomerCreatedEvent(Guid customerId) => CustomerId = customerId;
}
