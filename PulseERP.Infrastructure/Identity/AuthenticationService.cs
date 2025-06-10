using AutoMapper;
using Microsoft.Extensions.Logging;
using PulseERP.Abstractions.Contracts.Repositories;
using PulseERP.Abstractions.Security.DTOs;
using PulseERP.Abstractions.Security.Interfaces;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Errors;
using PulseERP.Domain.Extensions;
using PulseERP.Domain.Interfaces;
using PulseERP.Domain.VO;

namespace PulseERP.Infrastructure.Identity
{
    public class AuthenticationService(
        IPasswordService passwordService,
        ITokenService tokenService,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<AuthenticationService> logger,
        IDateTimeProvider dateTimeProvider,
        IEmailSenderService emailService)
        : IAuthenticationService
    {
        #region Login

        public async Task<AuthResponse> LoginAsync(LoginRequest request, string ipAddress, string userAgent)
        {
            var nowUtc = dateTimeProvider.UtcNow;
            var nowLocal = dateTimeProvider.NowLocal;

            var emailVo = new EmailAddress(request.Email);

            // 2. Retrieve user, bypass cache for security
            var user = await userRepository.FindByEmailAsync(emailVo, bypassCache: true)
                       ?? throw new NotFoundException("Invalid credentials.", emailVo.Value);

            // 3. Check if account is locked
            if (user.IsLockedOut(nowLocal))
                await HandleLockedAccountAsync(user);

            if (user.PasswordHash is null)
                throw new BadRequestException("You needs to activate your account.");

            // 4. Check password expiration / force reset
            user.CheckPasswordExpiration(nowUtc);

            if (user.RequirePasswordChange)
                throw new UnauthorizedAccessException("Password change required.");

            // 5. Verify password
            var passwordValid = passwordService.VerifyPassword(request.Password, user.PasswordHash);

            if (!passwordValid)
                await HandleFailedPasswordAsync(user, nowUtc, nowLocal, ipAddress, userAgent);

            // 6. Check if account is active
            if (!user.IsActive)
                throw new UnauthorizedAccessException("Account is not active.");

            var accessToken = tokenService.GenerateAccessToken(
                user.Id,
                user.Email.Value,
                user.Role.Value
            );
            var refreshToken = await tokenService.GenerateRefreshTokenAsync(
                user.Id,
                ipAddress,
                userAgent
            );

            user.RegisterSuccessfulLogin(nowUtc);
            user.ClearDomainEvents();

            await unitOfWork.SaveChangesAndDispatchEventsAsync();

            var userInfo = mapper.Map<UserInfo>(user);
            return new AuthResponse(userInfo, accessToken, refreshToken);
        }

        #endregion

        #region Activate Account

        public async Task ActivateAccountAsync(ActivateAccountRequest request)
        {
            // 1. Validate passwords match
            if (request.Password != request.ConfirmPassword)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { nameof(request.ConfirmPassword), ["Passwords do not match."] }
                });
            }

            // 2. Validate token and get UserId
            var validationResult = await tokenService.ValidateAndRevokeActivationTokenAsync(request.Token);

            if (!validationResult.IsValid || validationResult.UserId == null)
            {
                throw new UnauthorizedAccessException("Invalid or expired activation token.");
            }

            var userId = validationResult.UserId.Value;

            // 3. Load user
            var user = await userRepository.FindByIdAsync(userId, bypassCache: true)
                       ?? throw new NotFoundException("User", userId);

            user.ClearDomainEvents();

            // 4. Check if password already set
            if (!string.IsNullOrWhiteSpace(user.PasswordHash) && user.IsActive)
            {
                throw new InvalidOperationException("Account is already activated.");
            }

            // 5. Create Password VO and hash
            var passwordVo = new Password(request.Password);
            var passwordHash = passwordVo.HashedValue;

            // 6. Set password
            user.UpdatePassword(passwordHash);

            // 7. Mark as Active 
            user.MarkAsActivate();

            // 8. Save
            await userRepository.UpdateAsync(user);

            await unitOfWork.SaveChangesAndDispatchEventsAsync();
        }

        #endregion

        #region Refresh Token

        public async Task<AuthResponse> RefreshTokenAsync(
            string refreshToken,
            string ipAddress,
            string userAgent
        )
        {
            // 1. Validate and revoke old refresh token
            var validation = await tokenService.ValidateAndRevokeRefreshTokenAsync(refreshToken);
            if (!validation.IsValid || validation.UserId is null)
            {
                throw new UnauthorizedAccessException("Invalid or expired refresh token.");
            }

            var userId = validation.UserId.Value;
            var user =
                await userRepository.FindByIdAsync(userId, bypassCache: false)
                ?? throw new NotFoundException("User", userId);

            // 2. Enforce password expiration before issuing new tokens
            user.CheckPasswordExpiration(dateTimeProvider.UtcNow);
            if (user.RequirePasswordChange)
            {
                throw new UnauthorizedAccessException(
                    "Password expired. Please reset your password to continue."
                );
            }

            // 3. Generate fresh tokens
            var newAccessToken = tokenService.GenerateAccessToken(
                user.Id,
                user.Email.Value,
                user.Role.Value
            );

            var newRefreshToken = await tokenService.GenerateRefreshTokenAsync(
                user.Id,
                ipAddress,
                userAgent
            );

            var userInfo = mapper.Map<UserInfo>(user);

            return new AuthResponse(userInfo, newAccessToken, newRefreshToken);
        }

        #endregion

        #region Logout

        public async Task LogoutAsync(RefreshTokenDto refreshTokenDto)
        {
            var startUtc = DateTime.UtcNow;
            var startLocal = dateTimeProvider.NowLocal;

            await tokenService.ValidateAndRevokeRefreshTokenAsync(refreshTokenDto.Token);

            var durationMs = (DateTime.UtcNow - startUtc).TotalMilliseconds;
            logger.LogInformation(
                "LogoutAsync: Refresh token revoked at {TimeLocal}. Duration={Duration}ms",
                startLocal,
                durationMs
            );
        }

        #endregion

        #region Password Management

        public async Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            var nowUtc = dateTimeProvider.UtcNow;

            var user = await userRepository.FindByIdAsync(userId, bypassCache: true)
                       ?? throw new NotFoundException("User", userId);

            var errors = new Dictionary<string, string[]>();

            // 1. Verify current password
            var isCurrentValid = user.PasswordHash != null && passwordService.VerifyPassword(currentPassword,
                user.PasswordHash);
            if (!isCurrentValid)
            {
                user.RegisterFailedLogin(nowUtc);
                await userRepository.UpdateAsync(user);
                user.ClearDomainEvents();

                await unitOfWork.SaveChangesAndDispatchEventsAsync();

                errors.Add(nameof(currentPassword), ["Current password is incorrect."]);
                throw new ValidationException(errors);
            }

            // 2. Validate new password
            var newPasswordVo = ValueObjectExtensions.TryCreateValueObject(
                () => new Password(newPassword),
                nameof(newPassword),
                errors
            );

            // 3. Update password on User
            user.UpdatePassword(passwordService.HashPassword(newPasswordVo.HashedValue));
            // enqueues UserPasswordChangedEvent

            await userRepository.UpdateAsync(user);
            await unitOfWork.SaveChangesAndDispatchEventsAsync();
        }


        public async Task ForceResetPasswordAsync(Guid userId)
        {
            var user =
                await userRepository.FindByIdAsync(userId, bypassCache: true)
                ?? throw new NotFoundException("User", userId);

            user.ForcePasswordReset(); // enqueues UserPasswordResetForcedEvent
            await userRepository.UpdateAsync(user);
            await userRepository.SaveChangesAsync();
        }

        #endregion

        #region Internal Helpers

        private Task HandleLockedAccountAsync(User user)
        {
            var lockoutEndUtc = user.LockoutEnd!.Value;
            var formatted = dateTimeProvider.ToBrusselsTimeString(lockoutEndUtc);
            throw new UnauthorizedAccessException($"Account is locked. Try again at {formatted}.");

        }

        private async Task HandleFailedPasswordAsync(
            User user,
            DateTime nowUtc,
            DateTime nowLocal = default,
            string? ipAddress = null,
            string? userAgent = null
        )
        {
            // Increment failed count and possibly set lockout
            var lockoutEndUtc = user.RegisterFailedLogin(nowUtc);
            // enqueues UserLockedOutEvent if threshold reached
            await userRepository.SaveChangesAsync();

            if (lockoutEndUtc.HasValue)
            {
                // Send lockout email
                var lockoutEndLocal = dateTimeProvider.ConvertToLocal(lockoutEndUtc.Value);
                var formatted = dateTimeProvider.ToBrusselsTimeString(lockoutEndUtc.Value);

                throw new UnauthorizedAccessException(
                    $"Account is locked. Try again at {formatted}."
                );
            }

            if (user.WillBeLockedOutAfterNextFailure)
            {
                throw new UnauthorizedAccessException(
                    "Invalid credentials. Warning: one more failed attempt will lock your account."
                );
            }

            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        #endregion
    }
}