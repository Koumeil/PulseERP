using Microsoft.EntityFrameworkCore;
using PulseERP.Abstractions.Security.Interfaces;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Enums.Token;
using PulseERP.Domain.Interfaces;
using PulseERP.Infrastructure.Database;

namespace PulseERP.Infrastructure.Repositories;

/// <summary>
/// Repository for <see cref="RefreshToken"/> entities. Caching is not applied.
/// </summary>
public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly CoreDbContext _ctx;
    private readonly IDateTimeProvider _time;

    /// <summary>
    /// Initializes a new instance of <see cref="RefreshTokenRepository"/>.
    /// </summary>
    /// <param name="ctx">EF Core DB context.</param>
    /// <param name="time">Date/time provider for current UTC.</param>
    public RefreshTokenRepository(CoreDbContext ctx, IDateTimeProvider time)
    {
        _ctx = ctx;
        _time = time;
    }

    /// <inheritdoc/>
    public Task<RefreshToken?> GetByTokenAsync(string tokenHash) =>
        _ctx.RefreshTokens.SingleOrDefaultAsync(rt => rt.Token == tokenHash);

    /// <inheritdoc/>
    public Task<RefreshToken?> GetActiveByUserIdAsync(Guid userId) =>
        _ctx.RefreshTokens.SingleOrDefaultAsync(rt =>
            rt.UserId == userId
            && rt.TokenType == TokenType.Refresh
            && rt.Revoked == null
            && rt.Expires > _time.UtcNow
        );

    /// <inheritdoc/>
    public async Task AddAsync(RefreshToken token)
    {
        await _ctx.RefreshTokens.AddAsync(token);
        await _ctx.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(RefreshToken token)
    {
        _ctx.RefreshTokens.Update(token);
        await _ctx.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task RevokeForUserAsync(Guid userId)
    {
        var tokens = _ctx.RefreshTokens.Where(rt =>
            rt.UserId == userId && rt.TokenType == TokenType.Refresh && rt.Revoked == null
        );
        await tokens.ExecuteUpdateAsync(s => s.SetProperty(rt => rt.Revoked, _time.UtcNow));
        await _ctx.SaveChangesAsync();
    }
}
