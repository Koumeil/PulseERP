namespace PulseERP.Domain.ValueObjects;

public sealed class UserRole : IEquatable<UserRole>
{
    public string RoleName { get; }

    private UserRole(string roleName)
    {
        RoleName = roleName;
    }

    public static UserRole Create(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            throw new ArgumentException("Role name cannot be empty.", nameof(roleName));

        return new UserRole(roleName.Trim());
    }

    public bool Equals(UserRole? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return string.Equals(RoleName, other.RoleName, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj) => Equals(obj as UserRole);

    public override int GetHashCode() => RoleName.ToLowerInvariant().GetHashCode();

    public override string ToString() => RoleName;

    public static bool operator ==(UserRole? left, UserRole? right) => Equals(left, right);

    public static bool operator !=(UserRole? left, UserRole? right) => !Equals(left, right);
}
