using PulseERP.Domain.Interfaces;

namespace PulseERP.Domain.Events.RefreshTokenEvents;

/// <summary>
/// Domain event triggered when a refresh token is revoked.
/// </summary>
public sealed class RefreshTokenRevokedEvent : IDomainEvent
{
    /// <summary>
    /// Gets the date and time when the event occurred.
    /// </summary>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the ID of the refresh token that was revoked.
    /// </summary>
    public Guid RefreshTokenId { get; }

    /// <summary>
    /// Gets the date and time when the token was revoked.
    /// </summary>
    public DateTime RevokedAt { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RefreshTokenRevokedEvent"/> class.
    /// </summary>
    /// <param name="refreshTokenId">The ID of the revoked refresh token.</param>
    /// <param name="revokedAt">The date and time when the token was revoked.</param>
    public RefreshTokenRevokedEvent(Guid refreshTokenId, DateTime revokedAt)
    {
        RefreshTokenId = refreshTokenId;
        RevokedAt = revokedAt;
    }
}
