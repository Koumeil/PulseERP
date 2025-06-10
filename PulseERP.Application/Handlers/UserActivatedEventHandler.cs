using MediatR;
using Microsoft.Extensions.Logging;
using PulseERP.Abstractions.Security.Interfaces;
using PulseERP.Application.Nofitication;
using PulseERP.Domain.Events.UserEvents;

namespace PulseERP.Application.Handlers;

public class UserActivatedEventHandler(
    IEmailSenderService emailService,
    ILogger<UserActivatedEventHandler> logger)
    : INotificationHandler<DomainEventNotification<UserActivatedEvent>>
{
    public async Task Handle(
        DomainEventNotification<UserActivatedEvent> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;
        logger.LogInformation(
            "Handling UserActivatedEvent for {UserId} at {Time}",
            evt.UserId, evt.OccurredOn);

        await emailService.SendWelcomeEmailAsync(
            toEmail: evt.Email,
            userFullName: $"{evt.FirstName} {evt.LastName}",
            loginUrl: "https://app.pulseepr.com/login"
        );
    }
}