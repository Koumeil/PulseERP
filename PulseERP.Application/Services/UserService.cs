using AutoMapper;
using PulseERP.Application.Exceptions;
using PulseERP.Application.Interfaces.Services;
using PulseERP.Domain.Interfaces.Repositories;
using PulseERP.Domain.Pagination;
using PulseERP.Domain.ValueObjects;
using PulseERP.Shared.Dtos.Auth;
using PulseERP.Shared.Dtos.Users;

namespace PulseERP.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly ISerilogAppLoggerService<UserService> _logger;
    private readonly IMapper _mapper;

    private readonly IAuthService _authService;

    public UserService(
        IUserRepository repository,
        ISerilogAppLoggerService<UserService> logger,
        IMapper mapper,
        IAuthService authService
    )
    {
        _repository = repository;
        _logger = logger;
        _mapper = mapper;
        _authService = authService;
    }

    public async Task<PaginationResult<UserDto>> GetAllAsync(PaginationParams paginationParams)
    {
        var users = await _repository.GetAllAsync(paginationParams);
        return _mapper.Map<PaginationResult<UserDto>>(users);
    }

    public async Task<UserDto> GetByIdAsync(Guid id)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user is null)
            throw new NotFoundException("User", id);

        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserInfo> CreateAsync(CreateUserRequest command)
    {
        var registerCommand = new RegisterRequest(
            command.FirstName,
            command.LastName,
            command.Email,
            command.Phone
        );

        var result = await _authService.RegisterAsync(registerCommand);

        _logger.LogInformation("Invitation email sent to {command.Email}", command.Email);

        return _mapper.Map<UserInfo>(result);
    }

    public async Task<UserDto> UpdateAsync(Guid id, UpdateUserRequest command)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user is null)
            throw new NotFoundException("User", id);

        user.UpdateName(command.FirstName, command.LastName);

        if (command.Email != null)
            user.ChangeEmail(Email.Create(command.Email));

        if (command.Phone != null)
            user.ChangePhone(PhoneNumber.Create(command.Phone));

        await _repository.UpdateAsync(user);
        _logger.LogInformation("Updated user {UserId}", user.Id);

        return _mapper.Map<UserDto>(user);
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user is null)
            throw new NotFoundException("User", id);

        user.Deactivate(); // Soft delete
        await _repository.UpdateAsync(user);
        _logger.LogInformation("Deactivated user {UserId}", user.Id);
    }
}
