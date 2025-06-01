using Microsoft.Extensions.Logging;
using PulseERP.Abstractions.Security.DTOs;
using PulseERP.Abstractions.Security.Interfaces;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Errors;
using PulseERP.Domain.Interfaces;
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
    private readonly IEmailSenderService _smtpEmailService;

    public AuthenticationService(
        IPasswordService passwordService,
        ITokenService tokenService,
        IUserQueryRepository userQuery,
        IUserCommandRepository userCommand,
        ILogger<AuthenticationService> logger,
        IDateTimeProvider dateTimeProvider,
        IEmailSenderService smtpEmailService
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

        await EnsureEmailNotUsedAsync(request.Email);

        var passwordHash = _passwordService.HashPassword(
            Password.Create(request.Password).ToString()
        );

        var domainUser = User.Create(
            request.FirstName.Trim(),
            request.LastName.Trim(),
            EmailAddress.Create(request.Email),
            Phone.Create(request.Phone),
            passwordHash
        );

        await _userCommand.AddAsync(domainUser);
        await _userCommand.SaveChangesAsync();

        var userName = $"{domainUser.FirstName} {domainUser.LastName}";
        var loginUrl =
            $"https://app.pulseERP.com/login?email={Uri.EscapeDataString(request.Email)}";

        await _smtpEmailService.SendWelcomeEmailAsync(
            toEmail: request.Email,
            userFullName: userName,
            loginUrl: loginUrl
        );

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

        return new AuthResponse(CreateUserInfo(domainUser), accessToken, refreshToken);
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request,
        string ipAddress,
        string userAgent
    )
    {
        _logger.LogInformation("Attempting login for email {Email}", request.Email);

        var email = EmailAddress.Create(request.Email);
        var user =
            await _userQuery.GetByEmailAsync(email)
            ?? throw new NotFoundException("Invalid credentials.", email);

        var nowUtc = _dateTimeProvider.UtcNow;

        if (user.IsLockedOut(nowUtc))
            await HandleLockedAccountAsync(user, request.Email);

        if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
            await HandleFailedPasswordAsync(user, request.Email, nowUtc);

        if (!user.IsActive)
        {
            _logger.LogWarning("Login attempt on inactive account {Email}", request.Email);
            throw new UnauthorizedAccessException("Account is inactive.");
        }

        if (user.RequirePasswordChange)
        {
            _logger.LogInformation(
                "User {Email} must change password before continuing",
                request.Email
            );
            throw new UnauthorizedAccessException("Password change required.");
        }

        user.RegisterSuccessfulLogin(nowUtc);
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
            ?? throw new NotFoundException("User", validation.UserId.Value);

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

    #region Private Helpers

    private async Task EnsureEmailNotUsedAsync(string email)
    {
        var exists = await _userQuery.GetByEmailAsync(EmailAddress.Create(email));
        if (exists != null)
            throw new ValidationException(
                new Dictionary<string, string[]> { ["email"] = ["Email is already in use."] }
            );
    }

    private async Task HandleLockedAccountAsync(User user, string email)
    {
        _logger.LogWarning("Login attempt on locked account {Email}", email);

        // user.LockoutEnd est déjà en heure locale grâce au converter EF Core
        var lockoutLocal = user.LockoutEnd!.Value;
        var formatted = lockoutLocal.ToString("HH:mm dd MMM yyyy");

        await _smtpEmailService.SendAccountLockedEmailAsync(
            user.Email.ToString(),
            $"{user.FirstName} {user.LastName}",
            lockoutLocal
        );

        throw new UnauthorizedAccessException($"Account is locked. Try again at {formatted}.");
    }

    private async Task HandleFailedPasswordAsync(User user, string email, DateTime nowUtc)
    {
        var lockoutEndUtc = user.RegisterFailedLoginAttempt(nowUtc);
        await _userCommand.SaveChangesAsync();

        if (lockoutEndUtc.HasValue)
        {
            _logger.LogWarning(
                "User {Email} just got locked out. Now: {NowUtc}, LockoutEnd: {LockoutEndUtc}, FailedAttempts: {Count}",
                email,
                nowUtc,
                lockoutEndUtc.Value,
                user.FailedLoginAttempts
            );

            // Le converter EF Core garantit que user.LockoutEnd est en local,
            // mais ici lockoutEndUtc vient juste d'être calculé en UTC. On convertit :
            var lockoutLocal = _dateTimeProvider.ConvertToLocal(lockoutEndUtc.Value);
            var formatted = lockoutLocal.ToString("HH:mm dd MMM yyyy");

            await _smtpEmailService.SendAccountLockedEmailAsync(
                user.Email.ToString(),
                $"{user.FirstName} {user.LastName}",
                lockoutLocal
            );

            throw new UnauthorizedAccessException($"Account is locked. Try again at {formatted}.");
        }

        if (user.WillBeLockedNextAttempt)
        {
            _logger.LogInformation("One more attempt before lockout for {Email}", email);
            throw new UnauthorizedAccessException(
                "Invalid credentials. Warning: one more failed attempt will lock your account."
            );
        }

        _logger.LogWarning(
            "Invalid login attempt for {Email}. FailedAttempts: {Count}",
            email,
            user.FailedLoginAttempts
        );
        throw new UnauthorizedAccessException("Invalid credentials.");
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

    #endregion
}
