using PulseERP.Application.DTOs.Users;
using PulseERP.Application.Interfaces;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Interfaces.Persistence;

namespace PulseERP.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;

    public UserService(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> CreateAsync(CreateUserCommand command)
    {
        var user = new User(command.FirstName, command.LastName, command.Email, command.Phone);
        await _repository.AddAsync(user);
        return user.Id;
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var user = await _repository.GetByIdAsync(id);
        return user is null ? null : ToDto(user);
    }

    public async Task<List<UserDto>> GetAllAsync()
    {
        var users = await _repository.GetAllAsync();
        return users.Select(ToDto).ToList();
    }

    public async Task<bool> UpdateAsync(UpdateUserCommand command)
    {
        var user = await _repository.GetByIdAsync(command.Id);
        if (user is null)
            return false;

        user.UpdateContact(command.Phone);
        typeof(User).GetProperty("FirstName")?.SetValue(user, command.FirstName);
        typeof(User).GetProperty("LastName")?.SetValue(user, command.LastName);
        typeof(User).GetProperty("Email")?.SetValue(user, command.Email);

        await _repository.UpdateAsync(user);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user is null)
            return false;
        await _repository.DeleteAsync(user);
        return true;
    }

    private static UserDto ToDto(User user) =>
        new(user.Id, user.FirstName, user.LastName, user.Email, user.Phone, user.IsActive);
}
