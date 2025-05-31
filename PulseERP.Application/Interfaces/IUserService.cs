using PulseERP.Application.Dtos.User;
using PulseERP.Domain.Pagination;
using PulseERP.Domain.Query.Users;

namespace PulseERP.Application.Interfaces;

public interface IUserService
{
    Task<PaginationResult<UserDto>> GetAllAsync(UserParams userParams);
    Task<UserDetailsDto> GetByIdAsync(Guid id);
    Task<UserInfo> CreateAsync(CreateUserRequest command);
    Task<UserDto> UpdateAsync(Guid id, UpdateUserRequest command);
    Task DeleteAsync(Guid id);
    Task ActivateUserAsync(Guid userId);
    Task DeactivateUserAsync(Guid userId);
}
