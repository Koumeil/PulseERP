using MediatR;
using Microsoft.Extensions.Logging;
using PulseERP.Application.Nofitication;
using PulseERP.Domain.Events.UserEvents;

namespace PulseERP.Application.Handlers;

public class UserPhoneChangedEventHandler(ILogger<UserPhoneChangedEventHandler> logger)
    : INotificationHandler<DomainEventNotification<UserPhoneChangedEvent>>
{
    public Task Handle(
        DomainEventNotification<UserPhoneChangedEvent> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;
        logger.LogInformation(
            "Handling UserPhoneChangedEvent for {UserId} at {Time}",
            evt.UserId, evt.OccurredOn);

        return Task.CompletedTask;
    }
}