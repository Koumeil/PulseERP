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

/// <summary>
/// Handles access, refresh and password reset tokens with a single repository and TokenType logic.
/// </summary>
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

    public AccessToken GenerateAccessToken(Guid userId, string email, string role)
    {
        _logger.LogInformation("Generating access token for user {UserId}", userId);

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
        _logger.LogInformation(
            "Generating refresh token for user {UserId} from IP {Ip}",
            userId,
            ipAddress
        );

        var rawToken = _gen.GenerateToken();
        var hash = _hasher.Hash(rawToken);

        var expiresUtc = _time.UtcNow.AddDays(_settings.RefreshTokenExpirationDays);

        var entity = RefreshToken.Create(
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
        if (entity is null || !entity.IsActive)
        {
            _logger.LogWarning("Refresh token invalid or expired at {TimeLocal}.", _time.NowLocal);
            return new RefreshTokenValidationResult(false, null);
        }

        await _tokenRepo.RevokeAllByUserIdAndTypeAsync(entity.UserId, TokenType.Refresh);
        return new RefreshTokenValidationResult(true, entity.UserId);
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
