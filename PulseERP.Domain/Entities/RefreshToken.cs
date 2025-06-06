namespace PulseERP.Domain.Entities;

using PulseERP.Domain.Common;
using PulseERP.Domain.Enums.Token;
using PulseERP.Domain.Events.ProductEvents;
using PulseERP.Domain.Events.RefreshTokenEvents;
using PulseERP.Domain.Interfaces;

/// <summary>
/// Represents a refresh token entity for authentication. Acts as an aggregate root.
/// </summary>
public sealed class RefreshToken : BaseEntity
{
    #region Properties

    /// <summary>
    /// The token string value.
    /// </summary>
    public string Token { get; private set; } = default!;

    /// <summary>
    /// Identifier of the associated user.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Expiry timestamp.
    /// </summary>
    public DateTime Expires { get; private set; }

    /// <summary>
    /// The type of token.
    /// </summary>
    public TokenType TokenType { get; private set; }

    /// <summary>
    /// Timestamp when the token was revoked, if any.
    /// </summary>
    public DateTime? Revoked { get; private set; }

    /// <summary>
    /// IP address where the token was created.
    /// </summary>
    public string? CreatedByIp { get; private set; }

    /// <summary>
    /// User agent string where the token was created.
    /// </summary>
    public string? CreatedByUserAgent { get; private set; }

    /// <summary>
    /// Indicates if the token is currently active (not revoked and not expired).
    /// </summary>
    public bool IsCurrentlyValid => !IsExpired() && !IsRevoked();

    #endregion

    #region Fields

    private readonly IDateTimeProvider _dateTimeProvider = default!;

    #endregion

    #region Constructors

    /// <summary>
    /// Protected constructor for EF Core.
    /// </summary>
    private RefreshToken() { }

    #endregion

    #region Factory

    /// <summary>
    /// Creates a new refresh token with provided IP and user agent. Expires after <paramref name="expiresAt"/>.
    /// </summary>
    public static RefreshToken Create(
        IDateTimeProvider dateTimeProvider,
        Guid userId,
        string token,
        TokenType tokenType,
        DateTime expiresAt,
        string? createdByIp = null,
        string? createdByUserAgent = null
    )
    {
        if (dateTimeProvider is null)
            throw new ArgumentNullException(nameof(dateTimeProvider));
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required.", nameof(userId));
        if (expiresAt <= dateTimeProvider.UtcNow)
            throw new ArgumentException("Expiration must be in the future.", nameof(expiresAt));

        var tokenInstance = new RefreshToken(
            dateTimeProvider,
            userId,
            token,
            tokenType,
            expiresAt,
            createdByIp,
            createdByUserAgent
        );

        tokenInstance.AddDomainEvent(new RefreshTokenCreatedEvent(tokenInstance.Id));

        return tokenInstance;
    }

    #endregion

    #region Private Constructors

    private RefreshToken(
        IDateTimeProvider dateTimeProvider,
        Guid userId,
        string token,
        TokenType tokenType,
        DateTime expiresAt,
        string? createdByIp,
        string? createdByUserAgent
    )
    {
        _dateTimeProvider = dateTimeProvider;
        Id = Guid.NewGuid();
        UserId = userId;
        Token = token;
        TokenType = tokenType;
        Expires = expiresAt;
        CreatedByIp = createdByIp;
        CreatedByUserAgent = createdByUserAgent;
    }

    #endregion

    #region Methods

    public void Revoke(DateTime revokedAt)
    {
        if (IsRevoked())
            return;

        Revoked = revokedAt;
        AddDomainEvent(new RefreshTokenRevokedEvent(Id, revokedAt));
        MarkAsUpdated();
    }

    public bool IsRevoked() => Revoked is not null;

    public bool IsExpired() => Expires <= (_dateTimeProvider?.UtcNow ?? DateTime.UtcNow);

    #endregion
}
