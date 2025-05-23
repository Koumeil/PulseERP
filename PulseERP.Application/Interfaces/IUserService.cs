using PulseERP.Application.DTOs.Users;
using PulseERP.Domain.Shared;

namespace PulseERP.Application.Interfaces;

public interface IUserService
{
    Task<Result<Guid>> CreateAsync(CreateUserCommand command);
    Task<Result> DeleteAsync(Guid id);
    Task<Result<IReadOnlyList<UserDto>>> GetAllAsync();
    Task<Result<UserDto>> GetByIdAsync(Guid id);
    Task<Result> UpdateAsync(Guid id, UpdateUserCommand command);
}
