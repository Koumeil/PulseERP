using Microsoft.EntityFrameworkCore;
using PulseERP.Domain.Enums.Token;
using PulseERP.Domain.Interfaces.Repositories;
using PulseERP.Domain.Interfaces.Services;
using PulseERP.Infrastructure.Database;

namespace PulseERP.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly CoreDbContext _ctx;
    private readonly IDateTimeProvider _time;

    public RefreshTokenRepository(CoreDbContext ctx, IDateTimeProvider time)
    {
        _ctx = ctx;
        _time = time;
    }

    public Task<RefreshToken?> GetByTokenAsync(string tokenHash) =>
        _ctx.RefreshTokens.SingleOrDefaultAsync(rt => rt.Token == tokenHash);

    public Task<RefreshToken?> GetActiveByUserIdAsync(Guid userId) =>
        _ctx.RefreshTokens.SingleOrDefaultAsync(rt =>
            rt.UserId == userId
            && rt.TokenType == TokenType.Refresh
            && rt.Revoked == null
            && rt.Expires > _time.UtcNow
        );

    public async Task AddAsync(RefreshToken token)
    {
        await _ctx.RefreshTokens.AddAsync(token);
        await _ctx.SaveChangesAsync();
    }

    public async Task UpdateAsync(RefreshToken token)
    {
        _ctx.RefreshTokens.Update(token);
        await _ctx.SaveChangesAsync();
    }

    public async Task RevokeForUserAsync(Guid userId)
    {
        var tokens = _ctx.RefreshTokens.Where(rt =>
            rt.UserId == userId && rt.TokenType == TokenType.Refresh && rt.Revoked == null
        );
        await tokens.ExecuteUpdateAsync(s => s.SetProperty(rt => rt.Revoked, _time.UtcNow));
        await _ctx.SaveChangesAsync();
    }
}
