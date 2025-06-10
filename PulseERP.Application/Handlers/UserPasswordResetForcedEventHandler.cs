using MediatR;
using Microsoft.Extensions.Logging;
using PulseERP.Abstractions.Security.Interfaces;
using PulseERP.Application.Nofitication;
using PulseERP.Domain.Events.UserEvents;

namespace PulseERP.Application.Handlers;

public class UserPasswordResetForcedEventHandler(
    IEmailSenderService emailService,
    ITokenService tokenService,
    ILogger<UserPasswordResetForcedEventHandler> logger)
    : INotificationHandler<DomainEventNotification<UserPasswordResetForcedEvent>>
{
    public async Task Handle(
        DomainEventNotification<UserPasswordResetForcedEvent> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;

        var resetToken = await tokenService.GenerateActivationTokenAsync(evt.UserId);


        logger.LogInformation(
            "Handling UserPasswordResetForcedEvent for {UserId} at {Time}",
            evt.UserId, evt.OccurredOn);

        var resetUrl = $"https://localhost:4200/password-reset?token={resetToken}";

        await emailService.SendPasswordResetEmailAsync(
            toEmail: evt.Email,
            userFullName: $"{evt.FirstName} {evt.LastName}",
            resetUrl: resetUrl,
            expiresAtUtc: DateTime.UtcNow.AddHours(1)
        );
    }
}