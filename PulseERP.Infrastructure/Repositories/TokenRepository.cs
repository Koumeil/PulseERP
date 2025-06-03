using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PulseERP.Abstractions.Security.Interfaces;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Enums.Token;
using PulseERP.Domain.Security.Interfaces;
using PulseERP.Infrastructure.Database;

namespace PulseERP.Infrastructure.Repositories;

/// <summary>
/// Unified repository for all authentication tokens, using TokenType to distinguish.
/// </summary>
public class TokenRepository : ITokenRepository
{
    private readonly CoreDbContext _ctx;
    private readonly IDateTimeProvider _time;
    private readonly ILogger<TokenRepository> _logger;

    public TokenRepository(
        CoreDbContext ctx,
        IDateTimeProvider time,
        ILogger<TokenRepository> logger
    )
    {
        _ctx = ctx;
        _time = time;
        _logger = logger;
    }

    public async Task AddAsync(RefreshToken token)
    {
        await RevokeAllByUserIdAndTypeAsync(token.UserId, token.TokenType); // Security: always one active token per type/user
        await _ctx.RefreshTokens.AddAsync(token);
        await _ctx.SaveChangesAsync();
        _logger.LogInformation(
            "{TokenType} token added for user {UserId}, expires {ExpiresLocal} at {NowLocal}.",
            token.TokenType,
            token.UserId,
            _time.ConvertToLocal(token.Expires),
            _time.NowLocal
        );
    }

    public async Task<RefreshToken?> GetByTokenAndTypeAsync(string tokenHash, TokenType tokenType)
    {
        var nowUtc = _time.UtcNow;
        var token = await _ctx.RefreshTokens.FirstOrDefaultAsync(x =>
            x.Token == tokenHash
            && x.TokenType == tokenType
            && x.Revoked == null
            && x.Expires > nowUtc
        );

        _logger.LogDebug(
            "Searched for {TokenType} token hash, found={Found}, at {NowLocal}.",
            tokenType,
            token != null,
            _time.NowLocal
        );
        return token;
    }

    public async Task<RefreshToken?> GetActiveByUserIdAndTypeAsync(Guid userId, TokenType tokenType)
    {
        var nowUtc = _time.UtcNow;
        var token = await _ctx
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
        var nowUtc = _time.UtcNow;
        var tokens = await _ctx
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

            await _ctx.SaveChangesAsync();
            _logger.LogInformation(
                "Revoked all {TokenType} tokens for user {UserId} at {NowLocal}.",
                tokenType,
                userId,
                _time.NowLocal
            );
        }
    }
}
