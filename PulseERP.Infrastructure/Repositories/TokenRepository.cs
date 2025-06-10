using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PulseERP.Abstractions.Security.Interfaces;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Enums.Token;
using PulseERP.Domain.Interfaces;
using PulseERP.Infrastructure.Database;

namespace PulseERP.Infrastructure.Repositories;

public class TokenRepository(
    CoreDbContext context,
    IDateTimeProvider time,
    ILogger<TokenRepository> logger
)
    : ITokenRepository
{
    public async Task AddAsync(TokenEntity tokenEntity)
    {
        await RevokeAllByUserIdAndTypeAsync(tokenEntity.UserId,
            tokenEntity.TokenType);
        await context.RefreshTokens.AddAsync(tokenEntity);
        await context.SaveChangesAsync();
    }

    public async Task<TokenEntity?> GetByTokenAndTypeAsync(string tokenHash, TokenType tokenType)
    {
        var nowUtc = time.UtcNow;
        var token = await context.RefreshTokens.FirstOrDefaultAsync(x =>
            x.Token == tokenHash
            && x.TokenType == tokenType
            && x.Revoked == null
            && x.Expires > nowUtc
        );
        return token;
    }

    public async Task<TokenEntity?> GetActiveByUserIdAndTypeAsync(Guid userId, TokenType tokenType)
    {
        var nowUtc = time.UtcNow;
        var token = await context
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
        var tokens = await context
            .RefreshTokens.Where(x =>
                x.UserId == userId
                && x.TokenType == tokenType
                && x.Revoked == null
                && x.Expires > nowUtc
            )
            .ToListAsync();

        if (tokens.Count != 0)
        {
            foreach (var token in tokens)
                token.Revoke(nowUtc);

            await context.SaveChangesAsync();
        }
    }
}