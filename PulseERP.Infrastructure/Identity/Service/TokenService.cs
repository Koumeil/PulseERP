using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using PulseERP.Application.Interfaces.Services;
using PulseERP.Application.Settings;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Interfaces.Repositories;
using PulseERP.Shared.Dtos.Auth.Token;

namespace PulseERP.Infrastructure.Identity.Service;

public class TokenService : ITokenService
{
    private readonly ITokenRepository _tokenRepository;
    private readonly JwtSettings _jwtSettings;
    private readonly byte[] _secretKey;
    private readonly ILogger<TokenService> _logger;

    public TokenService(
        IConfiguration configuration,
        ITokenRepository tokenRepository,
        ILogger<TokenService> logger
    )
    {
        _tokenRepository =
            tokenRepository ?? throw new ArgumentNullException(nameof(tokenRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _jwtSettings =
            configuration.GetSection("JwtSettings").Get<JwtSettings>()
            ?? throw new InvalidOperationException("JwtSettings configuration section is missing.");

        if (string.IsNullOrWhiteSpace(_jwtSettings.SecretKey))
            throw new InvalidOperationException("JWT SecretKey must be configured.");

        try
        {
            _secretKey = Convert.FromBase64String(_jwtSettings.SecretKey);
        }
        catch (FormatException ex)
        {
            _logger.LogError(ex, "JWT SecretKey is not a valid Base64 string.");
            throw new InvalidOperationException("JWT SecretKey is not a valid Base64 string.", ex);
        }

        ValidateJwtSettings(_jwtSettings);
    }

    private void ValidateJwtSettings(JwtSettings settings)
    {
        if (settings.AccessTokenExpirationMinutes <= 0)
            throw new InvalidOperationException(
                "AccessTokenExpirationMinutes must be a positive integer."
            );

        if (settings.RefreshTokenExpirationDays <= 0)
            throw new InvalidOperationException(
                "RefreshTokenExpirationDays must be a positive integer."
            );

        if (string.IsNullOrWhiteSpace(settings.Issuer))
            throw new InvalidOperationException("JWT Issuer must be configured.");

        if (string.IsNullOrWhiteSpace(settings.Audience))
            throw new InvalidOperationException("JWT Audience must be configured.");
    }

    public AccessTokenDto GenerateAccessToken(Guid userId, string email, IList<string> roles)
    {
        _logger.LogInformation("Generating access token for user {UserId}", userId);

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(_secretKey);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256Signature
            ),
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        _logger.LogInformation(
            "Access token generated for user {UserId} expiring at {Expiration}",
            userId,
            expires
        );

        return new AccessTokenDto(tokenString, expires);
    }

    public async Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId)
    {
        _logger.LogInformation("Generating refresh token for user {UserId}", userId);

        var refreshToken = new RefreshToken
        {
            Token = GenerateSecureRandomToken(),
            UserId = userId,
            Expires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
        };

        await _tokenRepository.AddAsync(refreshToken);

        _logger.LogInformation(
            "Refresh token generated for user {UserId} expiring at {Expiration}",
            userId,
            refreshToken.Expires
        );

        return refreshToken;
    }

    private static string GenerateSecureRandomToken()
    {
        var randomBytes = new byte[64]; // 512 bits
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public Guid? ValidateAccessToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("ValidateAccessToken called with empty token");
            return null;
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(_secretKey);

        try
        {
            var validationParams = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
            };

            var principal = tokenHandler.ValidateToken(
                token,
                validationParams,
                out var validatedToken
            );

            if (
                validatedToken is JwtSecurityToken jwtToken
                && jwtToken.Header.Alg.Equals(
                    SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase
                )
            )
            {
                var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub);
                if (Guid.TryParse(userIdClaim?.Value, out var userId))
                {
                    _logger.LogInformation(
                        "Access token validated successfully for user {UserId}",
                        userId
                    );
                    return userId;
                }
                else
                {
                    _logger.LogWarning("Access token has invalid subject claim");
                }
            }
            else
            {
                _logger.LogWarning("Access token uses unexpected algorithm or is invalid");
            }
        }
        catch (SecurityTokenExpiredException ex)
        {
            _logger.LogWarning(ex, "Access token expired");
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning(ex, "Access token validation failed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during access token validation");
        }

        return null;
    }

    public async Task<bool> ValidateRefreshTokenAsync(string refreshToken, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            _logger.LogWarning("ValidateRefreshTokenAsync called with empty token");
            return false;
        }

        try
        {
            var tokenEntity = await _tokenRepository.GetByTokenAsync(refreshToken);
            var isValid =
                tokenEntity != null && tokenEntity.UserId == userId && tokenEntity.IsActive;

            if (isValid)
                _logger.LogInformation(
                    "Refresh token validated successfully for user {UserId}",
                    userId
                );
            else
                _logger.LogWarning("Refresh token validation failed for user {UserId}", userId);

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating refresh token");
            return false;
        }
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            _logger.LogWarning("RevokeRefreshTokenAsync called with empty token");
            return;
        }

        try
        {
            var tokenEntity = await _tokenRepository.GetByTokenAsync(refreshToken);
            if (tokenEntity != null && tokenEntity.IsActive)
            {
                tokenEntity.Revoked = DateTime.UtcNow;
                await _tokenRepository.UpdateAsync(tokenEntity);
                _logger.LogInformation(
                    "Refresh token revoked for user {UserId}",
                    tokenEntity.UserId
                );
            }
            else
            {
                _logger.LogWarning("Refresh token not found or already revoked");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking refresh token");
        }
    }

    public Task<RefreshToken?> GetRefreshTokenInfoAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("GetRefreshTokenInfoAsync called with empty token");
            return Task.FromResult<RefreshToken?>(null);
        }

        return _tokenRepository.GetByTokenAsync(token);
    }
}
