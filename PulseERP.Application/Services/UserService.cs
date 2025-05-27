using AutoMapper;
using PulseERP.Application.Exceptions;
using PulseERP.Application.Interfaces.Services;
using PulseERP.Contracts.Dtos.Users;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Interfaces.Repositories;
using PulseERP.Domain.Pagination;

namespace PulseERP.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly ISerilogAppLoggerService<UserService> _logger;
    private readonly IMapper _mapper;

    public UserService(
        IUserRepository repository,
        ISerilogAppLoggerService<UserService> logger,
        IMapper mapper
    )
    {
        _repository = repository;
        _logger = logger;
        _mapper = mapper;
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

    public async Task<UserDto> CreateAsync(CreateUserRequest command)
    {
        var user = User.Create(command.FirstName, command.LastName, command.Email, command.Phone);

        await _repository.AddAsync(user);
        _logger.LogInformation("Created user {UserId}", user.Id);

        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> UpdateAsync(Guid id, UpdateUserRequest command)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user is null)
            throw new NotFoundException("User", id);

        user.UpdateName(command.FirstName, command.LastName);

        if (command.Email != null)
            user.ChangeEmail(command.Email);

        if (command.Phone != null)
            user.ChangePhone(command.Phone);

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
