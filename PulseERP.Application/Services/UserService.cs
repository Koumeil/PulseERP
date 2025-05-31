using AutoMapper;
using Microsoft.Extensions.Logging;
using PulseERP.Application.Dtos.User;
using PulseERP.Application.Interfaces;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Errors;
using PulseERP.Domain.Interfaces.Repositories;
using PulseERP.Domain.Interfaces.Services;
using PulseERP.Domain.Pagination;
using PulseERP.Domain.Query.Users;
using PulseERP.Domain.Shared.Roles;

public class UserService : IUserService
{
    private readonly IUserQueryRepository _userQuery;
    private readonly IUserCommandRepository _userCommand;
    private readonly IPasswordService _passwordService;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserQueryRepository userQuery,
        IUserCommandRepository userCommand,
        IPasswordService passwordService,
        IMapper mapper,
        ILogger<UserService> logger
    )
    {
        _userQuery = userQuery;
        _userCommand = userCommand;
        _passwordService = passwordService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PaginationResult<UserDto>> GetAllAsync(UserParams userParams)
    {
        var users = await _userQuery.GetAllAsync(userParams);
        return _mapper.Map<PaginationResult<UserDto>>(users);
    }

    public async Task<UserDetailsDto> GetByIdAsync(Guid id)
    {
        var user = await _userQuery.GetByIdAsync(id);
        if (user is null)
            throw new NotFoundException("User", id);

        return _mapper.Map<UserDetailsDto>(user);
    }

    public async Task<UserInfo> CreateAsync(CreateUserRequest request)
    {
        var passwordHash = _passwordService.HashPassword(request.Password);

        var user = User.Create(
            request.FirstName,
            request.LastName,
            Email.Create(request.Email),
            Phone.Create(request.Phone),
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

    public async Task<UserDto> UpdateAsync(Guid id, UpdateUserRequest request)
    {
        var user = await _userQuery.GetByIdAsync(id);
        if (user is null)
            throw new NotFoundException("User", id);

        user.UpdateName(request.FirstName, request.LastName);

        if (!string.IsNullOrWhiteSpace(request.Email))
            user.UpdateEmail(Email.Create(request.Email));

        if (!string.IsNullOrWhiteSpace(request.Phone))
            user.UpdatePhone(Phone.Create(request.Phone));

        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            var role = SystemRoles.All.FirstOrDefault(r => r.RoleName == request.Role);
            if (role is not null)
                user.SetRole(role);
        }

        await _userCommand.UpdateAsync(user);
        await _userCommand.SaveChangesAsync();

        _logger.LogInformation("Updated user {UserId}", user.Id);
        return _mapper.Map<UserDto>(user);
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
