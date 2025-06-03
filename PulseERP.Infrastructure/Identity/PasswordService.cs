using Microsoft.Extensions.Logging;
using PulseERP.Abstractions.Security.Interfaces;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Enums.Token;
using PulseERP.Domain.Errors;
using PulseERP.Domain.Interfaces;
using PulseERP.Domain.Security.Interfaces;
using PulseERP.Domain.ValueObjects.Adresses;
using PulseERP.Domain.ValueObjects.Passwords;

namespace PulseERP.Infrastructure.Identity;

/// <summary>
/// Handles secure password operations (hash, verify, change, reset, and password reset tokens).
/// Uses Value Objects for all input. All logs are context-rich and safe.
/// </summary>
public class PasswordService : IPasswordService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenRepository _tokenRepository;
    private readonly IEmailSenderService _emailService;
    private readonly ITokenGeneratorService _tokenGenerator;
    private readonly ITokenHasherService _tokenHasher;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<PasswordService> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="PasswordService"/>.
    /// </summary>
    public PasswordService(
        IUserRepository userRepository,
        ITokenRepository tokenRepository,
        IEmailSenderService emailService,
        IDateTimeProvider dateTimeProvider,
        ITokenGeneratorService tokenGenerator,
        ITokenHasherService tokenHasher,
        ILogger<PasswordService> logger
    )
    {
        _userRepository = userRepository;
        _tokenRepository = tokenRepository;
        _emailService = emailService;
        _dateTimeProvider = dateTimeProvider;
        _tokenGenerator = tokenGenerator;
        _tokenHasher = tokenHasher;
        _logger = logger;
    }

    /// <summary>
    /// Hashes the provided password using a secure algorithm.
    /// </summary>
    public string HashPassword(string password)
    {
        _logger.LogDebug("Hashing password at {TimeLocal}.", _dateTimeProvider.NowLocal);
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    /// <summary>
    /// Verifies the provided password against a hashed password.
    /// </summary>
    public bool VerifyPassword(Password password, string hashedPassword)
    {
        _logger.LogDebug("Verifying password hash at {TimeLocal}.", _dateTimeProvider.NowLocal);
        return BCrypt.Net.BCrypt.Verify(password.Value, hashedPassword);
    }

    /// <summary>
    /// Changes a user's password after verifying the current one.
    /// Throws if the current password does not match.
    /// </summary>
    public async Task ChangePasswordAsync(
        Guid userId,
        Password currentPassword,
        Password newPassword
    )
    {
        var user =
            await _userRepository.FindByIdAsync(userId)
            ?? throw new DomainException("User not found.");

        if (!VerifyPassword(currentPassword, user.PasswordHash))
        {
            _logger.LogWarning(
                "Failed password change: current password incorrect for user {UserId} at {TimeLocal}.",
                userId,
                _dateTimeProvider.NowLocal
            );
            throw new DomainException("Current password is incorrect.");
        }

        await UpdateUserPasswordAsync(user, newPassword, requirePasswordChange: false);
        _logger.LogInformation(
            "Password changed for user {UserId} at {TimeLocal}.",
            userId,
            _dateTimeProvider.NowLocal
        );
    }

    /// <summary>
    /// Directly resets a user's password (admin or internal process).
    /// Does not require the old password.
    /// </summary>
    public async Task ResetPasswordAsync(Guid userId, Password newPassword)
    {
        var user =
            await _userRepository.FindByIdAsync(userId)
            ?? throw new DomainException("User not found.");

        await UpdateUserPasswordAsync(user, newPassword, requirePasswordChange: false);
        _logger.LogInformation(
            "Password reset for user {UserId} at {TimeLocal}.",
            userId,
            _dateTimeProvider.NowLocal
        );
    }

    /// <summary>
    /// Forces a user's password to be reset and requires them to change it at next login.
    /// </summary>
    public async Task ForcePasswordResetAsync(EmailAddress email, Password newPassword)
    {
        var user =
            await _userRepository.FindByEmailAsync(email.Value)
            ?? throw new DomainException("Unknown email.");

        await UpdateUserPasswordAsync(user, newPassword, requirePasswordChange: true);

        _logger.LogWarning(
            "Password forcibly reset by admin for user {UserId} ({Email}) at {TimeLocal}.",
            user.Id,
            email.Value,
            _dateTimeProvider.NowLocal
        );
    }

    /// <summary>
    /// Initiates a password reset flow by generating a reset token and sending it via email.
    /// </summary>/// <summary>
    /// Initiates a password reset flow: generates a reset token, stores it, and sends an email with the reset link.
    /// </summary>
    public async Task RequestPasswordResetAsync(EmailAddress email)
    {
        var user =
            await _userRepository.FindByEmailAsync(email)
            ?? throw new DomainException("Unknown email.");

        var rawToken = _tokenGenerator.GenerateToken();
        var tokenHash = _tokenHasher.Hash(rawToken);
        var expiresUtc = _dateTimeProvider.UtcNow.AddHours(1);

        var resetTokenEntity = RefreshToken.Create(
            _dateTimeProvider,
            user.Id,
            tokenHash,
            TokenType.PasswordReset,
            expiresUtc
        );

        await _tokenRepository.AddAsync(resetTokenEntity);

        // Génère l'URL de reset (à adapter selon ta conf front)
        var resetUrl =
            $"https://app.pulseerp.com/reset-password?token={Uri.EscapeDataString(rawToken)}";

        await _emailService.SendPasswordResetEmailAsync(
            user.Email.Value,
            $"{user.FirstName} {user.LastName}",
            resetUrl,
            expiresUtc
        );

        _logger.LogInformation(
            "Password reset token generated and sent to {Email} at {TimeLocal}.",
            email.Value,
            _dateTimeProvider.NowLocal
        );
    }

    /// <summary>
    /// Resets a user's password using a valid password reset token.
    /// </summary>
    public async Task ResetPasswordWithTokenAsync(string resetToken, Password newPassword)
    {
        var tokenHash = _tokenHasher.Hash(resetToken);
        var entity = await _tokenRepository.GetByTokenAndTypeAsync(
            tokenHash,
            TokenType.PasswordReset
        );

        if (entity == null)
        {
            _logger.LogWarning(
                "Password reset with token failed (invalid or expired) at {TimeLocal}.",
                _dateTimeProvider.NowLocal
            );
            throw new DomainException("Invalid or expired reset token.");
        }

        var user =
            await _userRepository.FindByIdAsync(entity.UserId)
            ?? throw new DomainException("User not found.");

        entity.Revoke(_dateTimeProvider.UtcNow);
        await _tokenRepository.RevokeAllByUserIdAndTypeAsync(
            entity.UserId,
            TokenType.PasswordReset
        );

        await UpdateUserPasswordAsync(user, newPassword, requirePasswordChange: false);

        _logger.LogInformation(
            "Password reset by token for user {UserId} at {TimeLocal}.",
            entity.UserId,
            _dateTimeProvider.NowLocal
        );
    }

    /// <summary>
    /// Updates a user's password, handles flag for requiring password change.
    /// </summary>
    /// <param name="user">User entity to update.</param>
    /// <param name="newPassword">New password (Value Object).</param>
    /// <param name="requirePasswordChange">If true, will require change on next login.</param>
    private async Task UpdateUserPasswordAsync(
        User user,
        Password newPassword,
        bool requirePasswordChange
    )
    {
        user.UpdatePassword(HashPassword(newPassword.Value));
        if (requirePasswordChange)
            user.RequirePasswordReset();
        await _userRepository.UpdateAsync(user);
    }
}
