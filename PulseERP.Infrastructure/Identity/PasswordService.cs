using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using PulseERP.Application.Exceptions;
using PulseERP.Application.Interfaces;
using PulseERP.Domain.Interfaces.Repositories;
using PulseERP.Domain.Interfaces.Services;
using PulseERP.Domain.ValueObjects;

namespace PulseERP.Infrastructure.Identity;

public class PasswordService : IPasswordService
{
    private readonly IUserQueryRepository _userQuery;
    private readonly IUserCommandRepository _userCommand;
    private readonly IPasswordResetTokenRepository _resetRepo;
    private readonly ISmtpEmailService _emailService;
    private readonly IDateTimeProvider _time;
    private readonly ILogger<PasswordService> _logger;

    public PasswordService(
        IUserQueryRepository userQuery,
        IUserCommandRepository userCommand,
        IPasswordResetTokenRepository resetRepo,
        ISmtpEmailService emailService,
        IDateTimeProvider time,
        ILogger<PasswordService> logger
    )
    {
        _userQuery = userQuery;
        _userCommand = userCommand;
        _resetRepo = resetRepo;
        _emailService = emailService;
        _time = time;
        _logger = logger;
    }

    public string HashPassword(string plainPassword)
    {
        if (string.IsNullOrWhiteSpace(plainPassword))
            throw new ArgumentException("Password cannot be empty.", nameof(plainPassword));
        ValidatePasswordComplexity(plainPassword);
        return BCrypt.Net.BCrypt.HashPassword(plainPassword);
    }

    public bool VerifyPassword(string plainPassword, string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(plainPassword) || string.IsNullOrWhiteSpace(hashedPassword))
            return false;
        return BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword);
    }

    public async Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        _logger.LogInformation("Changing password for user {UserId}", userId);
        var user =
            await _userQuery.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException($"User {userId} not found.");
        if (!VerifyPassword(currentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Current password is incorrect.");

        user.UpdatePassword(HashPassword(newPassword));
        user.ClearPasswordResetRequirement();
        await _userCommand.UpdateAsync(user);
        await _userCommand.SaveChangesAsync();
    }

    public async Task ResetPasswordAsync(Guid userId, string newPassword)
    {
        _logger.LogInformation("Resetting password for user {UserId}", userId);
        var user =
            await _userQuery.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException($"User {userId} not found.");

        user.UpdatePassword(HashPassword(newPassword));
        user.ClearPasswordResetRequirement();
        await _userCommand.UpdateAsync(user);
        await _userCommand.SaveChangesAsync();
    }

    public async Task ForcePasswordResetAsync(string email, string newPassword)
    {
        _logger.LogInformation("Force resetting password for email {Email}", email);
        var user =
            await _userQuery.GetByEmailAsync(Email.Create(email))
            ?? throw new KeyNotFoundException($"User with email {email} not found.");

        user.UpdatePassword(HashPassword(newPassword));
        user.RequirePasswordReset();
        await _userCommand.UpdateAsync(user);
        await _userCommand.SaveChangesAsync();
    }

    public async Task RequestPasswordResetAsync(string email)
    {
        _logger.LogInformation("Password reset requested for email {Email}", email);
        var user = await _userQuery.GetByEmailAsync(Email.Create(email));
        if (user == null)
            return;

        var token = Guid.NewGuid().ToString("N");
        await _resetRepo.StoreAsync(user.Id, token, _time.UtcNow.AddHours(1));
        await _userCommand.SaveChangesAsync();

        await _emailService.SendEmailAsync(user.Email.ToString(), "Password Reset", token);
    }

    public async Task ResetPasswordWithTokenAsync(string resetToken, string newPassword)
    {
        _logger.LogInformation("Resetting password with token");
        var entity =
            await _resetRepo.GetActiveByTokenAsync(resetToken)
            ?? throw new UnauthorizedAccessException("Invalid or expired password reset token.");

        var user =
            await _userQuery.GetByIdAsync(entity.UserId)
            ?? throw new KeyNotFoundException($"User {entity.UserId} not found.");

        user.UpdatePassword(HashPassword(newPassword));
        user.ClearPasswordResetRequirement();

        await _resetRepo.MarkAsUsedAsync(resetToken);
        await _userCommand.UpdateAsync(user);
        await _userCommand.SaveChangesAsync();
    }

    private void ValidatePasswordComplexity(string password)
    {
        var errors = new List<string>();

        if (password.Length < 8)
            errors.Add("Password must be at least 8 characters long.");
        if (password.Length > 100)
            errors.Add("Password must be at most 100 characters long.");
        if (!Regex.IsMatch(password, @"[A-Z]"))
            errors.Add("Password must contain at least one uppercase letter.");
        if (!Regex.IsMatch(password, @"[a-z]"))
            errors.Add("Password must contain at least one lowercase letter.");
        if (!Regex.IsMatch(password, @"\d"))
            errors.Add("Password must contain at least one digit.");
        if (!Regex.IsMatch(password, @"[^a-zA-Z0-9]"))
            errors.Add("Password must contain at least one special character.");

        if (errors.Any())
            throw new ValidationException(
                new Dictionary<string, string[]> { ["password"] = errors.ToArray() }
            );
    }
}
