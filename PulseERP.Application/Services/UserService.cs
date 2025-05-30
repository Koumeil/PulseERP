using AutoMapper;
using Microsoft.Extensions.Logging;
using PulseERP.Application.Interfaces;
using PulseERP.Domain.Dtos.Auth;
using PulseERP.Domain.Dtos.Users;
using PulseERP.Domain.Errors;
using PulseERP.Domain.Interfaces.Repositories;
using PulseERP.Domain.Interfaces.Services;
using PulseERP.Domain.Pagination;

namespace PulseERP.Application.Services;

public class UserService : IUserService
{
    private readonly IUserQueryRepository _userQuery;
    private readonly IUserCommandRepository _userCommand;
    private readonly IAuthenticationService _authService;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserQueryRepository userQuery,
        IUserCommandRepository userCommand,
        IAuthenticationService authService,
        IMapper mapper,
        ILogger<UserService> logger
    )
    {
        _userQuery = userQuery;
        _userCommand = userCommand;
        _authService = authService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PaginationResult<UserDto>> GetAllAsync(PaginationParams paginationParams)
    {
        var users = await _userQuery.GetAllAsync(paginationParams);
        return _mapper.Map<PaginationResult<UserDto>>(users);
    }

    public async Task<UserDto> GetByIdAsync(Guid id)
    {
        var user = await _userQuery.GetByIdAsync(id);
        if (user is null)
            throw new NotFoundException("User", id);

        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserInfo> CreateAsync(CreateUserRequest command)
    {
        var registerRequest = new RegisterRequest(
            command.FirstName,
            command.LastName,
            command.Email,
            command.Phone,
            command.Password
        );

        var authResult = await _authService.RegisterAsync(
            registerRequest,
            string.Empty,
            string.Empty
        );
        _logger.LogInformation("User created with email {Email}", command.Email);

        return authResult.User;
    }

    public async Task<UserDto> UpdateAsync(Guid id, UpdateUserRequest command)
    {
        var user = await _userQuery.GetByIdAsync(id);
        if (user is null)
            throw new NotFoundException("User", id);

        user.UpdateName(command.FirstName, command.LastName);

        if (!string.IsNullOrWhiteSpace(command.Email))
            user.UpdateEmail(Email.Create(command.Email));

        if (!string.IsNullOrWhiteSpace(command.Phone))
            user.UpdatePhone(Phone.Create(command.Phone));

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
