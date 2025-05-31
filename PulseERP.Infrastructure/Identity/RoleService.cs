using Microsoft.Extensions.Logging;
using PulseERP.Abstractions.Security.DTOs;
using PulseERP.Abstractions.Security.Interfaces;
using PulseERP.Domain.Errors;
using PulseERP.Domain.Identity;
using PulseERP.Domain.Interfaces;
using PulseERP.Domain.ValueObjects;

namespace PulseERP.Infrastructure.Identity;

public class RoleService : IRoleService
{
    private readonly IUserQueryRepository _userQuery;
    private readonly IUserCommandRepository _userCommand;
    private readonly ILogger<RoleService> _logger;

    public RoleService(
        IUserQueryRepository userQuery,
        IUserCommandRepository userCommand,
        ILogger<RoleService> logger
    )
    {
        _userQuery = userQuery;
        _userCommand = userCommand;
        _logger = logger;
    }

    public async Task ChangeUserRoleAsync(
        Guid currentUserId,
        Guid targetUserId,
        UserRole oldRole,
        UserRole newRole
    )
    {
        if (currentUserId == Guid.Empty)
            throw new ArgumentException("Current user ID is required.", nameof(currentUserId));
        if (targetUserId == Guid.Empty)
            throw new ArgumentException("Target user ID is required.", nameof(targetUserId));
        if (oldRole == null)
            throw new ArgumentNullException(nameof(oldRole));
        if (newRole == null)
            throw new ArgumentNullException(nameof(newRole));

        var adminUser =
            await _userQuery.GetByIdAsync(currentUserId)
            ?? throw new UnauthorizedAccessException("User not found.");

        if (!adminUser.HasRole(SystemRoles.Admin))
            throw new UnauthorizedAccessException("Only an administrator can modify roles.");

        var targetUser =
            await _userQuery.GetByIdAsync(targetUserId)
            ?? throw new DomainException("Target user does not exist.");

        if (!targetUser.HasRole(Role.Create(oldRole.Name)))
            throw new DomainException($"Target user does not have role '{oldRole}'.");

        targetUser.SetRole(Role.Create(newRole.Name));
        await _userCommand.UpdateAsync(targetUser);
        await _userCommand.SaveChangesAsync();

        _logger.LogInformation(
            "User {AdminUserId} changed role for user {TargetUserId} from {OldRole} to {NewRole}",
            currentUserId,
            targetUserId,
            oldRole,
            newRole
        );
    }

    public async Task<bool> UserHasRoleAsync(Guid userId, UserRole role)
    {
        var user = await _userQuery.GetByIdAsync(userId);
        return user?.HasRole(Role.Create(role.Name)) ?? false;
    }
}
