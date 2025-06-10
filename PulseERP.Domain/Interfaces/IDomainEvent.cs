namespace PulseERP.Domain.Interfaces;

using System;


public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
