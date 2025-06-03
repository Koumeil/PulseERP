using Microsoft.Extensions.Logging;
using PulseERP.Abstractions.Security.DTOs;
using PulseERP.Abstractions.Security.Interfaces;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Errors;
using PulseERP.Domain.Interfaces;
using PulseERP.Domain.Security.DTOs;
using PulseERP.Domain.Security.Interfaces;
using PulseERP.Domain.ValueObjects.Adresses;
using PulseERP.Domain.ValueObjects.Passwords;

namespace PulseERP.Infrastructure.Identity;

/// <summary>
/// Handles user registration, authentication (login), token refresh and logout.
/// All logs are in Brussels local time. Password expiration enforced via RequirePasswordChange.
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IEmailSenderService _emailSender;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationService"/> class.
    /// </summary>
    public AuthenticationService(
        IPasswordService passwordService,
        ITokenService tokenService,
        IUserRepository userRepository,
        ILogger<AuthenticationService> logger,
        IDateTimeProvider dateTimeProvider,
        IEmailSenderService emailSender
    )
    {
        _passwordService = passwordService;
        _tokenService = tokenService;
        _userRepository = userRepository;
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
        _emailSender = emailSender;
    }

    /// <summary>
    /// Registers a new user, enforces unique email, sends welcome email, returns AuthResponse.
    /// </summary>
    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request,
        string ipAddress,
        string userAgent
    )
    {
        var start = DateTime.UtcNow;
        _logger.LogInformation(
            "User registration START: Email={Email}, IP={IP}, UA={UA}, At={TimeLocal}",
            request.Email,
            ipAddress,
            userAgent,
            _dateTimeProvider.NowLocal
        );

        await EnsureEmailNotUsedAsync(request.Email);

        var password = Password.Create(request.Password);
        var passwordHash = _passwordService.HashPassword(password.Value);

        var user = User.Create(
            request.FirstName.Trim(),
            request.LastName.Trim(),
            request.Email,
            request.Phone,
            passwordHash
        );

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        var userName = $"{user.FirstName} {user.LastName}";
        var loginUrl =
            $"https://app.pulseERP.com/login?email={Uri.EscapeDataString(request.Email)}";
        await _emailSender.SendWelcomeEmailAsync(
            toEmail: user.Email.Value,
            userFullName: userName,
            loginUrl: loginUrl
        );

        _logger.LogInformation(
            "User {UserId} ({Email}) registered successfully from IP={IP}, UA={UA}, At={TimeLocal}. Duration={Duration}ms",
            user.Id,
            user.Email,
            ipAddress,
            userAgent,
            _dateTimeProvider.NowLocal,
            (DateTime.UtcNow - start).TotalMilliseconds
        );

        var accessToken = _tokenService.GenerateAccessToken(
            user.Id,
            user.Email.Value,
            user.Role.ToString()
        );
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(
            user.Id,
            ipAddress,
            userAgent
        );

        return new AuthResponse(CreateUserInfo(user), accessToken, refreshToken);
    }

    /// <summary>
    /// Handles user login: verifies credentials, enforces lockout and password policies,
    /// manages failed attempts, and issues JWT tokens. All logs in Brussels local time.
    /// Strictly follows domain flow: only resets attempts on full login success, and handles
    /// warnings and lockouts exactly as in legacy code.
    /// </summary>
    public async Task<AuthResponse> LoginAsync(
        LoginRequest request,
        string ipAddress,
        string userAgent
    )
    {
        var nowUtc = _dateTimeProvider.UtcNow;
        var nowLocal = _dateTimeProvider.NowLocal;
        var email = request.Email;

        _logger.LogInformation(
            "Login attempt for {Email} from {IP}, UA {UA} at {TimeLocal}.",
            email,
            ipAddress,
            userAgent,
            nowLocal
        );

        // Always bypass cache on login for security
        var user =
            await _userRepository.FindByEmailAsync(email, bypassCache: true)
            ?? throw new NotFoundException("Invalid credentials.", email);

        // Update expiration policy (this may require a password change)
        user.EnforcePasswordExpirationPolicy(nowUtc);

        // 1. Lockout check — use helper (log, mail, throw)
        if (user.IsLockedOut(nowUtc))
            await HandleLockedAccountAsync(user);

        // 2. Password check — use helper (increments, log, mail, throw)
        if (!_passwordService.VerifyPassword(Password.Create(request.Password), user.PasswordHash))
            await HandleFailedPasswordAsync(user, nowUtc, nowLocal);

        // 3. Inactive account
        if (!user.IsActive)
        {
            _logger.LogWarning(
                "Login attempt on inactive account {Email} at {NowLocal} (IP: {IP}, UA: {UA}).",
                email,
                nowLocal,
                ipAddress,
                userAgent
            );
            throw new UnauthorizedAccessException("Account is inactive.");
        }

        // 4. Password must be changed (expired/admin)
        if (user.RequirePasswordChange)
        {
            _logger.LogInformation(
                "User {Email} requires password change before login (expired/admin) at {NowLocal}.",
                email,
                nowLocal
            );
            throw new UnauthorizedAccessException("Password change required.");
        }

        // 5. Success — reset counters, update last login, save
        user.RegisterSuccessfulLogin(nowUtc);
        await _userRepository.SaveChangesAsync();

        _logger.LogInformation(
            "User {UserId} ({Email}) successfully logged in from {IP} at {NowLocal}.",
            user.Id,
            user.Email,
            ipAddress,
            nowLocal
        );

        var accessToken = _tokenService.GenerateAccessToken(
            user.Id,
            user.Email.Value,
            user.Role.ToString()
        );
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(
            user.Id,
            ipAddress,
            userAgent
        );

        return new AuthResponse(CreateUserInfo(user), accessToken, refreshToken);
    }

    /// <summary>
    /// Refreshes the access and refresh tokens for a user if the refresh token is valid.
    /// Enforces password expiration before issuing new tokens.
    /// </summary>
    public async Task<AuthResponse> RefreshTokenAsync(
        string refreshToken,
        string ipAddress,
        string userAgent
    )
    {
        var start = DateTime.UtcNow;
        _logger.LogInformation(
            "RefreshToken START: IP={IP}, UA={UA}, At={TimeLocal}",
            ipAddress,
            userAgent,
            _dateTimeProvider.NowLocal
        );

        RefreshTokenValidationResult validation =
            await _tokenService.ValidateAndRevokeRefreshTokenAsync(refreshToken);
        if (!validation.IsValid || validation.UserId is null)
        {
            _logger.LogWarning(
                "Invalid/expired refresh token from IP={IP}, UA={UA} at {TimeLocal}",
                ipAddress,
                userAgent,
                _dateTimeProvider.NowLocal
            );
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");
        }

        var user =
            await _userRepository.FindByIdAsync(validation.UserId.Value)
            ?? throw new NotFoundException("User", validation.UserId.Value);

        user.EnforcePasswordExpirationPolicy(_dateTimeProvider.UtcNow);
        if (user.RequirePasswordChange)
        {
            _logger.LogInformation(
                "Token refresh denied for user {Email}, UserId={UserId}: password change required at {TimeLocal}",
                user.Email,
                user.Id,
                _dateTimeProvider.NowLocal
            );
            throw new UnauthorizedAccessException(
                "Password expired. Please reset your password to continue."
            );
        }

        var newAccessToken = _tokenService.GenerateAccessToken(
            user.Id,
            user.Email.Value,
            user.Role.ToString()
        );
        var newRefreshToken = await _tokenService.GenerateRefreshTokenAsync(
            user.Id,
            ipAddress,
            userAgent
        );

        _logger.LogInformation(
            "Token refreshed for user {UserId} ({Email}) from IP={IP}, UA={UA}, At={TimeLocal}, Duration={Duration}ms",
            user.Id,
            user.Email,
            ipAddress,
            userAgent,
            _dateTimeProvider.NowLocal,
            (DateTime.UtcNow - start).TotalMilliseconds
        );

        return new AuthResponse(CreateUserInfo(user), newAccessToken, newRefreshToken);
    }

    /// <summary>
    /// Logs out a user by revoking their refresh token.
    /// </summary>
    public async Task LogoutAsync(RefreshTokenDto refreshTokenDto)
    {
        var start = DateTime.UtcNow;
        await _tokenService.ValidateAndRevokeRefreshTokenAsync(refreshTokenDto.Token);
        _logger.LogInformation(
            "Logout: Refresh token revoked at {TimeLocal}. Duration={Duration}ms",
            _dateTimeProvider.NowLocal,
            (DateTime.UtcNow - start).TotalMilliseconds
        );
    }

    /// <summary>
    /// Ensures an email is not already used (throws otherwise).
    /// </summary>
    private async Task EnsureEmailNotUsedAsync(EmailAddress email)
    {
        var exists = await _userRepository.FindByEmailAsync(email);
        if (exists != null)
        {
            _logger.LogWarning(
                "Registration failed: Email {Email} already exists at {TimeLocal}.",
                email,
                _dateTimeProvider.NowLocal
            );
            throw new ValidationException(
                new Dictionary<string, string[]> { ["email"] = ["Email is already in use."] }
            );
        }
    }

    /// <summary>
    /// Handles account lockout on login attempt. Sends notification and throws.
    /// </summary>
    private async Task HandleLockedAccountAsync(User user)
    {
        var lockoutLocal = user.LockoutEnd!.Value;
        var formatted = _dateTimeProvider.ToBrusselsTimeString(lockoutLocal);

        await _emailSender.SendAccountLockedEmailAsync(
            user.Email.Value,
            $"{user.FirstName} {user.LastName}",
            lockoutLocal
        );

        _logger.LogWarning(
            "LOCKOUT: Account lockout notification sent to {Email}, UserId={UserId}, At={TimeLocal}. Locked until {LockedUntil}.",
            user.Email,
            user.Id,
            _dateTimeProvider.NowLocal,
            formatted
        );

        throw new UnauthorizedAccessException($"Account is locked. Try again at {formatted}.");
    }

    /// <summary>
    /// Handles logic for failed password attempts, lockout escalation, and user notifications.
    /// Throws with proper message depending on attempt count.
    /// </summary>
    private async Task HandleFailedPasswordAsync(User user, DateTime nowUtc, DateTime nowLocal)
    {
        var lockoutEndUtc = user.RegisterFailedLoginAttempt(nowUtc);
        await _userRepository.SaveChangesAsync();

        if (lockoutEndUtc.HasValue)
        {
            var lockoutLocal = _dateTimeProvider.ConvertToLocal(lockoutEndUtc.Value);
            var formatted = _dateTimeProvider.ToBrusselsTimeString(lockoutEndUtc.Value);

            await _emailSender.SendAccountLockedEmailAsync(
                user.Email.Value,
                $"{user.FirstName} {user.LastName}",
                lockoutLocal
            );

            _logger.LogError(
                "LOCKOUT: User {Email}, UserId={UserId} locked out at {LocalTime}. LockoutEnd={LockedUntil}, FailedAttempts={Count}",
                user.Email,
                user.Id,
                nowLocal,
                formatted,
                user.FailedLoginAttempts
            );

            throw new UnauthorizedAccessException($"Account is locked. Try again at {formatted}.");
        }

        if (user.WillBeLockedNextAttempt)
        {
            _logger.LogInformation(
                "SECURITY WARNING: User {Email}, UserId={UserId}: next failed attempt will lock account. ({LocalTime}, Attempts={Count})",
                user.Email,
                user.Id,
                nowLocal,
                user.FailedLoginAttempts
            );
            throw new UnauthorizedAccessException(
                "Invalid credentials. Warning: one more failed attempt will lock your account."
            );
        }

        _logger.LogWarning(
            "Invalid login attempt for {Email}, UserId={UserId} at {LocalTime}. FailedAttempts={Count}",
            user.Email,
            user.Id,
            nowLocal,
            user.FailedLoginAttempts
        );
        throw new UnauthorizedAccessException("Invalid credentials.");
    }

    /// <summary>
    /// Maps User entity to UserInfo DTO for AuthResponse.
    /// </summary>
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
