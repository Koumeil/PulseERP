using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PulseERP.Abstractions.Security.Interfaces;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Enums.Token;
using PulseERP.Domain.Interfaces;
using PulseERP.Infrastructure.Database;

namespace PulseERP.Infrastructure.Repositories;

public class TokenRepository(
    CoreDbContext ctx,
    IDateTimeProvider time,
    ILogger<TokenRepository> logger)
    : ITokenRepository
{
    public async Task AddAsync(RefreshToken token)
    {
        await RevokeAllByUserIdAndTypeAsync(token.UserId, token.TokenType); // Security: always one active token per type/user
        await ctx.RefreshTokens.AddAsync(token);
        await ctx.SaveChangesAsync();
        logger.LogInformation(
            "{TokenType} token added for user {UserId}, expires {ExpiresLocal} at {NowLocal}.",
            token.TokenType,
            token.UserId,
            time.ConvertToLocal(token.Expires),
            time.NowLocal
        );
    }

    public async Task<RefreshToken?> GetByTokenAndTypeAsync(string tokenHash, TokenType tokenType)
    {
        var nowUtc = time.UtcNow;
        var token = await ctx.RefreshTokens.FirstOrDefaultAsync(x =>
            x.Token == tokenHash
            && x.TokenType == tokenType
            && x.Revoked == null
            && x.Expires > nowUtc
        );

        logger.LogDebug(
            "Searched for {TokenType} token hash, found={Found}, at {NowLocal}.",
            tokenType,
            token != null,
            time.NowLocal
        );
        return token;
    }

    public async Task<RefreshToken?> GetActiveByUserIdAndTypeAsync(Guid userId, TokenType tokenType)
    {
        var nowUtc = time.UtcNow;
        var token = await ctx
            .RefreshTokens.OrderByDescending(x => x.Expires)
            .FirstOrDefaultAsync(x =>
                x.UserId == userId
                && x.TokenType == tokenType
                && x.Revoked == null
                && x.Expires > nowUtc
            );
        return token;
    }

    public async Task RevokeAllByUserIdAndTypeAsync(Guid userId, TokenType tokenType)
    {
        var nowUtc = time.UtcNow;
        var tokens = await ctx
            .RefreshTokens.Where(x =>
                x.UserId == userId
                && x.TokenType == tokenType
                && x.Revoked == null
                && x.Expires > nowUtc
            )
            .ToListAsync();

        if (tokens.Any())
        {
            foreach (var token in tokens)
                token.Revoke(nowUtc);

            await ctx.SaveChangesAsync();
            logger.LogInformation(
                "Revoked all {TokenType} tokens for user {UserId} at {NowLocal}.",
                tokenType,
                userId,
                time.NowLocal
            );
        }
    }
}
