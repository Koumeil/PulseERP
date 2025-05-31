namespace PulseERP.Domain.ValueObjects;

/// <summary>
/// Value-Object immuable représentant un rôle fonctionnel (Admin, Manager, …).
/// – Normalise la casse (trim + lower)\n
/// – Garantit qu’il n’est jamais vide\n
/// – Égalité et <c>GetHashCode</c> insensibles à la casse.
/// </summary>
public readonly struct Role : IEquatable<Role>, IComparable<Role>
{
    public string Value { get; }

    #region Ctor / Factory
    private Role(string role) => Value = role;

    /// <summary>Crée un rôle ; <c>null/empty</c> ⇒ rôle « User » (par défaut).</summary>
    public static Role Create(string? role = null)
    {
        var normalized = string.IsNullOrWhiteSpace(role) ? "User" : role.Trim();

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
