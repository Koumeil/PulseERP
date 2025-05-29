using PulseERP.Domain.Pagination;
using PulseERP.Shared.Dtos.Users;

namespace PulseERP.Application.Interfaces;

public interface IUserService
{
    Task<PaginationResult<UserDto>> GetAllAsync(PaginationParams paginationParams);
    Task<UserDto> GetByIdAsync(Guid id);
    Task<UserInfo> CreateAsync(CreateUserRequest command);
    Task<UserDto> UpdateAsync(Guid id, UpdateUserRequest command);
    Task DeleteAsync(Guid id);
    Task ActivateUserAsync(Guid userId);
    Task DeactivateUserAsync(Guid userId);
}
