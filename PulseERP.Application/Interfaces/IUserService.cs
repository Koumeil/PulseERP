using PulseERP.Domain.Dtos.Users;
using PulseERP.Domain.Pagination;

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
