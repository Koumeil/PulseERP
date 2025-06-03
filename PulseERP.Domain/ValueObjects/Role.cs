using System;
using System.Globalization;
using PulseERP.Domain.Errors;

namespace PulseERP.Domain.ValueObjects;

/// <summary>
/// Immutable Value Object representing a functional role (Admin, Manager, ...).
/// – Normalizes casing (trim + title case)
/// – Guarantees the role is never empty and always valid
/// – Equality and <c>GetHashCode</c> are case-insensitive.
/// </summary>
public readonly struct Role : IEquatable<Role>, IComparable<Role>
{
    public string Value { get; }

    private static readonly string[] AllowedRoles = { "User", "Manager", "Admin" };

    #region Ctor / Factory
    private Role(string role) => Value = role;

    /// <summary>
    /// Creates a role; null/empty → "User" (default).
    /// Throws DomainException if the role is not allowed.
    /// </summary>
    public static Role Create(string? role = null)
    {
        var normalized = string.IsNullOrWhiteSpace(role)
            ? "User"
            : CultureInfo.InvariantCulture.TextInfo.ToTitleCase(role.Trim().ToLowerInvariant());

        if (Array.IndexOf(AllowedRoles, normalized) < 0)
            throw new DomainException(
                $"Invalid role '{normalized}'. Allowed roles are: {string.Join(", ", AllowedRoles)}."
            );

        return new Role(normalized);
    }

    public static Role User => new("User");
    public static Role Manager => new("Manager");
    public static Role Admin => new("Admin");
    #endregion

    #region Equality & comparison
    public bool Equals(Role other) =>
        string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);

    public int CompareTo(Role other) =>
        StringComparer.OrdinalIgnoreCase.Compare(Value, other.Value);

    public override bool Equals(object? obj) => obj is Role r && Equals(r);

    public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Value);

    public static bool operator ==(Role left, Role right) => left.Equals(right);

    public static bool operator !=(Role left, Role right) => !left.Equals(right);

    public static bool operator >(Role left, Role right) => left.CompareTo(right) > 0;

    public static bool operator <(Role left, Role right) => left.CompareTo(right) < 0;
    #endregion

    #region Conversions & display
    public override string ToString() => Value;

    public static implicit operator string(Role r) => r.Value;

    public static explicit operator Role(string s) => Create(s);
    #endregion
}
