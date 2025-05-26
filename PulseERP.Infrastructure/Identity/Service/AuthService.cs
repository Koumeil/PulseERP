using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using PulseERP.Contracts.Dtos.Auth;
using PulseERP.Contracts.Dtos.Services;
using PulseERP.Contracts.Interfaces.Services;
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

    public async Task<ServiceResult<AuthResponse>> RegisterAsync(RegisterRequest command)
    {
        // 1. Check if email exists
        var existingUser = await _userManager.FindByEmailAsync(command.Email);
        if (existingUser != null)
            return ServiceResult<AuthResponse>.Failure("Email already used.");

        // 2. Create domain user
        var domainUser = User.Create(
            command.FirstName,
            command.LastName,
            command.Email,
            command.Phone
        );

        // 3. Create Identity user
        var appUser = new ApplicationUser(domainUser.Id)
        {
            UserName = command.Email,
            Email = command.Email,
        };
        appUser.SetDomainUser(domainUser);

        // 4. Create user with password
        var result = await _userManager.CreateAsync(appUser, command.Password);
        if (!result.Succeeded)
            return ServiceResult<AuthResponse>.Failure(
                string.Join(", ", result.Errors.Select(e => e.Description))
            );

        // 5. Get user roles
        var roles = await _userManager.GetRolesAsync(appUser);

        // 6. Generate JWT access token
        var accessToken = _tokenService.GenerateAccessToken(appUser.Id, appUser.Email, roles);

        // 7. Generate refresh token
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(appUser.Id);

        // 8. Create AuthResponse DTO
        var authResponse = new AuthResponse(
            UserId: appUser.Id.ToString(),
            FirstName: domainUser.FirstName,
            LastName: domainUser.LastName,
            Email: domainUser.Email.ToString(),
            Token: accessToken,
            RefreshToken: refreshToken,
            ExpiresAt: DateTime.UtcNow.AddHours(1)
        );

        return ServiceResult<AuthResponse>.Success(authResponse);
    }

    public async Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest command)
    {
        // 1. Find ApplicationUser by email including the DomainUser entity
        var user = await _userManager
            .Users.Include(u => u.DomainUser)
            .SingleOrDefaultAsync(u => u.Email == command.Email);

        // 2. Validate user existence and password
        if (user == null || !await _userManager.CheckPasswordAsync(user, command.Password))
            return ServiceResult<AuthResponse>.Failure("Invalid credentials.");

        // 3. Ensure DomainUser is loaded
        if (user.DomainUser == null)
            return ServiceResult<AuthResponse>.Failure("Domain user data missing.");

        // 4. Get user roles
        var roles = await _userManager.GetRolesAsync(user);

        // 5. Check email is not null or empty
        if (string.IsNullOrEmpty(user.Email))
            throw new InvalidOperationException("User email cannot be null or empty.");

        // 6. Generate JWT access token
        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email, roles);

        // 7. Generate refresh token
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id);

        // 8. Build AuthResponse using DomainUser data
        var authResponse = new AuthResponse(
            UserId: user.Id.ToString(),
            FirstName: user.DomainUser.FirstName,
            LastName: user.DomainUser.LastName,
            Email: user.Email,
            Token: accessToken,
            RefreshToken: refreshToken,
            ExpiresAt: DateTime.UtcNow.AddHours(1)
        );

        // 8. Return success
        return ServiceResult<AuthResponse>.Success(authResponse);
    }

    public async Task<ServiceResult<AuthResponse>> RefreshTokenAsync(
        string token,
        string refreshToken
    )
    {
        // Extract principal from expired token
        var principal = GetPrincipalFromExpiredToken(token);
        if (principal == null)
            return ServiceResult<AuthResponse>.Failure("Invalid token.");

        // Parse userId claim
        var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub);
        if (!Guid.TryParse(userIdClaim?.Value, out var userId))
            return ServiceResult<AuthResponse>.Failure("Invalid token claims.");

        // Validate refresh token
        var isValid = await _tokenService.ValidateRefreshTokenAsync(refreshToken, userId);
        if (!isValid)
            return ServiceResult<AuthResponse>.Failure("Invalid refresh token.");

        // Load user including domain info
        var user = await _userManager
            .Users.Include(u => u.DomainUser)
            .SingleOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return ServiceResult<AuthResponse>.Failure("User not found.");

        if (user.DomainUser == null)
            return ServiceResult<AuthResponse>.Failure("Domain user data missing.");

        // Get roles
        var roles = await _userManager.GetRolesAsync(user);

        // Generate new tokens
        var newAccessToken = _tokenService.GenerateAccessToken(user.Id, user.Email, roles);
        var newRefreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id);

        // Revoke old refresh token
        await _tokenService.RevokeRefreshTokenAsync(refreshToken);

        // Create response with domain data
        var authResponse = new AuthResponse(
            UserId: user.Id.ToString(),
            FirstName: user.DomainUser.FirstName,
            LastName: user.DomainUser.LastName,
            Email: user.Email,
            Token: newAccessToken,
            RefreshToken: newRefreshToken,
            ExpiresAt: DateTime.UtcNow.AddHours(1)
        );

        // Return success with full info
        return ServiceResult<AuthResponse>.Success(authResponse);
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
