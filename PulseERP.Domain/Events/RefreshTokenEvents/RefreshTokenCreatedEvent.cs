using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.RefreshTokenEvents;

/// <summary>
/// Domain event triggered when a new refresh token is created.
/// </summary>
public sealed class RefreshTokenCreatedEvent : IDomainEvent
{
    /// <summary>
    /// Gets the date and time when the event occurred.
    /// </summary>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the ID of the refresh token that was created.
    /// </summary>
    public Guid RefreshTokenId { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RefreshTokenCreatedEvent"/> class.
    /// </summary>
    /// <param name="refreshTokenId">The ID of the newly created refresh token.</param>
    public RefreshTokenCreatedEvent(Guid refreshTokenId)
    {
        RefreshTokenId = refreshTokenId;
    }
}
