using MediatR;
using Microsoft.Extensions.Logging;
using PulseERP.Abstractions.Security.Interfaces;
using PulseERP.Application.Nofitication;
using PulseERP.Domain.Events.UserEvents;

namespace PulseERP.Application.Handlers;

public class UserRestoredEventHandler(
    IEmailSenderService emailService,
    ILogger<UserRestoredEventHandler> logger)
    : INotificationHandler<DomainEventNotification<UserRestoredEvent>>
{
    public async Task Handle(
        DomainEventNotification<UserRestoredEvent> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;
        logger.LogInformation(
            "Handling UserRestoredEvent for {UserId} at {Time}",
            evt.UserId, evt.OccurredOn);

        await emailService.SendWelcomeEmailAsync(
            toEmail: evt.Email,
            userFullName: $"{evt.FirstName} {evt.LastName}",
            loginUrl: "https://app.pulseepr.com/login"
        );
    }
}