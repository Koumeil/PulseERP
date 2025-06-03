using Microsoft.Extensions.Logging;
using PulseERP.Abstractions.Security.DTOs;
using PulseERP.Abstractions.Security.Interfaces;
using PulseERP.Domain.Errors;
using PulseERP.Domain.Interfaces;
using PulseERP.Domain.Security.Roles;
using PulseERP.Domain.ValueObjects;

namespace PulseERP.Infrastructure.Identity;

public class RoleService : IRoleService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<RoleService> _logger;

    public RoleService(IUserRepository userRepository, ILogger<RoleService> logger)
    {
        _userRepository = userRepository;
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
            await _userRepository.FindByIdAsync(currentUserId)
            ?? throw new UnauthorizedAccessException("User not found.");

        if (!adminUser.HasRole(SystemRoles.Admin))
            throw new UnauthorizedAccessException("Only an administrator can modify roles.");

        var targetUser =
            await _userRepository.FindByIdAsync(targetUserId)
            ?? throw new DomainException("Target user does not exist.");

        if (!targetUser.HasRole(Role.Create(oldRole.Name)))
            throw new DomainException($"Target user does not have role '{oldRole}'.");

        targetUser.SetRole(Role.Create(newRole.Name));
        await _userRepository.UpdateAsync(targetUser);
        await _userRepository.SaveChangesAsync();

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
        var user = await _userRepository.FindByIdAsync(userId);
        return user?.HasRole(Role.Create(role.Name)) ?? false;
    }
}
