using Microsoft.EntityFrameworkCore;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Enums.Token;
using PulseERP.Domain.Interfaces.Repositories;
using PulseERP.Domain.Interfaces.Services;
using PulseERP.Infrastructure.Database;

namespace PulseERP.Infrastructure.Repositories;

public class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly CoreDbContext _ctx;
    private readonly IDateTimeProvider _time;

    public PasswordResetTokenRepository(CoreDbContext ctx, IDateTimeProvider time)
    {
        _ctx = ctx;
        _time = time;
    }

    public async Task StoreAsync(Guid userId, string token, DateTime expiry)
    {
        var entity = new RefreshToken(_time)
        {
            Token = token,
            UserId = userId,
            Expires = expiry,
            TokenType = TokenType.PasswordReset,
        };
        await _ctx.RefreshTokens.AddAsync(entity);
    }

    public Task<RefreshToken?> GetActiveByTokenAsync(string token) =>
        _ctx.RefreshTokens.SingleOrDefaultAsync(rt =>
            rt.Token == token
            // && rt.TokenType == TokenType.PasswordReset
            && rt.Revoked == null
            && rt.Expires > _time.UtcNow
        );

    public async Task MarkAsUsedAsync(string token)
    {
        var entity = await _ctx.RefreshTokens.SingleOrDefaultAsync(rt =>
            rt.Token == token && rt.TokenType == TokenType.PasswordReset
        );
        if (entity != null)
        {
            entity.Revoked = _time.UtcNow;
            _ctx.RefreshTokens.Update(entity);
        }
    }
}
