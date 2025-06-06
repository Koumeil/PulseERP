using PulseERP.Domain.ValueObjects;

namespace PulseERP.Domain.VO;

/// <summary>
/// Predefined system roles used across the application.
/// Centralized definitions for consistency and reuse.
/// </summary>
public static class SystemRoles
{
    public static readonly Role Admin = new("Admin");
    public static readonly Role Manager = new("Manager");
    public static readonly Role User = new("User");
    public static readonly Role Default = new("Default");

    /// <summary>
    /// All declared roles for enumeration, validation, seeding, etc.
    /// </summary>
    public static IEnumerable<Role> All => new[] { Admin, Manager, User, Default };
}
