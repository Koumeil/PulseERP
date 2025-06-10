using System;
using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.ProductEvents;

/// <summary>
/// Raised when a new product is created in the system.
/// </summary>
public sealed class ProductCreatedEvent(Guid productId) : IDomainEvent
{
    public Guid ProductId { get; } = productId;
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
