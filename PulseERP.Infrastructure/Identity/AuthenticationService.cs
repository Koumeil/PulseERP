using Microsoft.Extensions.Logging;
using PulseERP.Application.Interfaces;
using PulseERP.Domain.Dtos.Auth;
using PulseERP.Domain.Dtos.Users;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Errors;
using PulseERP.Domain.Interfaces.Repositories;
using PulseERP.Domain.Interfaces.Services;
using PulseERP.Domain.ValueObjects;

namespace PulseERP.Infrastructure.Identity;

public class AuthenticationService : IAuthenticationService
{
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;
    private readonly IUserQueryRepository _userQuery;
    private readonly IUserCommandRepository _userCommand;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ISmtpEmailService _smtpEmailService;

    public AuthenticationService(
        IPasswordService passwordService,
        ITokenService tokenService,
        IUserQueryRepository userQuery,
        IUserCommandRepository userCommand,
        ILogger<AuthenticationService> logger,
        IDateTimeProvider dateTimeProvider,
        ISmtpEmailService smtpEmailService
    )
    {
        _passwordService = passwordService;
        _tokenService = tokenService;
        _userQuery = userQuery;
        _userCommand = userCommand;
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
        _smtpEmailService = smtpEmailService;
    }

    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request,
        string ipAddress,
        string userAgent
    )
    {
        _logger.LogInformation("Registering new user with email {Email}", request.Email);

        // 1. Vérifier que l'email n'est pas déjà utilisé
        await EnsureEmailNotUsedAsync(request.Email);

        // 2. Hasher le mot de passe
        var password = Password.Create(request.Password);
        var passwordHash = _passwordService.HashPassword(password.ToString());

        // 3. Créer l'utilisateur domaine
        var domainUser = User.Create(
            request.FirstName,
            request.LastName,
            Email.Create(request.Email),
            Phone.Create(request.Phone),
            passwordHash
        );

        // 4. Persister en base
        await _userCommand.AddAsync(domainUser);
        await _userCommand.SaveChangesAsync();

        // 5. Préparer le prénom + nom complet
        var userName = $"{request.FirstName.Trim()} {request.LastName.Trim()}";

        // 6. Générer l'URL de connexion (à adapter selon ton front)
        var loginUrl =
            $"https://app.pulseERP.com/login?email={Uri.EscapeDataString(request.Email)}";

        // 7. Envoyer l'email de bienvenue
        await _smtpEmailService.SendWelcomeEmailAsync(
            toEmail: request.Email,
            userFullName: userName,
            loginUrl: loginUrl
        );

        // 8. Générer les tokens
        var accessToken = _tokenService.GenerateAccessToken(
            domainUser.Id,
            domainUser.Email.ToString(),
            domainUser.Role.ToString()
        );
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(
            domainUser.Id,
            ipAddress,
            userAgent
        );

        // 9. Retourner la réponse d'authentification
        return new AuthResponse(CreateUserInfo(domainUser), accessToken, refreshToken);
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request,
        string ipAddress,
        string userAgent
    )
    {
        _logger.LogInformation("Attempting login for email {Email}", request.Email);

        var email = Email.Create(request.Email);
        var user = await _userQuery.GetByEmailAsync(email);

        if (user is null)
        {
            _logger.LogWarning("Invalid login attempt for unknown user {Email}", request.Email);
            throw new NotFoundException("Invalid credentials.", email);
        }

        var now = _dateTimeProvider.UtcNow;

        // 🔐 Déjà bloqué
        if (user.IsLockedOut(now))
        {
            _logger.LogWarning("Login attempt on locked account {Email}", request.Email);

            await _smtpEmailService.SendAccountLockedEmailAsync(
                user.Email.ToString(),
                $"{user.FirstName} {user.LastName}",
                user.LockoutEnd!.Value
            );

            throw new UnauthorizedAccessException(
                $"Account is locked. Try again at {user.LockoutEnd.Value:HH:mm} UTC."
            );
        }

        // ❌ Mot de passe incorrect
        if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
        {
            var lockoutEnd = user.RegisterFailedLoginAttempt(now);
            await _userCommand.SaveChangesAsync();

            if (lockoutEnd.HasValue)
            {
                _logger.LogWarning(
                    "User {Email} just got locked out. Now: {Now}, LockoutEnd: {LockoutEnd}, FailedAttempts: {Count}",
                    request.Email,
                    now,
                    lockoutEnd.Value,
                    user.FailedLoginAttempts
                );

                await _smtpEmailService.SendAccountLockedEmailAsync(
                    user.Email.ToString(),
                    $"{user.FirstName} {user.LastName}",
                    lockoutEnd.Value
                );

                throw new UnauthorizedAccessException(
                    $"Account is locked. Try again at {lockoutEnd.Value:HH:mm} UTC."
                );
            }

            if (user.WillBeLockedNextAttempt)
            {
                _logger.LogInformation(
                    "One more attempt before lockout for {Email}",
                    request.Email
                );

                throw new UnauthorizedAccessException(
                    "Invalid credentials. Warning: one more failed attempt will lock your account."
                );
            }

            _logger.LogWarning(
                "Invalid login attempt for {Email}. FailedAttempts: {Count}",
                request.Email,
                user.FailedLoginAttempts
            );

            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        // 🔒 Compte désactivé
        if (!user.IsActive)
        {
            _logger.LogWarning("Login attempt on inactive account {Email}", request.Email);
            throw new UnauthorizedAccessException("Account is inactive.");
        }

        // 🔄 Changement de mot de passe obligatoire
        if (user.RequirePasswordChange)
        {
            _logger.LogInformation(
                "User {Email} must change password before continuing",
                request.Email
            );
            throw new UnauthorizedAccessException("Password change required.");
        }

        // ✅ Succès
        user.RegisterSuccessfulLogin(now);
        await _userCommand.SaveChangesAsync();

        var accessToken = _tokenService.GenerateAccessToken(
            user.Id,
            user.Email.ToString(),
            user.Role.ToString()
        );

        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(
            user.Id,
            ipAddress,
            userAgent
        );

        return new AuthResponse(CreateUserInfo(user), accessToken, refreshToken);
    }

    public async Task<AuthResponse> RefreshTokenAsync(
        string refreshToken,
        string ipAddress,
        string userAgent
    )
    {
        var validation = await _tokenService.ValidateAndRevokeRefreshTokenAsync(refreshToken);

        if (!validation.IsValid || validation.UserId is null)
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        var user =
            await _userQuery.GetByIdAsync(validation.UserId.Value)
            ?? throw new NotFoundException("User", validation.UserId);

        var newAccessToken = _tokenService.GenerateAccessToken(
            user.Id,
            user.Email.ToString(),
            user.Role.ToString()
        );
        var newRefreshToken = await _tokenService.GenerateRefreshTokenAsync(
            user.Id,
            ipAddress,
            userAgent
        );

        return new AuthResponse(CreateUserInfo(user), newAccessToken, newRefreshToken);
    }

    public async Task LogoutAsync(string refreshToken)
    {
        await _tokenService.ValidateAndRevokeRefreshTokenAsync(refreshToken);
        _logger.LogInformation("User logged out and refresh token revoked");
    }

    private async Task EnsureEmailNotUsedAsync(string email)
    {
        var exist = await _userQuery.GetByEmailAsync(Email.Create(email));
        if (exist != null)
            throw new ValidationException(
                new Dictionary<string, string[]> { ["email"] = ["Email is already in use."] }
            );
    }

    private static UserInfo CreateUserInfo(User user) =>
        new(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email.ToString(),
            user.Phone.ToString(),
            user.Role.ToString()
        );
}
