using MediatR;
using Microsoft.Extensions.Logging;
using PulseERP.Application.Nofitication;
using PulseERP.Domain.Events.UserEvents;

namespace PulseERP.Application.Handlers;

public class UserNameChangedEventHandler(ILogger<UserNameChangedEventHandler> logger)
    : INotificationHandler<DomainEventNotification<UserNameChangedEvent>>
{
    public Task Handle(
        DomainEventNotification<UserNameChangedEvent> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;
        logger.LogInformation(
            "Handling UserNameChangedEvent for {UserId} at {Time}",
            evt.UserId, evt.OccurredOn);

        return Task.CompletedTask;
    }
}