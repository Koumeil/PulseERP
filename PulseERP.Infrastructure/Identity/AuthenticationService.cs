using Microsoft.Extensions.Logging;
using PulseERP.Abstractions.Contracts.Repositories;
using PulseERP.Abstractions.Security.DTOs;
using PulseERP.Abstractions.Security.Interfaces;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Errors;
using PulseERP.Domain.Interfaces;
using PulseERP.Domain.VO;

namespace PulseERP.Infrastructure.Identity
{
    /// <summary>
    /// Handles user registration, login (authentication), token refresh, and logout.
    /// All logs are in Brussels local time. Password expiration is enforced via User.RequirePasswordChange.
    /// Every relevant User method is invoked so that business rules remain in the domain.
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IPasswordService _passwordService;
        private readonly ITokenService _tokenService;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IEmailSenderService _emailSender;

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

        #region Registration

        /// <summary>
        /// Registers a new user, enforcing unique email, sending welcome email, and returning AuthResponse.
        /// Invokes User constructor and, if applicable, User.UpdatePhone().
        /// </summary>
        /// <param name="request">Contains FirstName, LastName, Email, Password, optional Phone.</param>
        /// <param name="ipAddress">Client IP address for logging.</param>
        /// <param name="userAgent">Client User-Agent for logging.</param>
        /// <returns>An AuthResponse with new UserInfo, AccessToken, and RefreshToken.</returns>
        /// <exception cref="ValidationException">Thrown if any input is invalid or email is duplicate.</exception>
        public async Task<AuthResponse> RegisterAsync(
            RegisterRequest request,
            string ipAddress,
            string userAgent
        )
        {
            var startUtc = DateTime.UtcNow;
            var startLocal = _dateTimeProvider.NowLocal;
            _logger.LogInformation(
                "RegisterAsync START: Email={Email}, IP={IP}, UA={UA}, At={TimeLocal}",
                request.Email,
                ipAddress,
                userAgent,
                startLocal
            );

            var errors = new Dictionary<string, string[]>();

            // 1. Validate Email
            EmailAddress emailVO = null!;
            try
            {
                emailVO = new EmailAddress(request.Email);
            }
            catch (DomainValidationException ex)
            {
                errors.Add(nameof(request.Email), new[] { ex.Message });
                _logger.LogWarning("RegisterAsync: Invalid email format: {Error}", ex.Message);
            }

            // 2. Ensure email is not already used
            if (errors.Count == 0)
            {
                var existingUser = await _userRepository.FindByEmailAsync(emailVO);
                if (existingUser != null)
                {
                    errors.Add(nameof(request.Email), new[] { "Email is already in use." });
                    _logger.LogWarning(
                        "RegisterAsync: Duplicate email attempt: {Email}",
                        request.Email
                    );
                }
            }

            // 3. Validate phone via Phone VO
            Phone phoneNumberVO = null!;
            if (errors.Count == 0)
            {
                try
                {
                    phoneNumberVO = new Phone(request.PhoneNumber);
                }
                catch (DomainValidationException ex)
                {
                    errors.Add(nameof(request.PhoneNumber), [ex.Message]);
                    _logger.LogWarning(
                        "CreateAsync: PhoneNumber validation failed: {Error}",
                        ex.Message
                    );
                }
            }

            // 4. Validate password via Password VO
            Password passwordVO = null!;
            if (errors.Count == 0)
            {
                try
                {
                    passwordVO = new Password(request.Password);
                }
                catch (DomainValidationException ex)
                {
                    errors.Add(nameof(request.Password), new[] { ex.Message });
                    _logger.LogWarning(
                        "CreateAsync: Password validation failed: {Error}",
                        ex.Message
                    );
                }
            }

            if (errors.Count > 0)
            {
                _logger.LogWarning("CreateAsync FAILED: Validation errors {@Errors}", errors);
                throw new ValidationException(errors);
            }

            // 5. Hash the password
            string passwordHash = passwordVO.HashedValue;

            // 6. Construct User aggregate (constructor enqueues UserCreatedEvent)
            var user = new User(
                request.FirstName,
                request.LastName,
                emailVO,
                phoneNumberVO,
                passwordHash
            );

            // 6. Optionally set phone
            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                try
                {
                    var phoneVO = new Phone(request.PhoneNumber);
                    user.UpdatePhone(phoneVO); // enqueues UserPhoneChangedEvent
                }
                catch (DomainValidationException ex)
                {
                    // Treat as validation error
                    errors.Add(nameof(request.PhoneNumber), new[] { ex.Message });
                    _logger.LogWarning(
                        "RegisterAsync: Phone validation failed: {Error}",
                        ex.Message
                    );
                }
            }

            if (errors.Count > 0)
            {
                _logger.LogWarning("RegisterAsync FAILED: Validation errors {@Errors}", errors);
                throw new ValidationException(errors);
            }

            // 7. Persist
            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            // 8. Send welcome email
            var userFullName = $"{user.FirstName} {user.LastName}";
            var loginUrl =
                $"https://app.pulseERP.com/login?email={Uri.EscapeDataString(request.Email)}";

            await _emailSender.SendWelcomeEmailAsync(
                toEmail: user.Email.Value,
                userFullName: userFullName,
                loginUrl: loginUrl
            );

            var durationMs = (DateTime.UtcNow - startUtc).TotalMilliseconds;
            _logger.LogInformation(
                "RegisterAsync SUCCESS: UserId={UserId}, Email={Email}, IP={IP}, UA={UA}, At={TimeLocal}, Duration={Duration}ms",
                user.Id,
                user.Email.Value,
                ipAddress,
                userAgent,
                _dateTimeProvider.NowLocal,
                durationMs
            );

            // 9. Generate tokens
            var accessToken = _tokenService.GenerateAccessToken(
                user.Id,
                user.Email.Value,
                user.Role.Value
            );

            var refreshToken = await _tokenService.GenerateRefreshTokenAsync(
                user.Id,
                ipAddress,
                userAgent
            );

            var userInfo = new UserInfo(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email.Value,
                user.Role.Value,
                user.PhoneNumber.Value
            );

            _logger.LogDebug(
                "RegisterAsync END: Returning AuthResponse for UserId={UserId} at {TimeLocal}",
                user.Id,
                _dateTimeProvider.NowLocal
            );
            return new AuthResponse(userInfo, accessToken, refreshToken);
        }

        #endregion

        #region Login

        /// <summary>
        /// Handles user login: verifies credentials, enforces lockout and password policies,
        /// manages FailedLoginAttempts, and issues JWT tokens.
        /// Invokes domain methods such as CheckPasswordExpiration, IsLockedOut, RegisterFailedLogin, and RegisterSuccessfulLogin.
        /// </summary>
        /// <param name="request">LoginRequest containing Email and Password.</param>
        /// <param name="ipAddress">Client IP address for logging.</param>
        /// <param name="userAgent">Client User-Agent for logging.</param>
        /// <returns>An AuthResponse with user info and fresh tokens on success.</returns>
        /// <exception cref="NotFoundException">Thrown if no user is found with the given email.</exception>
        /// <exception cref="UnauthorizedAccessException">
        /// Thrown if account is locked, inactive, credentials are invalid, or password expired.
        /// </exception>
        /// <exception cref="ValidationException">Thrown if email format is invalid.</exception>
        public async Task<AuthResponse> LoginAsync(
            LoginRequest request,
            string ipAddress,
            string userAgent
        )
        {
            var nowUtc = _dateTimeProvider.UtcNow;
            var nowLocal = _dateTimeProvider.NowLocal;

            // 1. Validate email format via VO
            EmailAddress emailVO = null!;
            try
            {
                emailVO = new EmailAddress(request.Email);
            }
            catch (DomainValidationException ex)
            {
                _logger.LogWarning("LoginAsync: Invalid email format: {Error}", ex.Message);
                throw new ValidationException(
                    new Dictionary<string, string[]>
                    {
                        { nameof(request.Email), new[] { ex.Message } },
                    }
                );
            }

            _logger.LogInformation(
                "LoginAsync START for {Email} from IP={IP}, UA={UA} at {TimeLocal}.",
                emailVO.Value,
                ipAddress,
                userAgent,
                nowLocal
            );

            // 2. Retrieve user, bypass cache for security
            var user =
                await _userRepository.FindByEmailAsync(emailVO, bypassCache: true)
                ?? throw new NotFoundException("Invalid credentials.", emailVO.Value);

            // 3. Check if account is locked
            if (user.IsLockedOut(nowUtc))
            {
                await HandleLockedAccountAsync(user);
            }

            // 4. Verify password
            bool passwordValid = _passwordService.VerifyPassword(
                request.Password,
                user.PasswordHash
            );
            if (!passwordValid)
            {
                await HandleFailedPasswordAsync(user, nowUtc, nowLocal, ipAddress, userAgent);
            }

            // 5. Check if user is inactive
            if (!user.IsActive)
            {
                _logger.LogWarning(
                    "LoginAsync: Attempt on inactive account {Email} at {TimeLocal} (IP={IP}, UA={UA}).",
                    emailVO.Value,
                    nowLocal,
                    ipAddress,
                    userAgent
                );
                throw new UnauthorizedAccessException("Account is inactive.");
            }

            // 6. Check password expiration / force reset
            user.CheckPasswordExpiration(nowUtc);
            if (user.RequirePasswordChange)
            {
                _logger.LogInformation(
                    "LoginAsync: Password change required for {Email} at {TimeLocal}.",
                    emailVO.Value,
                    nowLocal
                );
                throw new UnauthorizedAccessException("Password change required.");
            }

            // 7. Successful login: reset failed attempts & lockout, update last login
            user.RegisterSuccessfulLogin(nowUtc);
            await _userRepository.SaveChangesAsync();

            _logger.LogInformation(
                "LoginAsync SUCCESS: UserId={UserId}, Email={Email} logged in from IP={IP} at {TimeLocal}.",
                user.Id,
                user.Email.Value,
                ipAddress,
                nowLocal
            );

            // 8. Generate tokens
            var accessToken = _tokenService.GenerateAccessToken(
                user.Id,
                user.Email.Value,
                user.Role.Value
            );

            var refreshToken = await _tokenService.GenerateRefreshTokenAsync(
                user.Id,
                ipAddress,
                userAgent
            );

            var userInfo = new UserInfo(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email.Value,
                user.Role.Value,
                user.PhoneNumber.Value
            );

            _logger.LogDebug(
                "LoginAsync END: Returning AuthResponse for UserId={UserId} at {TimeLocal}",
                user.Id,
                _dateTimeProvider.NowLocal
            );
            return new AuthResponse(userInfo, accessToken, refreshToken);
        }

        #endregion

        #region Refresh Token

        /// <summary>
        /// Refreshes both the access token and the refresh token if the provided refresh token is valid.
        /// Also enforces that the user’s password is not expired before issuing new tokens.
        /// Invokes User.CheckPasswordExpiration().
        /// </summary>
        /// <param name="refreshToken">Existing refresh token to validate and revoke.</param>
        /// <param name="ipAddress">Client IP address for logging.</param>
        /// <param name="userAgent">Client User-Agent for logging.</param>
        /// <returns>A new AuthResponse with fresh tokens.</returns>
        /// <exception cref="UnauthorizedAccessException">
        /// Thrown if the refresh token is invalid/expired or the user’s password has expired.
        /// </exception>
        public async Task<AuthResponse> RefreshTokenAsync(
            string refreshToken,
            string ipAddress,
            string userAgent
        )
        {
            var startUtc = DateTime.UtcNow;
            var startLocal = _dateTimeProvider.NowLocal;
            _logger.LogInformation(
                "RefreshTokenAsync START: IP={IP}, UA={UA}, At={TimeLocal}",
                ipAddress,
                userAgent,
                startLocal
            );

            // 1. Validate and revoke old refresh token
            var validation = await _tokenService.ValidateAndRevokeRefreshTokenAsync(refreshToken);
            if (!validation.IsValid || validation.UserId is null)
            {
                _logger.LogWarning(
                    "RefreshTokenAsync: Invalid or expired refresh token. IP={IP}, UA={UA}, At={TimeLocal}",
                    ipAddress,
                    userAgent,
                    _dateTimeProvider.NowLocal
                );
                throw new UnauthorizedAccessException("Invalid or expired refresh token.");
            }

            var userId = validation.UserId.Value;
            var user =
                await _userRepository.FindByIdAsync(userId, bypassCache: false)
                ?? throw new NotFoundException("User", userId);

            // 2. Enforce password expiration before issuing new tokens
            user.CheckPasswordExpiration(_dateTimeProvider.UtcNow);
            if (user.RequirePasswordChange)
            {
                _logger.LogInformation(
                    "RefreshTokenAsync: Denied for UserId={UserId} ({Email}): password expired at {TimeLocal}",
                    user.Id,
                    user.Email.Value,
                    _dateTimeProvider.NowLocal
                );
                throw new UnauthorizedAccessException(
                    "Password expired. Please reset your password to continue."
                );
            }

            // 3. Generate fresh tokens
            var newAccessToken = _tokenService.GenerateAccessToken(
                user.Id,
                user.Email.Value,
                user.Role.Value
            );

            var newRefreshToken = await _tokenService.GenerateRefreshTokenAsync(
                user.Id,
                ipAddress,
                userAgent
            );

            var durationMs = (DateTime.UtcNow - startUtc).TotalMilliseconds;
            _logger.LogInformation(
                "RefreshTokenAsync SUCCESS: UserId={UserId}, Email={Email}, IP={IP}, UA={UA}, At={TimeLocal}, Duration={Duration}ms",
                user.Id,
                user.Email.Value,
                ipAddress,
                userAgent,
                _dateTimeProvider.NowLocal,
                durationMs
            );

            var userInfo = new UserInfo(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email.Value,
                user.Role.Value,
                user.PhoneNumber.Value
            );

            _logger.LogDebug(
                "RefreshTokenAsync END: Returning AuthResponse for UserId={UserId} at {TimeLocal}",
                user.Id,
                _dateTimeProvider.NowLocal
            );
            return new AuthResponse(userInfo, newAccessToken, newRefreshToken);
        }

        #endregion

        #region Logout

        /// <summary>
        /// Logs out a user by revoking their refresh token.
        /// No exceptions are thrown if the token is already invalid.
        /// </summary>
        /// <param name="refreshTokenDto">DTO containing the refresh token to revoke.</param>
        public async Task LogoutAsync(RefreshTokenDto refreshTokenDto)
        {
            var startUtc = DateTime.UtcNow;
            var startLocal = _dateTimeProvider.NowLocal;

            await _tokenService.ValidateAndRevokeRefreshTokenAsync(refreshTokenDto.Token);

            var durationMs = (DateTime.UtcNow - startUtc).TotalMilliseconds;
            _logger.LogInformation(
                "LogoutAsync: Refresh token revoked at {TimeLocal}. Duration={Duration}ms",
                startLocal,
                durationMs
            );
        }

        #endregion

        #region Password Management

        /// <summary>
        /// Changes an existing user’s password, given the correct current password,
        /// and resets expiration. Invokes User.UpdatePassword() on success.
        /// On incorrect current password, calls User.RegisterFailedLogin().
        /// </summary>
        /// <param name="userId">Unique identifier of the user whose password is changed.</param>
        /// <param name="currentPassword">Current plaintext password.</param>
        /// <param name="newPassword">New plaintext password.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="NotFoundException">Thrown if no user exists with the given ID.</exception>
        /// <exception cref="ValidationException">Thrown if the current password is incorrect or new password is invalid.</exception>
        public async Task ChangePasswordAsync(
            Guid userId,
            string currentPassword,
            string newPassword
        )
        {
            var nowUtc = _dateTimeProvider.UtcNow;
            var nowLocal = _dateTimeProvider.NowLocal;

            _logger.LogDebug(
                "ChangePasswordAsync START: UserId={UserId}, At={TimeLocal}",
                userId,
                nowLocal
            );

            var user =
                await _userRepository.FindByIdAsync(userId, bypassCache: true)
                ?? throw new NotFoundException("User", userId);

            // 1. Verify current password
            var isCurrentValid = _passwordService.VerifyPassword(
                currentPassword,
                user.PasswordHash
            );
            if (!isCurrentValid)
            {
                user.RegisterFailedLogin(nowUtc);
                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();

                _logger.LogWarning(
                    "ChangePasswordAsync: Incorrect current password for UserId={UserId} at {TimeLocal}. FailedAttempts={FailedAttempts}",
                    userId,
                    nowLocal,
                    user.FailedLoginAttempts
                );

                throw new ValidationException(
                    new Dictionary<string, string[]>
                    {
                        { nameof(currentPassword), new[] { "Current password is incorrect." } },
                    }
                );
            }

            // 2. Validate new password
            Password newPasswordVO = null!;
            try
            {
                newPasswordVO = new Password(newPassword);
            }
            catch (DomainValidationException ex)
            {
                _logger.LogWarning(
                    "ChangePasswordAsync: New password validation failed for UserId={UserId}: {Error}",
                    userId,
                    ex.Message
                );
                throw new ValidationException(
                    new Dictionary<string, string[]>
                    {
                        { nameof(newPassword), new[] { ex.Message } },
                    }
                );
            }

            // 3. Update password on User
            user.UpdatePassword(_passwordService.HashPassword(newPasswordVO.HashedValue));
            // enqueues UserPasswordChangedEvent
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            _logger.LogInformation(
                "ChangePasswordAsync SUCCESS: UserId={UserId}, At={TimeLocal}",
                userId,
                _dateTimeProvider.NowLocal
            );
            _logger.LogDebug(
                "ChangePasswordAsync END: Password changed for UserId={UserId} at {TimeLocal}",
                userId,
                _dateTimeProvider.NowLocal
            );
        }

        /// <summary>
        /// Forces an immediate password reset on the next login for a user, by calling User.ForcePasswordReset().
        /// </summary>
        /// <param name="userId">ID of the user for whom password reset is forced.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="NotFoundException">Thrown if no user exists with the given ID.</exception>
        public async Task ForceResetPasswordAsync(Guid userId)
        {
            var startLocal = _dateTimeProvider.NowLocal;
            _logger.LogDebug(
                "ForceResetPasswordAsync START: UserId={UserId}, At={TimeLocal}",
                userId,
                startLocal
            );

            var user =
                await _userRepository.FindByIdAsync(userId, bypassCache: true)
                ?? throw new NotFoundException("User", userId);

            user.ForcePasswordReset(); // enqueues UserPasswordResetForcedEvent
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            _logger.LogInformation(
                "ForceResetPasswordAsync SUCCESS: UserId={UserId}, At={TimeLocal}",
                userId,
                _dateTimeProvider.NowLocal
            );
            _logger.LogDebug(
                "ForceResetPasswordAsync END: Password reset forced for UserId={UserId} at {TimeLocal}",
                userId,
                _dateTimeProvider.NowLocal
            );
        }

        #endregion

        #region Internal Helpers

        /// <summary>
        /// Handles an already‐locked user at login: sends an account‐locked email, logs a warning,
        /// and throws UnauthorizedAccessException with the lockout end time.
        /// </summary>
        /// <param name="user">The locked‐out user entity.</param>
        private async Task HandleLockedAccountAsync(User user)
        {
            var lockoutEndUtc = user.LockoutEnd!.Value;
            var lockoutEndLocal = _dateTimeProvider.ConvertToLocal(lockoutEndUtc);
            var formatted = _dateTimeProvider.ToBrusselsTimeString(lockoutEndUtc);

            // Send account locked email
            await _emailSender.SendAccountLockedEmailAsync(
                user.Email.Value,
                $"{user.FirstName} {user.LastName}",
                lockoutEndLocal
            );

            _logger.LogWarning(
                "HandleLockedAccountAsync: Account lockout notification sent to {Email}, UserId={UserId}, At={TimeLocal}, LockedUntil={LockedUntilLocal}",
                user.Email.Value,
                user.Id,
                _dateTimeProvider.NowLocal,
                formatted
            );

            throw new UnauthorizedAccessException($"Account is locked. Try again at {formatted}.");
        }

        /// <summary>
        /// Handles a failed password attempt: increments failed attempts, possibly locks out user,
        /// sends lockout email if needed, and logs appropriate messages. Always throws UnauthorizedAccessException.
        /// Invokes User.RegisterFailedLogin().
        /// </summary>
        /// <param name="user">The user aggregate.</param>
        /// <param name="nowUtc">Current UTC time.</param>
        /// <param name="nowLocal">Current local time (Brussels).</param>
        /// <param name="ipAddress">Client IP (for logging).</param>
        /// <param name="userAgent">Client UA (for logging).</param>
        private async Task HandleFailedPasswordAsync(
            User user,
            DateTime nowUtc,
            DateTime nowLocal,
            string ipAddress,
            string userAgent
        )
        {
            // Increment failed count and possibly set lockout
            var lockoutEndUtc = user.RegisterFailedLogin(nowUtc);
            // enqueues UserLockedOutEvent if threshold reached
            await _userRepository.SaveChangesAsync();

            if (lockoutEndUtc.HasValue)
            {
                // Send lockout email
                var lockoutEndLocal = _dateTimeProvider.ConvertToLocal(lockoutEndUtc.Value);
                var formatted = _dateTimeProvider.ToBrusselsTimeString(lockoutEndUtc.Value);

                await _emailSender.SendAccountLockedEmailAsync(
                    user.Email.Value,
                    $"{user.FirstName} {user.LastName}",
                    lockoutEndLocal
                );

                _logger.LogError(
                    "HandleFailedPasswordAsync: LOCKOUT triggered for {Email}, UserId={UserId} at {LocalTime}, IP={IP}, UA={UA}. LockoutEnd={LockedUntil}, FailedAttempts={Count}",
                    user.Email.Value,
                    user.Id,
                    nowLocal,
                    ipAddress,
                    userAgent,
                    formatted,
                    user.FailedLoginAttempts
                );

                throw new UnauthorizedAccessException(
                    $"Account is locked. Try again at {formatted}."
                );
            }

            if (user.WillBeLockedOutAfterNextFailure)
            {
                _logger.LogInformation(
                    "HandleFailedPasswordAsync: SECURITY WARNING for {Email}, UserId={UserId} at {LocalTime}, IP={IP}, UA={UA}. FailedAttempts={Count}. Next failure will lock account.",
                    user.Email.Value,
                    user.Id,
                    nowLocal,
                    ipAddress,
                    userAgent,
                    user.FailedLoginAttempts
                );

                throw new UnauthorizedAccessException(
                    "Invalid credentials. Warning: one more failed attempt will lock your account."
                );
            }

            _logger.LogWarning(
                "HandleFailedPasswordAsync: Invalid login attempt for {Email}, UserId={UserId} at {LocalTime}, IP={IP}, UA={UA}. FailedAttempts={Count}",
                user.Email.Value,
                user.Id,
                nowLocal,
                ipAddress,
                userAgent,
                user.FailedLoginAttempts
            );

            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        #endregion
    }
}


// using Microsoft.Extensions.Logging;
// using PulseERP.Abstractions.Security.DTOs;
// using PulseERP.Abstractions.Security.Interfaces;
// using PulseERP.Domain.Entities;
// using PulseERP.Domain.Errors;
// using PulseERP.Domain.Interfaces;
// using PulseERP.Domain.Security.DTOs;
// using PulseERP.Domain.Security.Interfaces;
// using PulseERP.Domain.VO;

// namespace PulseERP.Infrastructure.Identity;

// /// <summary>
// /// Handles user registration, authentication (login), token refresh and logout.
// /// All logs are in Brussels local time. Password expiration enforced via RequirePasswordChange.
// /// </summary>
// public class AuthenticationService : IAuthenticationService
// {
//     private readonly IPasswordService _passwordService;
//     private readonly ITokenService _tokenService;
//     private readonly IUserRepository _userRepository;
//     private readonly ILogger<AuthenticationService> _logger;
//     private readonly IDateTimeProvider _dateTimeProvider;
//     private readonly IEmailSenderService _emailSender;

//     /// <summary>
//     /// Initializes a new instance of the <see cref="AuthenticationService"/> class.
//     /// </summary>
//     public AuthenticationService(
//         IPasswordService passwordService,
//         ITokenService tokenService,
//         IUserRepository userRepository,
//         ILogger<AuthenticationService> logger,
//         IDateTimeProvider dateTimeProvider,
//         IEmailSenderService emailSender
//     )
//     {
//         _passwordService = passwordService;
//         _tokenService = tokenService;
//         _userRepository = userRepository;
//         _logger = logger;
//         _dateTimeProvider = dateTimeProvider;
//         _emailSender = emailSender;
//     }

//     /// <summary>
//     /// Registers a new user, enforces unique email, sends welcome email, returns AuthResponse.
//     /// </summary>
//     public async Task<AuthResponse> RegisterAsync(
//         RegisterRequest request,
//         string ipAddress,
//         string userAgent
//     )
//     {
//         var start = DateTime.UtcNow;
//         _logger.LogInformation(
//             "User registration START: Email={Email}, IP={IP}, UA={UA}, At={TimeLocal}",
//             request.Email,
//             ipAddress,
//             userAgent,
//             _dateTimeProvider.NowLocal
//         );

//         EmailAddress email = new EmailAddress(request.Email);

//         await EnsureEmailNotUsedAsync(email);

//         var password = new Password(request.Password);
//         var passwordHash = _passwordService.HashPassword(password.ToString());

//         var user = new User(request.FirstName.Trim(), request.LastName.Trim(), email, passwordHash);

//         if (request.Phone is not null)
//         {
//             Phone phoneNumber = new Phone(request.Phone);
//             user.UpdatePhone(phoneNumber);
//         }
//         await _userRepository.AddAsync(user);
//         await _userRepository.SaveChangesAsync();

//         var userName = $"{user.FirstName} {user.LastName}";
//         var loginUrl =
//             $"https://app.pulseERP.com/login?email={Uri.EscapeDataString(request.Email)}";
//         await _emailSender.SendWelcomeEmailAsync(
//             toEmail: user.Email.Value,
//             userFullName: userName,
//             loginUrl: loginUrl
//         );

//         _logger.LogInformation(
//             "User {UserId} ({Email}) registered successfully from IP={IP}, UA={UA}, At={TimeLocal}. Duration={Duration}ms",
//             user.Id,
//             user.Email,
//             ipAddress,
//             userAgent,
//             _dateTimeProvider.NowLocal,
//             (DateTime.UtcNow - start).TotalMilliseconds
//         );

//         var accessToken = _tokenService.GenerateAccessToken(
//             user.Id,
//             user.Email.Value,
//             user.Role.ToString()
//         );
//         var refreshToken = await _tokenService.GenerateRefreshTokenAsync(
//             user.Id,
//             ipAddress,
//             userAgent
//         );

//         return new AuthResponse(CreateUserInfo(user), accessToken, refreshToken);
//     }

//     /// <summary>
//     /// Handles user login: verifies credentials, enforces lockout and password policies,
//     /// manages failed attempts, and issues JWT tokens. All logs in Brussels local time.
//     /// Strictly follows domain flow: only resets attempts on full login success, and handles
//     /// warnings and lockouts exactly as in legacy code.
//     /// </summary>
//     public async Task<AuthResponse> LoginAsync(
//         LoginRequest request,
//         string ipAddress,
//         string userAgent
//     )
//     {
//         var nowUtc = _dateTimeProvider.UtcNow;
//         var nowLocal = _dateTimeProvider.NowLocal;
//         var email = new EmailAddress(request.Email);

//         _logger.LogInformation(
//             "Login attempt for {Email} from {IP}, UA {UA} at {TimeLocal}.",
//             email,
//             ipAddress,
//             userAgent,
//             nowLocal
//         );

//         // Always bypass cache on login for security
//         var user =
//             await _userRepository.FindByEmailAsync(email, bypassCache: true)
//             ?? throw new NotFoundException("Invalid credentials.", email);

//         // 1. Lockout check — use helper (log, mail, throw)
//         if (user.IsLockedOut(nowUtc))
//             await HandleLockedAccountAsync(user);

//         // 2. Password check — use helper (increments, log, mail, throw)
//         if (!_passwordService.VerifyPassword(new Password(request.Password), user.PasswordHash))
//             await HandleFailedPasswordAsync(user, nowUtc, nowLocal);

//         // 3. Inactive account
//         if (!user.IsActive)
//         {
//             _logger.LogWarning(
//                 "Login attempt on inactive account {Email} at {NowLocal} (IP: {IP}, UA: {UA}).",
//                 email,
//                 nowLocal,
//                 ipAddress,
//                 userAgent
//             );
//             throw new UnauthorizedAccessException("Account is inactive.");
//         }

//         // 4. Password must be changed (expired/admin)
//         if (user.RequirePasswordChange)
//         {
//             _logger.LogInformation(
//                 "User {Email} requires password change before login (expired/admin) at {NowLocal}.",
//                 email,
//                 nowLocal
//             );
//             throw new UnauthorizedAccessException("Password change required.");
//         }

//         // 5. Success — reset counters, update last login, save
//         user.RegisterSuccessfulLogin(nowUtc);
//         await _userRepository.SaveChangesAsync();

//         _logger.LogInformation(
//             "User {UserId} ({Email}) successfully logged in from {IP} at {NowLocal}.",
//             user.Id,
//             user.Email,
//             ipAddress,
//             nowLocal
//         );

//         var accessToken = _tokenService.GenerateAccessToken(
//             user.Id,
//             user.Email.Value,
//             user.Role.ToString()
//         );
//         var refreshToken = await _tokenService.GenerateRefreshTokenAsync(
//             user.Id,
//             ipAddress,
//             userAgent
//         );

//         return new AuthResponse(CreateUserInfo(user), accessToken, refreshToken);
//     }

//     /// <summary>
//     /// Refreshes the access and refresh tokens for a user if the refresh token is valid.
//     /// Enforces password expiration before issuing new tokens.
//     /// </summary>
//     public async Task<AuthResponse> RefreshTokenAsync(
//         string refreshToken,
//         string ipAddress,
//         string userAgent
//     )
//     {
//         var start = DateTime.UtcNow;
//         _logger.LogInformation(
//             "RefreshToken START: IP={IP}, UA={UA}, At={TimeLocal}",
//             ipAddress,
//             userAgent,
//             _dateTimeProvider.NowLocal
//         );

//         RefreshTokenValidationResult validation =
//             await _tokenService.ValidateAndRevokeRefreshTokenAsync(refreshToken);
//         if (!validation.IsValid || validation.UserId is null)
//         {
//             _logger.LogWarning(
//                 "Invalid/expired refresh token from IP={IP}, UA={UA} at {TimeLocal}",
//                 ipAddress,
//                 userAgent,
//                 _dateTimeProvider.NowLocal
//             );
//             throw new UnauthorizedAccessException("Invalid or expired refresh token.");
//         }

//         var user =
//             await _userRepository.FindByIdAsync(validation.UserId.Value)
//             ?? throw new NotFoundException("User", validation.UserId.Value);

//         user.CheckPasswordExpiration(_dateTimeProvider.UtcNow);
//         if (user.RequirePasswordChange)
//         {
//             _logger.LogInformation(
//                 "Token refresh denied for user {Email}, UserId={UserId}: password change required at {TimeLocal}",
//                 user.Email,
//                 user.Id,
//                 _dateTimeProvider.NowLocal
//             );
//             throw new UnauthorizedAccessException(
//                 "Password expired. Please reset your password to continue."
//             );
//         }

//         var newAccessToken = _tokenService.GenerateAccessToken(
//             user.Id,
//             user.Email.Value,
//             user.Role.ToString()
//         );
//         var newRefreshToken = await _tokenService.GenerateRefreshTokenAsync(
//             user.Id,
//             ipAddress,
//             userAgent
//         );

//         _logger.LogInformation(
//             "Token refreshed for user {UserId} ({Email}) from IP={IP}, UA={UA}, At={TimeLocal}, Duration={Duration}ms",
//             user.Id,
//             user.Email,
//             ipAddress,
//             userAgent,
//             _dateTimeProvider.NowLocal,
//             (DateTime.UtcNow - start).TotalMilliseconds
//         );

//         return new AuthResponse(CreateUserInfo(user), newAccessToken, newRefreshToken);
//     }

//     /// <summary>
//     /// Logs out a user by revoking their refresh token.
//     /// </summary>
//     public async Task LogoutAsync(RefreshTokenDto refreshTokenDto)
//     {
//         var start = DateTime.UtcNow;
//         await _tokenService.ValidateAndRevokeRefreshTokenAsync(refreshTokenDto.Token);
//         _logger.LogInformation(
//             "Logout: Refresh token revoked at {TimeLocal}. Duration={Duration}ms",
//             _dateTimeProvider.NowLocal,
//             (DateTime.UtcNow - start).TotalMilliseconds
//         );
//     }

//     /// <summary>
//     /// Ensures an email is not already used (throws otherwise).
//     /// </summary>
//     private async Task EnsureEmailNotUsedAsync(EmailAddress email)
//     {
//         var exists = await _userRepository.FindByEmailAsync(email);
//         if (exists != null)
//         {
//             _logger.LogWarning(
//                 "Registration failed: Email {Email} already exists at {TimeLocal}.",
//                 email,
//                 _dateTimeProvider.NowLocal
//             );
//             throw new ValidationException(
//                 new Dictionary<string, string[]> { ["email"] = ["Email is already in use."] }
//             );
//         }
//     }

//     /// <summary>
//     /// Handles account lockout on login attempt. Sends notification and throws.
//     /// </summary>
//     private async Task HandleLockedAccountAsync(User user)
//     {
//         var lockoutLocal = user.LockoutEnd!.Value;
//         var formatted = _dateTimeProvider.ToBrusselsTimeString(lockoutLocal);

//         await _emailSender.SendAccountLockedEmailAsync(
//             user.Email.Value,
//             $"{user.FirstName} {user.LastName}",
//             lockoutLocal
//         );

//         _logger.LogWarning(
//             "LOCKOUT: Account lockout notification sent to {Email}, UserId={UserId}, At={TimeLocal}. Locked until {LockedUntil}.",
//             user.Email,
//             user.Id,
//             _dateTimeProvider.NowLocal,
//             formatted
//         );

//         throw new UnauthorizedAccessException($"Account is locked. Try again at {formatted}.");
//     }

//     /// <summary>
//     /// Handles logic for failed password attempts, lockout escalation, and user notifications.
//     /// Throws with proper message depending on attempt count.
//     /// </summary>
//     private async Task HandleFailedPasswordAsync(User user, DateTime nowUtc, DateTime nowLocal)
//     {
//         var lockoutEndUtc = user.RegisterFailedLogin(nowUtc);
//         await _userRepository.SaveChangesAsync();

//         if (lockoutEndUtc.HasValue)
//         {
//             var lockoutLocal = _dateTimeProvider.ConvertToLocal(lockoutEndUtc.Value);
//             var formatted = _dateTimeProvider.ToBrusselsTimeString(lockoutEndUtc.Value);

//             await _emailSender.SendAccountLockedEmailAsync(
//                 user.Email.Value,
//                 $"{user.FirstName} {user.LastName}",
//                 lockoutLocal
//             );

//             _logger.LogError(
//                 "LOCKOUT: User {Email}, UserId={UserId} locked out at {LocalTime}. LockoutEnd={LockedUntil}, FailedAttempts={Count}",
//                 user.Email,
//                 user.Id,
//                 nowLocal,
//                 formatted,
//                 user.FailedLoginAttempts
//             );

//             throw new UnauthorizedAccessException($"Account is locked. Try again at {formatted}.");
//         }

//         if (user.WillBeLockedOutAfterNextFailure)
//         {
//             _logger.LogInformation(
//                 "SECURITY WARNING: User {Email}, UserId={UserId}: next failed attempt will lock account. ({LocalTime}, Attempts={Count})",
//                 user.Email,
//                 user.Id,
//                 nowLocal,
//                 user.FailedLoginAttempts
//             );
//             throw new UnauthorizedAccessException(
//                 "Invalid credentials. Warning: one more failed attempt will lock your account."
//             );
//         }

//         _logger.LogWarning(
//             "Invalid login attempt for {Email}, UserId={UserId} at {LocalTime}. FailedAttempts={Count}",
//             user.Email,
//             user.Id,
//             nowLocal,
//             user.FailedLoginAttempts
//         );
//         throw new UnauthorizedAccessException("Invalid credentials.");
//     }

//     /// <summary>
//     /// Maps User entity to UserInfo DTO for AuthResponse.
//     /// </summary>
//     private static UserInfo CreateUserInfo(User user) =>
//         new(user.Id, user.FirstName, user.LastName, user.Email.ToString(), user.Role.ToString());
// }
