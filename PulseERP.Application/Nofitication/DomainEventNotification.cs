using MediatR;
using PulseERP.Domain.Interfaces;

namespace PulseERP.Application.Nofitication;

public class DomainEventNotification<TDomainEvent>(TDomainEvent domainEvent) : INotification
    where TDomainEvent : IDomainEvent
{
    public TDomainEvent DomainEvent { get; } = domainEvent;
}
