// PulseERP.Domain.Identity/SystemRoles.cs
using PulseERP.Domain.ValueObjects;

namespace PulseERP.Domain.Identity;

public static class SystemRoles
{
    public static readonly Role Admin = Role.Create("Admin");
    public static readonly Role Manager = Role.Create("Manager");
    public static readonly Role User = Role.Create("User");

    private static readonly IReadOnlyCollection<Role> _all = new[] { Admin, Manager, User };

    public static IReadOnlyCollection<Role> All => _all;

    public static bool IsValid(Role role) => _all.Contains(role);
}
