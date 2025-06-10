using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PulseERP.Abstractions.Security.DTOs;
using PulseERP.Abstractions.Security.Interfaces;
using PulseERP.Abstractions.Settings;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Enums.Token;
using PulseERP.Domain.Interfaces;

namespace PulseERP.Infrastructure.Identity;

public class TokenService : ITokenService
{
    private readonly JwtSettings _settings;
    private readonly byte[] _key;
    private readonly ITokenRepository _tokenRepo;
    private readonly ITokenGeneratorService _gen;
    private readonly ITokenHasherService _hasher;
    private readonly IDateTimeProvider _time;
    private readonly ILogger<TokenService> _logger;

    public TokenService(
        IOptions<JwtSettings> opts,
        ITokenRepository tokenRepo,
        ITokenGeneratorService gen,
        ITokenHasherService hasher,
        IDateTimeProvider time,
        ILogger<TokenService> logger
    )
    {
        _settings = opts?.Value ?? throw new ArgumentNullException(nameof(opts));
        if (string.IsNullOrWhiteSpace(_settings.SecretKey))
            throw new InvalidOperationException("JWT SecretKey must be configured.");

        _key = Encoding.UTF8.GetBytes(_settings.SecretKey);
        _tokenRepo = tokenRepo;
        _gen = gen;
        _hasher = hasher;
        _time = time;
        _logger = logger;
    }
    public async Task<string> GenerateActivationTokenAsync(Guid userId)
    {
        // 1. Generate raw token
        var rawToken = _gen.GenerateToken();
        var hash = _hasher.Hash(rawToken);

        // 2. Define expiration for activation token (ex: 2 days)
        var expiresUtc = _time.UtcNow.AddDays(2);

        // 3. Create token entity (generic model: RefreshToken)
        var entity = TokenEntity.Create(
            _time,
            userId,
            hash,
            TokenType.Activation,
            expiresUtc
        );

        // 4. Persist token
        await _tokenRepo.AddAsync(entity);

        // 5. Return the RAW token (the one to send in email link)
        return rawToken;
    }

    public AccessToken GenerateAccessToken(Guid userId, string email, string role)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var creds = new SigningCredentials(
            new SymmetricSecurityKey(_key),
            SecurityAlgorithms.HmacSha256Signature
        );

        var expiresUtc = _time.UtcNow.AddMinutes(_settings.AccessTokenExpirationMinutes);
        var jwt = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiresUtc,
            signingCredentials: creds
        );

        var token = new JwtSecurityTokenHandler().WriteToken(jwt);
        var expiresLocal = _time.ConvertToLocal(expiresUtc);

        return new AccessToken(token, expiresLocal);
    }

    public async Task<RefreshTokenDto> GenerateRefreshTokenAsync(
        Guid userId,
        string ipAddress,
        string userAgent
    )
    {
        var rawToken = _gen.GenerateToken();
        var hash = _hasher.Hash(rawToken);

        var expiresUtc = _time.UtcNow.AddDays(_settings.RefreshTokenExpirationDays);

        var entity = TokenEntity.Create(
            _time,
            userId,
            hash,
            TokenType.Refresh,
            expiresUtc,
            userAgent,
            ipAddress
        );

        await _tokenRepo.AddAsync(entity);

        var expiresLocal = _time.ConvertToLocal(expiresUtc);

        return new RefreshTokenDto(rawToken, expiresLocal);
    }

    public async Task<RefreshTokenValidationResult> ValidateAndRevokeRefreshTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return new RefreshTokenValidationResult(false, null);

        var hash = _hasher.Hash(token);
        var entity = await _tokenRepo.GetByTokenAndTypeAsync(hash, TokenType.Refresh);
        if (entity is null || !entity!.IsActive)
        {
            _logger.LogWarning("Refresh token invalid or expired at {TimeLocal}.", _time.NowLocal);
            return new RefreshTokenValidationResult(false, null);
        }
        if (entity.Revoked is not null)
            return new RefreshTokenValidationResult(false, null);

        await _tokenRepo.RevokeAllByUserIdAndTypeAsync(entity.UserId, TokenType.Refresh);
        return new RefreshTokenValidationResult(true, entity.UserId);
    }

    public async Task<ActivationTokenValidationResult> ValidateAndRevokeActivationTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return new ActivationTokenValidationResult(false, null);

        var hash = _hasher.Hash(token);
        var entity = await _tokenRepo.GetByTokenAndTypeAsync(hash, TokenType.Activation);

        if (entity is null || !entity.IsActive)
        {
            _logger.LogWarning("Activation token invalid or expired at {TimeLocal}.", _time.NowLocal);
            return new ActivationTokenValidationResult(false, null);
        }
        if (entity.Revoked is not null)
            return new ActivationTokenValidationResult(false, null);

        await _tokenRepo.RevokeAllByUserIdAndTypeAsync(entity.UserId, TokenType.Activation);

        return new ActivationTokenValidationResult(true, entity.UserId);
    }


    public Guid? ValidateAccessToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(
                token,
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(_key),
                    ValidateIssuer = true,
                    ValidIssuer = _settings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _settings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                },
                out var validatedToken
            );

            if (
                validatedToken is JwtSecurityToken jwt
                && jwt.Header.Alg.Equals(
                    SecurityAlgorithms.HmacSha256,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                var sub = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                if (Guid.TryParse(sub, out var userId))
                    return userId;
            }
        }
        catch (SecurityTokenExpiredException ex)
        {
            _logger.LogWarning(ex, "Access token expired");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Access token validation failed");
        }

        return null;
    }
}
