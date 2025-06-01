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
    private readonly IRefreshTokenRepository _repo;
    private readonly ITokenGenerator _gen;
    private readonly ITokenHasher _hasher;
    private readonly IDateTimeProvider _time;
    private readonly ILogger<TokenService> _logger;

    public TokenService(
        IOptions<JwtSettings> opts,
        IRefreshTokenRepository repo,
        ITokenGenerator gen,
        ITokenHasher hasher,
        IDateTimeProvider time,
        ILogger<TokenService> logger
    )
    {
        _settings = opts?.Value ?? throw new ArgumentNullException(nameof(opts));
        if (string.IsNullOrWhiteSpace(_settings.SecretKey))
            throw new InvalidOperationException("JWT SecretKey must be configured.");

        _key = Encoding.UTF8.GetBytes(_settings.SecretKey);
        _repo = repo;
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

        // 1) Calcul de l’expiration UTC
        var expiresUtc = _time.UtcNow.AddMinutes(_settings.AccessTokenExpirationMinutes);

        // 2) Création du JWT avec l’heure UTC
        var jwt = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiresUtc,
            signingCredentials: creds
        );

        var token = new JwtSecurityTokenHandler().WriteToken(jwt);

        // 3) Conversion de l’expiration en heure locale (Bruxelles)
        var expiresLocal = _time.ConvertToLocal(expiresUtc);

        // 4) On renvoie l’AccessToken avec l’heure locale dans le champ Expires
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

        var existing = await _repo.GetActiveByUserIdAsync(userId);
        if (existing != null)
            await _repo.RevokeForUserAsync(userId);

        // 1) Calcul de l’expiration UTC pour le refresh token
        var expiresUtc = _time.UtcNow.AddDays(_settings.RefreshTokenExpirationDays);

        // 2) Création de l’entité avec l’expiration UTC
        var entity = RefreshToken.Create(
            _time,
            userId,
            hash,
            TokenType.Refresh,
            expiresUtc,
            userAgent,
            ipAddress
        );

        await _repo.AddAsync(entity);

        // 3) Conversion de l’expiration en heure locale
        var expiresLocal = _time.ConvertToLocal(expiresUtc);

        return new RefreshTokenDto(rawToken, expiresLocal);
    }

    public async Task<RefreshTokenValidationResult> ValidateAndRevokeRefreshTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return new RefreshTokenValidationResult(false, null);

        var hash = _hasher.Hash(token);
        var entity = await _repo.GetByTokenAsync(hash);
        if (entity is null || !entity.IsActive)
            return new RefreshTokenValidationResult(false, null);

        await _repo.RevokeForUserAsync(entity.UserId);
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
