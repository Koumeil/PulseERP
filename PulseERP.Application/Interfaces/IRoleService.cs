using PulseERP.Domain.ValueObjects;

namespace PulseERP.Application.Interfaces;

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
