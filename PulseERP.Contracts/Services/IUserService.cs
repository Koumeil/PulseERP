using PulseERP.Contracts.Dtos.Services;
using PulseERP.Contracts.Dtos.Users;

namespace PulseERP.Contracts.Services;

public interface IUserService
{
    Task<Result<IReadOnlyList<UserDto>>> GetAllAsync();
    Task<Result<UserDto>> GetByIdAsync(Guid id);
    Task<Result<UserDto>> CreateAsync(CreateUserCommand command);
    Task<Result> UpdateAsync(Guid id, UpdateUserCommand command);
    Task<Result> DeleteAsync(Guid id);
}
