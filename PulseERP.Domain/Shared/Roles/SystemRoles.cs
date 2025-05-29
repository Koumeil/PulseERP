using PulseERP.Domain.ValueObjects;

namespace PulseERP.Domain.Shared.Roles;

public static class SystemRoles
{
    public static readonly UserRole Admin = UserRole.Create("Admin");
    public static readonly UserRole Manager = UserRole.Create("Manager");
    public static readonly UserRole User = UserRole.Create("User");

    private static readonly IReadOnlyCollection<UserRole> _all = new List<UserRole>
    {
        Admin,
        Manager,
        User,
    };

    public static IReadOnlyCollection<UserRole> All => _all;

    public static bool IsValid(UserRole role) => _all.Contains(role);
}
