using System.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PulseERP.Application.Exceptions;
using PulseERP.Application.Interfaces.Services;
using PulseERP.Domain.Entities;
using PulseERP.Infrastructure.Identity.Entities;
using PulseERP.Shared.Dtos.Auth;
using PulseERP.Shared.Dtos.Auth.Token;
using PulseERP.Shared.Dtos.Users;

namespace PulseERP.Infrastructure.Identity.Service;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly ISmtpEmailService _emailService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        ISmtpEmailService emailService,
        ILogger<AuthService> logger
    )
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        _logger.LogInformation("Registering new user with email {Email}", request.Email);

        await EnsureEmailNotUsedAsync(request.Email);

        var domainUser = CreateDomainUser(request);
        var appUser = CreateApplicationUser(domainUser, request);

        await CreateIdentityUserAsync(appUser);
        await SendInvitationEmailAsync(appUser, domainUser);

        var roles = await _userManager.GetRolesAsync(appUser);
        var accessToken = _tokenService.GenerateAccessToken(appUser.Id, appUser.Email!, roles);
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(appUser.Id);

        _logger.LogInformation("User {UserId} registered successfully", appUser.Id);

        return new AuthResponse(
            CreateUserInfo(domainUser),
            accessToken,
            new RefreshTokenDto(refreshToken.Token, refreshToken.Expires)
        );
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        _logger.LogInformation("Attempting login for email {Email}", request.Email);

        var user = await GetUserByEmailAsync(request.Email);

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
        {
            _logger.LogWarning("Invalid password for user {UserId}", user.Id);
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        _logger.LogInformation("Refreshing token");

        var storedToken = await _tokenService.GetRefreshTokenInfoAsync(refreshToken);
        if (storedToken == null || storedToken.Revoked != null || storedToken.IsExpired)
        {
            _logger.LogWarning("Invalid or expired refresh token");
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");
        }

        var user = await GetUserByIdAsync(storedToken.UserId);

        await _tokenService.RevokeRefreshTokenAsync(refreshToken);

        return await GenerateAuthResponseAsync(user);
    }

    public async Task LogoutAsync(string refreshToken)
    {
        _logger.LogInformation("Logging out refresh token");
        await _tokenService.RevokeRefreshTokenAsync(refreshToken);
    }

    // ----------- Méthodes privées ------------

    private async Task<AuthResponse> GenerateAuthResponseAsync(ApplicationUser user)
    {
        var domainUser = EnsureDomainUserIsNotNull(user);
        var email = EnsureEmailIsNotNull(user);
        var roles = await _userManager.GetRolesAsync(user);

        var accessToken = _tokenService.GenerateAccessToken(user.Id, email, roles);
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id);

        return new AuthResponse(
            CreateUserInfo(domainUser),
            accessToken,
            new RefreshTokenDto(refreshToken.Token, refreshToken.Expires)
        );
    }

    private async Task<ApplicationUser> GetUserByEmailAsync(string email)
    {
        var user = await _userManager
            .Users.Include(u => u.DomainUser)
            .SingleOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            _logger.LogWarning("User not found for email {Email}", email);
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        return user;
    }

    private async Task<ApplicationUser> GetUserByIdAsync(Guid userId)
    {
        var user = await _userManager
            .Users.Include(u => u.DomainUser)
            .SingleOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            _logger.LogWarning("User not found with Id {UserId}", userId);
            throw new NotFoundException("User", userId);
        }

        return user;
    }

    private static User CreateDomainUser(RegisterRequest request) =>
        User.Create(
            request.FirstName,
            request.LastName,
            Domain.ValueObjects.Email.Create(request.Email),
            Domain.ValueObjects.PhoneNumber.Create(request.Phone)
        );

    private static ApplicationUser CreateApplicationUser(User domainUser, RegisterRequest request)
    {
        var appUser = new ApplicationUser(domainUser.Id)
        {
            UserName = request.Email,
            Email = request.Email,
            EmailConfirmed = false,
            PhoneNumber = request.Phone,
        };
        appUser.SetDomainUser(domainUser);
        return appUser;
    }

    private async Task CreateIdentityUserAsync(ApplicationUser appUser)
    {
        var result = await _userManager.CreateAsync(appUser);
        if (!result.Succeeded)
        {
            _logger.LogWarning(
                "Failed to create identity user {UserId}: {Errors}",
                appUser.Id,
                string.Join(", ", result.Errors.Select(e => e.Description))
            );

            throw new ValidationException(
                new Dictionary<string, string[]>
                {
                    ["identity"] = result.Errors.Select(e => e.Description).ToArray(),
                }
            );
        }
    }

    private async Task SendInvitationEmailAsync(ApplicationUser appUser, User domainUser)
    {
        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(appUser);
        var encodedToken = HttpUtility.UrlEncode(resetToken);

        var frontendUrl = "https://localhost:5003";
        var invitationLink = $"{frontendUrl}/set-password?userId={appUser.Id}&token={encodedToken}";

        var subject = "Bienvenue sur PulseERP – Définissez votre mot de passe";
        var body =
            $@"
            <p>Bonjour {domainUser.FirstName},</p>
            <p>Votre compte a été créé avec succès. Veuillez cliquer sur le lien ci-dessous pour définir votre mot de passe :</p>
            <p><a href=""{invitationLink}"">Configurer mon mot de passe</a></p>
            <p>Ce lien expirera dans quelques heures.</p>";

        await _emailService.SendEmailAsync(appUser.Email!, subject, body);
    }

    private async Task EnsureEmailNotUsedAsync(string email)
    {
        if (await _userManager.FindByEmailAsync(email) != null)
        {
            _logger.LogWarning("Email already in use: {Email}", email);
            throw new ValidationException(
                new Dictionary<string, string[]>
                {
                    ["email"] = new[] { "Email is already in use." },
                }
            );
        }
    }

    private static User EnsureDomainUserIsNotNull(ApplicationUser user) =>
        user.DomainUser ?? throw new InvalidOperationException("Domain user cannot be null.");

    private static string EnsureEmailIsNotNull(ApplicationUser user) =>
        user.Email ?? throw new InvalidOperationException("User email cannot be null.");

    private static UserInfo CreateUserInfo(User domainUser) =>
        new(domainUser.Id, domainUser.FirstName, domainUser.LastName, domainUser.Email.ToString());
}
