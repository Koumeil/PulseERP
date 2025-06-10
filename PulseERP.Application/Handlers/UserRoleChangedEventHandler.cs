using MediatR;
using Microsoft.Extensions.Logging;
using PulseERP.Abstractions.Security.Interfaces;
using PulseERP.Application.Nofitication;
using PulseERP.Domain.Events.UserEvents;

namespace PulseERP.Application.Handlers;

public class UserRoleChangedEventHandler(
    IEmailSenderService emailService,
    ILogger<UserRoleChangedEventHandler> logger)
    : INotificationHandler<DomainEventNotification<UserRoleChangedEvent>>
{
    public Task Handle(
        DomainEventNotification<UserRoleChangedEvent> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;
        logger.LogInformation(
            "Handling UserRoleChangedEvent for {UserId} at {Time}",
            evt.UserId, evt.OccurredOn);

        return Task.CompletedTask;
    }
}