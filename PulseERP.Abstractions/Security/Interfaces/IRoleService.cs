using PulseERP.Abstractions.Security.DTOs;

namespace PulseERP.Abstractions.Security.Interfaces;

public interface IRoleService
{
    Task ChangeUserRoleAsync(
        Guid currentUserId,
        Guid targetUserId,
        UserRole oldRole,
        UserRole newRole
    );
    Task<bool> UserHasRoleAsync(Guid userId, UserRole role);
}
