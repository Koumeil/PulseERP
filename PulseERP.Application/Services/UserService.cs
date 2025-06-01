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
using PulseERP.Domain.Identity;
using PulseERP.Domain.Interfaces;
using PulseERP.Domain.ValueObjects;

public class UserService : IUserService
{
    private readonly IUserQueryRepository _userQuery;
    private readonly IUserCommandRepository _userCommand;
    private readonly IPasswordService _passwordService;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UserService(
        IUserQueryRepository userQuery,
        IUserCommandRepository userCommand,
        IPasswordService passwordService,
        IMapper mapper,
        ILogger<UserService> logger,
        IDateTimeProvider dateTimeProvider
    )
    {
        _userQuery = userQuery;
        _userCommand = userCommand;
        _passwordService = passwordService;
        _mapper = mapper;
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<PagedResult<UserSummary>> GetAllAsync(UserFilter userFilter)
    {
        var users = await _userQuery.GetAllAsync(userFilter);
        return _mapper.Map<PagedResult<UserSummary>>(users);
    }

    public async Task<UserDetails> GetByIdAsync(Guid id)
    {
        var user = await _userQuery.GetByIdAsync(id);
        if (user is null)
            throw new NotFoundException("User", id);

        return _mapper.Map<UserDetails>(user);
    }

    public async Task<UserInfo> CreateAsync(CreateUserCommand cmd)
    {
        var passwordHash = _passwordService.HashPassword(cmd.Password);

        var user = User.Create(
            cmd.FirstName,
            cmd.LastName,
            EmailAddress.Create(cmd.Email),
            Phone.Create(cmd.Phone),
            passwordHash
        );
        await _userCommand.AddAsync(user);
        await _userCommand.SaveChangesAsync();

        _logger.LogInformation("User created with email {Email}", user.Email.Value);
        return new UserInfo(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email.Value,
            user.Phone.Value,
            user.Role.ToString()
        );
    }

    public async Task<UserDetails> UpdateAsync(Guid id, UpdateUserCommand cmd)
    {
        var user = await _userQuery.GetByIdAsync(id);
        if (user is null)
            throw new NotFoundException("User", id);

        user.UpdateName(cmd.FirstName, cmd.LastName);

        if (!string.IsNullOrWhiteSpace(cmd.Email))
            user.UpdateEmail(EmailAddress.Create(cmd.Email));

        if (!string.IsNullOrWhiteSpace(cmd.Phone))
            user.UpdatePhone(Phone.Create(cmd.Phone));

        if (!string.IsNullOrWhiteSpace(cmd.Role))
        {
            var role = SystemRoles.All.FirstOrDefault(r => r.Value == cmd.Role);
            if (role.Value is not null)
                user.SetRole(role);
        }

        await _userCommand.UpdateAsync(user);
        await _userCommand.SaveChangesAsync();

        _logger.LogInformation("Updated user {UserId}", user.Id);
        return _mapper.Map<UserDetails>(user);
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await _userQuery.GetByIdAsync(id);
        if (user is null)
            throw new NotFoundException("User", id);

        user.Deactivate();
        await _userCommand.UpdateAsync(user);
        await _userCommand.SaveChangesAsync();

        _logger.LogInformation("Deactivated user {UserId}", user.Id);
    }

    public async Task ActivateUserAsync(Guid id)
    {
        var user = await _userQuery.GetByIdAsync(id);
        if (user is null)
            throw new NotFoundException("User", id);

        user.Activate();
        await _userCommand.UpdateAsync(user);
        await _userCommand.SaveChangesAsync();

        _logger.LogInformation("Activated user {UserId}", id);
    }

    public async Task DeactivateUserAsync(Guid id)
    {
        var user = await _userQuery.GetByIdAsync(id);
        if (user is null)
            throw new NotFoundException("User", id);

        user.Deactivate();
        await _userCommand.UpdateAsync(user);
        await _userCommand.SaveChangesAsync();

        _logger.LogInformation("Deactivated user {UserId}", id);
    }
}
