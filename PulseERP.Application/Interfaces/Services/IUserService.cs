using PulseERP.Contracts.Dtos.Users;
using PulseERP.Domain.Pagination;

namespace PulseERP.Application.Interfaces.Services;

public interface IUserService
{
    Task<PaginationResult<UserDto>> GetAllAsync(PaginationParams paginationParams);
    Task<UserDto> GetByIdAsync(Guid id);
    Task<UserDto> CreateAsync(CreateUserRequest command);
    Task<UserDto> UpdateAsync(Guid id, UpdateUserRequest command);
    Task DeleteAsync(Guid id);
}
