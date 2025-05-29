using Microsoft.Extensions.Logging;
using PulseERP.Application.Exceptions;
using PulseERP.Application.Interfaces;
using PulseERP.Application.Services;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Interfaces.Repositories;
using PulseERP.Domain.Interfaces.Services;
using PulseERP.Domain.ValueObjects;
using PulseERP.Shared.Dtos.Auth;
using PulseERP.Shared.Dtos.Users;

namespace PulseERP.Infrastructure.Identity;

public class AuthenticationService : IAuthenticationService
{
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;
    private readonly IUserQueryRepository _userQuery;
    private readonly IUserCommandRepository _userCommand;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        IPasswordService passwordService,
        ITokenService tokenService,
        IUserQueryRepository userQuery,
        IUserCommandRepository userCommand,
        ILogger<AuthenticationService> logger
    )
    {
        _passwordService = passwordService;
        _tokenService = tokenService;
        _userQuery = userQuery;
        _userCommand = userCommand;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request,
        string ipAddress,
        string userAgent
    )
    {
        _logger.LogInformation("Registering new user with email {Email}", request.Email);

        await EnsureEmailNotUsedAsync(request.Email);
        var passwordHash = _passwordService.HashPassword(request.Password);

        var domainUser = User.Create(
            request.FirstName,
            request.LastName,
            Email.Create(request.Email),
            Phone.Create(request.Phone),
            passwordHash,
            new SystemDateTimeProvider()
        );

        await _userCommand.AddAsync(domainUser);
        await _userCommand.SaveChangesAsync();

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

        var email = Email.Create(request.Email);
        var user = await _userQuery.GetByEmailAsync(email);

        if (user is null || !_passwordService.VerifyPassword(request.Password, user.PasswordHash))
        {
            if (user != null)
            {
                user.RegisterFailedLoginAttempt();
                await _userCommand.SaveChangesAsync();

                if (user.IsLockedOut)
                    _logger.LogWarning(
                        "User account {Email} locked due to too many failed login attempts",
                        request.Email
                    );
            }

            _logger.LogWarning("Invalid login attempt for {Email}", request.Email);
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login attempt on inactive account {Email}", request.Email);
            throw new UnauthorizedAccessException("Account is inactive.");
        }

        if (user.IsLockedOut)
        {
            _logger.LogWarning("Login attempt on locked account {Email}", request.Email);
            throw new UnauthorizedAccessException("Account is locked. Try again later.");
        }

        if (user.RequirePasswordChange)
        {
            _logger.LogInformation(
                "User {Email} must change password before continuing",
                request.Email
            );
            throw new UnauthorizedAccessException("Password change required.");
        }

        user.RegisterSuccessfulLogin();
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

// using Microsoft.Extensions.Logging;
// using PulseERP.Application.Exceptions;
// using PulseERP.Application.Interfaces.Services;
// using PulseERP.Domain.Entities;
// using PulseERP.Domain.Interfaces.Repositories;
// using PulseERP.Domain.Interfaces.Services;
// using PulseERP.Domain.ValueObjects;
// using PulseERP.Shared.Dtos.Auth;
// using PulseERP.Shared.Dtos.Users;

// namespace PulseERP.Infrastructure.Identity.Service;

// public class AuthService : IAuthService
// {
//     private readonly IUserPasswordService _passwordService;
//     private readonly ITokenService _tokenService;
//     private readonly ISmtpEmailService _emailService;
//     private readonly ILogger<AuthService> _logger;
//     private readonly IUserRepository _userRepository;

//     public AuthService(
//         IUserPasswordService passwordService,
//         ITokenService tokenService,
//         ISmtpEmailService emailService,
//         ILogger<AuthService> logger,
//         IUserRepository userRepository
//     )
//     {
//         _passwordService = passwordService;
//         _tokenService = tokenService;
//         _emailService = emailService;
//         _logger = logger;
//         _userRepository = userRepository;
//     }

//     public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
//     {
//         _logger.LogInformation("Registering new user with email {Email}", request.Email);

//         await EnsureEmailNotUsedAsync(request.Email);

//         var passwordHash = _passwordService.HashPassword(request.Password);
//         var domainUser = User.Create(
//             request.FirstName,
//             request.LastName,
//             Email.Create(request.Email),
//             PhoneNumber.Create(request.Phone),
//             passwordHash
//         );

//         await _userRepository.AddAsync(domainUser);

//         var accessToken = _tokenService.GenerateAccessToken(
//             domainUser.Id,
//             domainUser.Email.ToString(),
//             domainUser.Role.ToString()
//         );
//         var refreshToken = await _tokenService.GenerateRefreshTokenAsync(domainUser.Id);

//         return new AuthResponse(CreateUserInfo(domainUser), accessToken, refreshToken);
//     }

//     public async Task<AuthResponse> LoginAsync(LoginRequest request)
//     {
//         _logger.LogInformation("Attempting login for email {Email}", request.Email);

//         var email = Email.Create(request.Email);
//         var user = await _userRepository.GetUserByEmailAsync(email);

//         if (user is null || !_passwordService.VerifyPassword(request.Password, user.PasswordHash))
//         {
//             user?.RegisterFailedLoginAttempt();
//             await _userRepository.SaveChangesAsync();

//             _logger.LogWarning("Invalid login attempt for {Email}", request.Email);
//             throw new UnauthorizedAccessException("Invalid credentials.");
//         }

//         if (user.IsLockedOut)
//             throw new UnauthorizedAccessException("Account is locked. Try again later.");

//         user.RegisterSuccessfulLogin();
//         await _userRepository.SaveChangesAsync();

//         var accessToken = _tokenService.GenerateAccessToken(
//             user.Id,
//             user.Email.ToString(),
//             user.Role.ToString()
//         );
//         var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id);

//         return new AuthResponse(CreateUserInfo(user), accessToken, refreshToken);
//     }

//     public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
//     {
//         var tokenValidationResult = await _tokenService.ValidateAndRevokeRefreshTokenAsync(
//             refreshToken
//         );

//         if (!tokenValidationResult.IsValid || tokenValidationResult.UserId is null)
//             throw new UnauthorizedAccessException("Invalid or expired refresh token.");

//         var user =
//             await _userRepository.GetByIdAsync(tokenValidationResult.UserId.Value)
//             ?? throw new NotFoundException("User", tokenValidationResult.UserId);

//         var newAccessToken = _tokenService.GenerateAccessToken(
//             user.Id,
//             user.Email.ToString(),
//             user.Role.ToString()
//         );
//         var newRefreshTokenDto = await _tokenService.GenerateRefreshTokenAsync(user.Id);

//         return new AuthResponse(CreateUserInfo(user), newAccessToken, newRefreshTokenDto);
//     }

//     public async Task LogoutAsync(string refreshToken)
//     {
//         await _tokenService.RevokeRefreshTokenAsync(refreshToken);
//     }

//     private async Task EnsureEmailNotUsedAsync(string email)
//     {
//         var userExist = await _userRepository.GetUserByEmailAsync(Email.Create(email));
//         if (userExist != null)
//         {
//             throw new ValidationException(
//                 new Dictionary<string, string[]>
//                 {
//                     ["email"] = new[] { "Email is already in use." },
//                 }
//             );
//         }
//     }

//     private static UserInfo CreateUserInfo(User user) =>
//         new(user.Id, user.FirstName, user.LastName, user.Email.ToString(), user.Role.ToString());
// }
