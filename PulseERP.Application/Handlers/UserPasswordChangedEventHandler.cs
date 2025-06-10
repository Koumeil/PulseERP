using MediatR;
using Microsoft.Extensions.Logging;
using PulseERP.Abstractions.Security.Interfaces;
using PulseERP.Application.Nofitication;
using PulseERP.Domain.Events.UserEvents;

namespace PulseERP.Application.Handlers;

public class UserPasswordChangedEventHandler(
    IEmailSenderService emailService,
    ILogger<UserPasswordChangedEventHandler> logger)
    : INotificationHandler<DomainEventNotification<UserPasswordChangedEvent>>
{
    public async Task Handle(
        DomainEventNotification<UserPasswordChangedEvent> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;
        logger.LogInformation(
            "Handling UserPasswordChangedEvent for {UserId} at {Time}",
            evt.UserId, evt.OccurredOn);

        await emailService.SendPasswordChangedEmailAsync(
            toEmail: evt.Email,
            userFullName: $"{evt.FirstName} {evt.LastName}"
        );
    }
}