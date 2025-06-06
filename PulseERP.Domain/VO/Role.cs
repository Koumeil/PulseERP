namespace PulseERP.Domain.ValueObjects;

using System;
using PulseERP.Domain.Errors;

/// <summary>
/// Immutable Value Object representing a system or application role (e.g., "Admin", "Manager").
/// Invariants:
///  • Must be non-null, non-empty, trimmed.
///  • Case-insensitive equality (stored in Title Case).
///  • Cannot exceed 50 characters.
/// </summary>
public readonly struct Role : IEquatable<Role>
{
    #region Properties

    /// <summary>
    /// Underlying role name in Title Case (e.g., "Administrator").
    /// </summary>
    public string Value { get; }

    #endregion

    #region Constructor

    /// <summary>
    /// Creates a new <see cref="Role"/> after validating and normalizing to Title Case.
    /// </summary>
    /// <param name="roleName">Role name (must be 1-50 chars, non-empty/whitespace).</param>
    /// <exception cref="DomainValidationException">
    /// Thrown if <paramref name="roleName"/> is null, empty, or exceeds 50 characters.
    /// </exception>
    public Role(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            throw new DomainValidationException("Role cannot be null or whitespace.");

        var trimmed = roleName.Trim();
        if (trimmed.Length > 50)
            throw new DomainValidationException(
                $"Role cannot exceed 50 characters; got {trimmed.Length}."
            );

        // Normalize to Title Case (e.g., "system admin" → "System Admin")
        Value = ToTitleCase(trimmed);
    }

    #endregion

    #region Equality

    /// <summary>
    /// Implements <see cref="IEquatable{Role}"/>.
    /// Two <see cref="Role"/> instances are equal if their <see cref="Value"/> matches (case-insensitive).
    /// </summary>
    public bool Equals(Role other) => Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Role other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Value);

    /// <summary>
    /// Equality operator for <see cref="Role"/>.
    /// </summary>
    public static bool operator ==(Role left, Role right) => left.Equals(right);

    /// <summary>
    /// Inequality operator for <see cref="Role"/>.
    /// </summary>
    public static bool operator !=(Role left, Role right) => !(left == right);

    #endregion

    #region Helpers

    private static string ToTitleCase(string input)
    {
        // Splits by whitespace, capitalizes first letter of each word, lowercases the rest.
        var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < parts.Length; i++)
        {
            var word = parts[i].ToLowerInvariant();
            parts[i] = char.ToUpperInvariant(word[0]) + word.Substring(1);
        }
        return string.Join(' ', parts);
    }

    #endregion

    #region ToString Override

    /// <inheritdoc/>
    public override string ToString() => Value;
    #endregion
}
