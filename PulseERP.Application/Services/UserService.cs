using AutoMapper;
using Microsoft.Extensions.Logging;
using PulseERP.Abstractions.Common.Filters;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Abstractions.Security.DTOs;
using PulseERP.Abstractions.Security.Interfaces;
using PulseERP.Application.Interfaces;
using PulseERP.Application.Users.Commands;
using PulseERP.Application.Users.Models;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Errors;
using PulseERP.Domain.Interfaces;
using PulseERP.Domain.Security.Interfaces;
using PulseERP.Domain.ValueObjects;
using PulseERP.Domain.ValueObjects.Adresses;
using PulseERP.Domain.ValueObjects.Passwords;

namespace PulseERP.Application.Services;

/// <summary>
/// Service for managing users (CRUD, activation, password, etc.).
/// </summary>
public sealed class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UserService(
        IUserRepository userRepository,
        IPasswordService passwordService,
        IMapper mapper,
        ILogger<UserService> logger,
        IDateTimeProvider dateTimeProvider
    )
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _mapper = mapper;
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
    }

    /// <summary>
    /// Gets all users matching the filter, paginated.
    /// </summary>
    public async Task<PagedResult<UserSummary>> GetAllAsync(UserFilter userFilter)
    {
        var users = await _userRepository.GetAllAsync(userFilter);
        _logger.LogInformation(
            "Retrieved {Count} users (filter: {@Filter})",
            users.Items.Count,
            userFilter
        );
        return _mapper.Map<PagedResult<UserSummary>>(users);
    }

    /// <summary>
    /// Gets details of a single user by ID.
    /// </summary>
    public async Task<UserDetails> GetByIdAsync(Guid id)
    {
        var user =
            await _userRepository.FindByIdAsync(id, true)
            ?? throw new NotFoundException("User", id);
        _logger.LogInformation("Fetched user details for UserId {UserId}", id);
        return _mapper.Map<UserDetails>(user);
    }

    /// <summary>
    /// Creates a new user. Throws <see cref="ValidationException"/> on invalid data.
    /// </summary>
    public async Task<UserInfo> CreateAsync(CreateUserCommand cmd)
    {
        var errors = new Dictionary<string, string[]>();

        if (await _userRepository.FindByEmailAsync(cmd.Email) is not null)
            errors.Add(nameof(cmd.Email), new[] { "Email is already in use." });

        // Validate password (you can enrich Password.Create for more feedback if you want)
        try
        {
            Password.Create(cmd.Password);
        }
        catch (DomainException ex)
        {
            errors.Add(nameof(cmd.Password), [ex.Message]);
        }

        if (errors.Count > 0)
        {
            _logger.LogWarning("Validation failed during user creation: {@Errors}", errors);
            throw new ValidationException(errors);
        }

        var password = Password.Create(cmd.Password);
        var passwordHash = _passwordService.HashPassword(password.Value);

        var user = User.Create(
            cmd.FirstName,
            cmd.LastName,
            EmailAddress.Create(cmd.Email),
            Phone.Create(cmd.Phone),
            passwordHash
        );

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        _logger.LogInformation(
            "User created with email {Email} (UserId: {UserId})",
            user.Email.Value,
            user.Id
        );

        return new UserInfo(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email.Value,
            user.Phone.Value,
            user.Role.ToString()
        );
    }

    /// <summary>
    /// Updates a user’s basic information. Throws <see cref="NotFoundException"/> if not found, <see cref="ValidationException"/> if input invalid.
    /// </summary>
    public async Task<UserDetails> UpdateAsync(Guid id, UpdateUserCommand cmd)
    {
        var user =
            await _userRepository.FindByIdAsync(id, true)
            ?? throw new NotFoundException("User", id);

        var errors = new Dictionary<string, string[]>();

        if (!string.IsNullOrWhiteSpace(cmd.Email))
        {
            // Check for duplicate email, except for self
            var userWithEmail = await _userRepository.FindByEmailAsync(cmd.Email);
            if (userWithEmail is not null && userWithEmail.Id != id)
                errors.Add(nameof(cmd.Email), new[] { "Email is already in use by another user." });
        }

        if (!string.IsNullOrWhiteSpace(cmd.Role))
        {
            try
            {
                Role.Create(cmd.Role);
            }
            catch (DomainException ex)
            {
                errors.Add(nameof(cmd.Role), new[] { ex.Message });
            }
        }

        if (errors.Count > 0)
        {
            _logger.LogWarning(
                "Validation failed during user update (UserId: {UserId}): {@Errors}",
                id,
                errors
            );
            throw new ValidationException(errors);
        }

        user.UpdateName(cmd.FirstName, cmd.LastName);

        if (!string.IsNullOrWhiteSpace(cmd.Email))
            user.UpdateEmail(EmailAddress.Create(cmd.Email));

        if (!string.IsNullOrWhiteSpace(cmd.Phone))
            user.UpdatePhone(Phone.Create(cmd.Phone));

        if (!string.IsNullOrWhiteSpace(cmd.Role))
            user.SetRole(Role.Create(cmd.Role));

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        _logger.LogInformation("Updated user {UserId} ({Email})", user.Id, user.Email.Value);

        return _mapper.Map<UserDetails>(user);
    }

    /// <summary>
    /// Soft-deactivates a user.
    /// </summary>
    public async Task DeleteAsync(Guid id)
    {
        var user =
            await _userRepository.FindByIdAsync(id) ?? throw new NotFoundException("User", id);

        user.Deactivate();
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        _logger.LogInformation("Deactivated user {UserId} ({Email})", user.Id, user.Email.Value);
    }

    /// <summary>
    /// Activates a user account (if not already active).
    /// </summary>
    public async Task ActivateUserAsync(Guid id)
    {
        var user =
            await _userRepository.FindByIdAsync(id) ?? throw new NotFoundException("User", id);

        user.Activate();
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        _logger.LogInformation("Activated user {UserId} ({Email})", user.Id, user.Email.Value);
    }

    /// <summary>
    /// Deactivates a user account (if not already deactivated).
    /// </summary>
    public async Task DeactivateUserAsync(Guid id)
    {
        var user =
            await _userRepository.FindByIdAsync(id) ?? throw new NotFoundException("User", id);

        user.Deactivate();
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        _logger.LogInformation("Deactivated user {UserId} ({Email})", user.Id, user.Email.Value);
    }
}
