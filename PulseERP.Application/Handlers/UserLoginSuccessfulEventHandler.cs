using MediatR;
using Microsoft.Extensions.Logging;
using PulseERP.Application.Nofitication;
using PulseERP.Domain.Events.UserEvents;

namespace PulseERP.Application.Handlers;

public class UserLoginSuccessfulEventHandler(ILogger<UserLoginSuccessfulEventHandler> logger)
    : INotificationHandler<DomainEventNotification<UserLoginSuccessfulEvent>>
{
    public Task Handle(
        DomainEventNotification<UserLoginSuccessfulEvent> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;
        logger.LogInformation(
            "Handling UserLoginSuccessfulEvent for {UserId} at {Time}",
            evt.UserId, evt.LoginTime);

        return Task.CompletedTask;
    }
}