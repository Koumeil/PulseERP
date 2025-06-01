using Microsoft.EntityFrameworkCore;
using PulseERP.Abstractions.Security.Interfaces;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Enums.Token;
using PulseERP.Domain.Interfaces;
using PulseERP.Infrastructure.Database;

namespace PulseERP.Infrastructure.Repositories;

/// <summary>
/// Repository for password-reset tokens. Caching is not applied.
/// </summary>
public class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly CoreDbContext _ctx;
    private readonly IDateTimeProvider _time;

    /// <summary>
    /// Initializes a new instance of <see cref="PasswordResetTokenRepository"/>.
    /// </summary>
    /// <param name="ctx">EF Core DB context.</param>
    /// <param name="time">Date/time provider for current UTC.</param>
    public PasswordResetTokenRepository(CoreDbContext ctx, IDateTimeProvider time)
    {
        _ctx = ctx;
        _time = time;
    }

    /// <inheritdoc/>
    public async Task StoreAsync(Guid userId, string token, DateTime expiry)
    {
        var entity = RefreshToken.Create(_time, userId, token, TokenType.PasswordReset, expiry);
        await _ctx.RefreshTokens.AddAsync(entity);
    }

    /// <inheritdoc/>
    public Task<RefreshToken?> GetActiveByTokenAsync(string token) =>
        _ctx.RefreshTokens.SingleOrDefaultAsync(rt =>
            rt.Token == token && rt.Revoked == null && rt.Expires > _time.UtcNow
        );

    /// <inheritdoc/>
    public async Task MarkAsUsedAsync(string token)
    {
        RefreshToken? entity = await _ctx.RefreshTokens.SingleOrDefaultAsync(rt =>
            rt.Token == token && rt.TokenType == TokenType.PasswordReset
        );
        if (entity is not null)
        {
            entity.Revoke(_time.UtcNow);
            _ctx.RefreshTokens.Update(entity);
        }
    }
}
