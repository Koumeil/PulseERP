using AutoMapper;
using PulseERP.Contracts.Dtos.Services;
using PulseERP.Contracts.Dtos.Users;
using PulseERP.Contracts.Interfaces.Services;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Interfaces.Repositories;

namespace PulseERP.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly IAppLoggerService<UserService> _logger;
    private readonly IMapper _mapper;

    public UserService(
        IUserRepository repository,
        IAppLoggerService<UserService> logger,
        IMapper mapper
    )
    {
        _repository = repository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<ServiceResult<UserDto>> CreateAsync(CreateUserRequest command)
    {
        try
        {
            var user = User.Create(
                command.FirstName,
                command.LastName,
                command.Email,
                command.Phone
            );

            await _repository.AddAsync(user);
            _logger.LogInformation($"Created user {user.Id}");

            var userDto = _mapper.Map<UserDto>(user);

            return ServiceResult<UserDto>.Success(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to create user", ex);
            return ServiceResult<UserDto>.Failure(ex.Message);
        }
    }

    public async Task<ServiceResult<UserDto>> GetByIdAsync(Guid id)
    {
        var user = await _repository.GetByIdAsync(id);
        return user is null
            ? ServiceResult<UserDto>.Failure("User not found")
            : ServiceResult<UserDto>.Success(_mapper.Map<UserDto>(user));
    }

    public async Task<ServiceResult<IReadOnlyList<UserDto>>> GetAllAsync()
    {
        var users = await _repository.GetAllAsync();
        var dtos = users.Select(u => _mapper.Map<UserDto>(u)).ToList().AsReadOnly();
        return ServiceResult<IReadOnlyList<UserDto>>.Success(dtos);
    }

    public async Task<ServiceResult> UpdateAsync(Guid id, UpdateUserRequest command)
    {
        try
        {
            var user = await _repository.GetByIdAsync(id);
            if (user is null)
                return ServiceResult.Failure("User not found");

            user.UpdateName(command.FirstName, command.LastName);

            if (command.Email != null)
                user.ChangeEmail(command.Email);

            if (command.Phone != null)
                user.ChangePhone(command.Phone);

            await _repository.UpdateAsync(user);
            _logger.LogInformation($"Updated user {user.Id}");

            return ServiceResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to update user {command.Id}", ex);
            return ServiceResult.Failure(ex.Message);
        }
    }

    public async Task<ServiceResult> DeleteAsync(Guid id)
    {
        try
        {
            var user = await _repository.GetByIdAsync(id);
            if (user is null)
                return ServiceResult.Failure("User not found");

            user.Deactivate(); // Soft delete
            await _repository.UpdateAsync(user);
            _logger.LogInformation($"Deactivated user {user.Id}");

            return ServiceResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to deactivate user {id}", ex);
            return ServiceResult.Failure(ex.Message);
        }
    }
}
