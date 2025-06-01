using PulseERP.Abstractions.Security.Interfaces;
using PulseERP.Domain.Enums.Token;

namespace PulseERP.Domain.Entities
{
    /// <summary>
    /// Represents a refresh token entity for authentication. Acts as an aggregate root.
    /// </summary>
    public sealed class RefreshToken
    {
        #region Properties

        /// <summary>
        /// Unique identifier for the refresh token.
        /// </summary>
        public Guid Id { get; private set; }

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
        public bool IsActive =>
            Revoked == null && Expires > (_dateTimeProvider?.UtcNow ?? DateTime.UtcNow);

        #endregion

        #region Fields

        private readonly IDateTimeProvider _dateTimeProvider = default!;

        #endregion

        #region Constructors

        /// <summary>
        /// Protected constructor for EF Core.
        /// </summary>
        protected RefreshToken() { }

        #endregion

        #region Factory

        /// <summary>
        /// Creates a new refresh token with provided IP and user agent. Expires after <paramref name="expiresAt"/>.
        /// </summary>
        /// <param name="dateTimeProvider">Provider for current UTC time.</param>
        /// <param name="userId">Associated user identifier.</param>
        /// <param name="tokenType">Type of token.</param>
        /// <param name="expiresAt">Expiry timestamp.</param>
        /// <param name="createdByIp">IP address where created.</param>
        /// <param name="createdByUserAgent">User agent string.</param>
        /// <returns>New <see cref="RefreshToken"/> instance.</returns>
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

            return new RefreshToken(
                dateTimeProvider,
                userId,
                token,
                tokenType,
                expiresAt,
                createdByIp,
                createdByUserAgent
            );
        }

        #endregion

        #region Private Constructors

        /// <summary>
        /// Private constructor used by <see cref="Create"/>.
        /// </summary>
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

        /// <summary>
        /// Revokes the refresh token at current UTC time.
        /// </summary>
        public void Revoke(DateTime revokedAt)
        {
            if (Revoked == null)
            {
                Revoked = revokedAt;
            }
        }

        #endregion
    }
}
