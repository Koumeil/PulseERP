using PulseERP.Application.DTOs.Users;

namespace PulseERP.Application.Interfaces;

public interface IUserService
{
    Task<Guid> CreateAsync(CreateUserCommand command);
    Task<UserDto?> GetByIdAsync(Guid id);
    Task<List<UserDto>> GetAllAsync();
    Task<bool> UpdateAsync(UpdateUserCommand command);
    Task<bool> DeleteAsync(Guid id);
}
