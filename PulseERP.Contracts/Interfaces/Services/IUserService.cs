using PulseERP.Contracts.Dtos.Services;
using PulseERP.Contracts.Dtos.Users;

namespace PulseERP.Contracts.Interfaces.Services;

public interface IUserService
{
    Task<ServiceResult<IReadOnlyList<UserDto>>> GetAllAsync();
    Task<ServiceResult<UserDto>> GetByIdAsync(Guid id);
    Task<ServiceResult<UserDto>> CreateAsync(CreateUserRequest command);
    Task<ServiceResult> UpdateAsync(Guid id, UpdateUserRequest command);
    Task<ServiceResult> DeleteAsync(Guid id);
}
