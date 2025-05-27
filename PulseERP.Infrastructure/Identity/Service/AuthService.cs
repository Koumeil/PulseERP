using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using PulseERP.Application.Exceptions;
using PulseERP.Application.Interfaces.Services;
using PulseERP.Contracts.Dtos.Auth;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Interfaces.Repositories;
using PulseERP.Infrastructure.Identity.Entities;

namespace PulseERP.Infrastructure.Identity.Service;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;
    private readonly IUserRepository _userRepository;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IConfiguration configuration,
        ILogger<AuthService> logger,
        IUserRepository userRepository
    )
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _configuration = configuration;
        _logger = logger;
        _userRepository = userRepository;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest command)
    {
        var existingUser = await _userManager.FindByEmailAsync(command.Email);
        if (existingUser != null)
        {
            throw new ValidationException(
                new Dictionary<string, string[]>
                {
                    ["email"] = new[] { "Email is already in use." },
                }
            );
        }

        var domainUser = User.Create(
            command.FirstName,
            command.LastName,
            command.Email,
            command.Phone
        );

        var appUser = new ApplicationUser(domainUser.Id)
        {
            UserName = command.Email,
            Email = command.Email,
        };
        appUser.SetDomainUser(domainUser);

        var result = await _userManager.CreateAsync(appUser, command.Password);
        if (!result.Succeeded)
        {
            throw new ValidationException(
                new Dictionary<string, string[]>
                {
                    ["password"] = result.Errors.Select(e => e.Description).ToArray(),
                }
            );
        }

        var roles = await _userManager.GetRolesAsync(appUser);

        var accessToken = _tokenService.GenerateAccessToken(appUser.Id, appUser.Email, roles);
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(appUser.Id);

        return new AuthResponse(
            UserId: appUser.Id.ToString(),
            FirstName: domainUser.FirstName,
            LastName: domainUser.LastName,
            Email: domainUser.Email.ToString(),
            Token: accessToken,
            RefreshToken: refreshToken,
            ExpiresAt: DateTime.UtcNow.AddHours(1)
        );
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest command)
    {
        var user = await _userManager
            .Users.Include(u => u.DomainUser)
            .SingleOrDefaultAsync(u => u.Email == command.Email);

        if (user == null || !await _userManager.CheckPasswordAsync(user, command.Password))
            throw new UnauthorizedAccessException("Invalid credentials.");

        if (user.DomainUser == null)
            throw new InvalidOperationException("Domain user data is missing.");

        var roles = await _userManager.GetRolesAsync(user);

        if (string.IsNullOrEmpty(user.Email))
            throw new InvalidOperationException("User email cannot be null or empty.");

        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email, roles);
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id);

        return new AuthResponse(
            UserId: user.Id.ToString(),
            FirstName: user.DomainUser.FirstName,
            LastName: user.DomainUser.LastName,
            Email: user.Email,
            Token: accessToken,
            RefreshToken: refreshToken,
            ExpiresAt: DateTime.UtcNow.AddHours(1)
        );
    }

    public async Task<AuthResponse> RefreshTokenAsync(string token, string refreshToken)
    {
        var principal = GetPrincipalFromExpiredToken(token);
        if (principal == null)
            throw new UnauthorizedAccessException("Invalid token.");

        var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub);
        if (!Guid.TryParse(userIdClaim?.Value, out var userId))
            throw new UnauthorizedAccessException("Invalid token claims.");

        var isValid = await _tokenService.ValidateRefreshTokenAsync(refreshToken, userId);
        if (!isValid)
            throw new UnauthorizedAccessException("Invalid refresh token.");

        var user = await _userManager
            .Users.Include(u => u.DomainUser)
            .SingleOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new NotFoundException("User", userId);

        if (user.DomainUser == null)
            throw new InvalidOperationException("Domain user data is missing.");

        var roles = await _userManager.GetRolesAsync(user);

        var newAccessToken = _tokenService.GenerateAccessToken(user.Id, user.Email, roles);
        var newRefreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id);

        await _tokenService.RevokeRefreshTokenAsync(refreshToken);

        return new AuthResponse(
            UserId: user.Id.ToString(),
            FirstName: user.DomainUser.FirstName,
            LastName: user.DomainUser.LastName,
            Email: user.Email,
            Token: newAccessToken,
            RefreshToken: newRefreshToken,
            ExpiresAt: DateTime.UtcNow.AddHours(1)
        );
    }

    public async Task LogoutAsync(string refreshToken)
    {
        await _tokenService.RevokeRefreshTokenAsync(refreshToken);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var secretKeyBase64 = _configuration["Jwt:SecretKey"];

        if (string.IsNullOrEmpty(secretKeyBase64))
        {
            _logger.LogError("Jwt:SecretKey is missing or empty in configuration.");
            return null;
        }

        var key = new SymmetricSecurityKey(Convert.FromBase64String(secretKeyBase64));

        try
        {
            var principal = tokenHandler.ValidateToken(
                token,
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero,
                    ValidateLifetime = false,
                },
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
                return principal;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Token validation failed: {Message}", ex.Message);
            return null;
        }
    }
}
