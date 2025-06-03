using PulseERP.Domain.Entities;
using PulseERP.Domain.Enums.Token;

namespace PulseERP.Domain.Security.Interfaces;

/// <summary>
/// Central repository for authentication tokens (refresh & password reset).
/// </summary>
public interface ITokenRepository
{
    /// <summary>
    /// Adds a new token (refresh or reset).
    /// </summary>
    Task AddAsync(RefreshToken token);

    /// <summary>
    /// Returns the token entity by hash and type, or null if not found/expired/revoked.
    /// </summary>
    Task<RefreshToken?> GetByTokenAndTypeAsync(string tokenHash, TokenType tokenType);

    /// <summary>
    /// Returns the active token entity for a user and a specific type.
    /// </summary>
    Task<RefreshToken?> GetActiveByUserIdAndTypeAsync(Guid userId, TokenType tokenType);

    /// <summary>
    /// Revokes all active tokens for a user and type.
    /// </summary>
    Task RevokeAllByUserIdAndTypeAsync(Guid userId, TokenType tokenType);
}
