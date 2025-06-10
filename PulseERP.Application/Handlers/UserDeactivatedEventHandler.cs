using MediatR;
using Microsoft.Extensions.Logging;
using PulseERP.Abstractions.Security.Interfaces;
using PulseERP.Application.Nofitication;
using PulseERP.Domain.Events.UserEvents;

namespace PulseERP.Application.Handlers;

public class UserDeactivatedEventHandler(
    IEmailSenderService emailService,
    ILogger<UserDeactivatedEventHandler> logger)
    : INotificationHandler<DomainEventNotification<UserDeactivatedEvent>>
{
    public async Task Handle(
        DomainEventNotification<UserDeactivatedEvent> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;
        logger.LogInformation(
            "Handling UserDeactivatedEvent for {UserId} at {Time}",
            evt.UserId, evt.OccurredOn);

        await emailService.SendAccountLockedEmailAsync(
            toEmail: evt.Email,
            userFullName: $"{evt.FirstName} {evt.LastName}",
            lockoutEndUtc: DateTime.UtcNow.AddYears(1)
        );
    }
}