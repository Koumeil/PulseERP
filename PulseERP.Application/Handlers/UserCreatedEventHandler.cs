using MediatR;
using Microsoft.Extensions.Logging;
using PulseERP.Abstractions.Security.Interfaces;
using PulseERP.Application.Nofitication;
using PulseERP.Domain.Events.UserEvents;

namespace PulseERP.Application.Handlers;

public class UserCreatedEventHandler(
    ITokenService tokenService,
    IEmailSenderService emailService,
    ILogger<UserCreatedEventHandler> logger)
    : INotificationHandler<DomainEventNotification<UserCreatedEvent>>
{
    public async Task Handle(DomainEventNotification<UserCreatedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        var activationToken = await tokenService.GenerateActivationTokenAsync(domainEvent.UserId);
        var activationUrl = $"https://localhost:4200/activate-account?token={activationToken}";

        await emailService.SendActivationEmailAsync(
            toEmail: domainEvent.Email,
            userFullName: $"{domainEvent.FirstName} {domainEvent.LastName}",
            activationUrl: activationUrl
        );

    }
}

