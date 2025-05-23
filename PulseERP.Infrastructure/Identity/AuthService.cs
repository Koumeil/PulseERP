using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using PulseERP.Application.DTOs.Auth;
using PulseERP.Application.Interfaces;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Interfaces.Persistence;

namespace PulseERP.Infrastructure.Identity;

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

    public async Task<AuthResult> RegisterAsync(RegisterCommand command)
    {
        var existingUser = await _userManager.FindByEmailAsync(command.Email);
        if (existingUser != null)
            return new AuthResult(false, null, null, new[] { "Email already used." });

        // 1. Créer l'entité DomainUser (ne pas encore l'ajouter)
        var domainUser = User.Create(
            command.FirstName,
            command.LastName,
            command.Email,
            command.Phone
        );

        // 2. Créer l'ApplicationUser avec le GUID du domainUser
        var appUser = new ApplicationUser(domainUser.Id)
        {
            UserName = command.Email,
            Email = command.Email,
        };
        appUser.SetDomainUser(domainUser);

        // 3. Créer l’utilisateur Identity (ça va persister le domainUser **via** navigation si bien configuré)
        var result = await _userManager.CreateAsync(appUser, command.Password);

        if (!result.Succeeded)
        {
            return new AuthResult(false, null, null, result.Errors.Select(e => e.Description));
        }

        var roles = await _userManager.GetRolesAsync(appUser);
        var accessToken = _tokenService.GenerateAccessToken(appUser.Id, appUser.Email, roles);
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(appUser.Id);

        return new AuthResult(true, accessToken, refreshToken);
    }

    // public async Task<AuthResult> RegisterAsync(RegisterCommand command)
    // {
    //     var existingUser = await _userManager.FindByEmailAsync(command.Email);
    //     if (existingUser != null)
    //         return new AuthResult(false, null, null, new[] { "Email already used." });

    //     var user = new ApplicationUser(Guid.NewGuid())
    //     {
    //         UserName = command.Email,
    //         Email = command.Email,
    //     };

    //     var result = await _userManager.CreateAsync(user, command.Password);
    //     if (!result.Succeeded)
    //         return new AuthResult(false, null, null, result.Errors.Select(e => e.Description));

    //     var roles = await _userManager.GetRolesAsync(user);
    //     var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email, roles);
    //     var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id);

    //     return new AuthResult(true, accessToken, refreshToken);
    // }

    public async Task<AuthResult> LoginAsync(LoginCommand command)
    {
        var user = await _userManager.FindByEmailAsync(command.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, command.Password))
            return new AuthResult(false, null, null, new[] { "Invalid credentials." });

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email, roles);
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id);

        return new AuthResult(true, accessToken, refreshToken);
    }

    public async Task<AuthResult> RefreshTokenAsync(string token, string refreshToken)
    {
        var principal = GetPrincipalFromExpiredToken(token);
        if (principal == null)
            return new AuthResult(false, null, null, new[] { "Invalid token." });

        var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub);
        if (!Guid.TryParse(userIdClaim?.Value, out var userId))
            return new AuthResult(false, null, null, new[] { "Invalid token claims." });

        var isValid = await _tokenService.ValidateRefreshTokenAsync(refreshToken, userId);
        if (!isValid)
            return new AuthResult(false, null, null, new[] { "Invalid refresh token." });

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return new AuthResult(false, null, null, new[] { "User not found." });

        var roles = await _userManager.GetRolesAsync(user);
        var newAccessToken = _tokenService.GenerateAccessToken(user.Id, user.Email, roles);
        var newRefreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id);

        await _tokenService.RevokeRefreshTokenAsync(refreshToken);

        return new AuthResult(true, newAccessToken, newRefreshToken);
    }

    public async Task LogoutAsync(string refreshToken)
    {
        await _tokenService.RevokeRefreshTokenAsync(refreshToken);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(
            Convert.FromBase64String(_configuration["Jwt:SecretKey"])
        );

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
