using PulseERP.Abstractions.Common.Filters;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Abstractions.Security.DTOs;
using PulseERP.Application.Users.Commands;
using PulseERP.Application.Users.Models;

namespace PulseERP.Application.Interfaces;

public interface IUserService
{
    Task<PagedResult<UserSummary>> GetAllAsync(UserFilter userFilter);
    Task<UserDetails> GetByIdAsync(Guid id);
    Task<UserInfo> CreateAsync(CreateUserCommand cmd);
    Task<UserDetails> UpdateAsync(Guid id, UpdateUserCommand cmd);
    Task DeleteAsync(Guid id);
    Task ActivateUserAsync(Guid userId);
    Task DeactivateUserAsync(Guid userId);
}
