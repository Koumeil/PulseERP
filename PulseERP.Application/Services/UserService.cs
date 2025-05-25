using AutoMapper;
using PulseERP.Contracts.Dtos.Services;
using PulseERP.Contracts.Dtos.Users;
using PulseERP.Contracts.Services;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Repositories;

namespace PulseERP.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly IAppLogger<UserService> _logger;
    private readonly IMapper _mapper;

    public UserService(IUserRepository repository, IAppLogger<UserService> logger, IMapper mapper)
    {
        _repository = repository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<UserDto>> CreateAsync(CreateUserCommand command)
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

            return Result<UserDto>.Success(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to create user", ex);
            return Result<UserDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<UserDto>> GetByIdAsync(Guid id)
    {
        var user = await _repository.GetByIdAsync(id);
        return user is null
            ? Result<UserDto>.Failure("User not found")
            : Result<UserDto>.Success(_mapper.Map<UserDto>(user));
    }

    public async Task<Result<IReadOnlyList<UserDto>>> GetAllAsync()
    {
        var users = await _repository.GetAllAsync();
        var dtos = users.Select(u => _mapper.Map<UserDto>(u)).ToList().AsReadOnly();
        return Result<IReadOnlyList<UserDto>>.Success(dtos);
    }

    public async Task<Result> UpdateAsync(Guid id, UpdateUserCommand command)
    {
        try
        {
            var user = await _repository.GetByIdAsync(id);
            if (user is null)
                return Result.Failure("User not found");

            user.UpdateName(command.FirstName, command.LastName);

            if (command.Email != null)
                user.ChangeEmail(command.Email);

            if (command.Phone != null)
                user.ChangePhone(command.Phone);

            await _repository.UpdateAsync(user);
            _logger.LogInformation($"Updated user {user.Id}");

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to update user {command.Id}", ex);
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        try
        {
            var user = await _repository.GetByIdAsync(id);
            if (user is null)
                return Result.Failure("User not found");

            user.Deactivate(); // Soft delete
            await _repository.UpdateAsync(user);
            _logger.LogInformation($"Deactivated user {user.Id}");

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to deactivate user {id}", ex);
            return Result.Failure(ex.Message);
        }
    }
}
