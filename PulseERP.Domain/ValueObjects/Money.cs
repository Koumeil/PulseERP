using System.Globalization;

namespace PulseERP.Domain.ValueObjects;

/// <summary>
/// Value Object immuable représentant une somme d’argent.
/// Invariants : valeur ≥ 0.
/// Les opérations (+, -) renvoient toujours un <see cref="Money"/> pour préserver l’invariant.
/// </summary>
public readonly struct Money : IComparable<Money>, IEquatable<Money>
{
    /// <summary>Montant hors devise (toujours positif ou nul).</summary>
    public decimal Value { get; }

    #region Ctor & Factory
    public Money(decimal value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(
                nameof(value),
                "Le montant ne peut pas être négatif."
            );

        Value = value;
    }

    public static Money Zero => new(0);
    #endregion

    #region Arithmetic
    public static Money operator +(Money left, Money right) => new(left.Value + right.Value);

    public static Money operator -(Money left, Money right)
    {
        var result = left.Value - right.Value;
        if (result < 0)
            throw new InvalidOperationException(
                "Le résultat d’une soustraction Money ne peut pas être négatif."
            );
        return new Money(result);
    }
    #endregion

    #region Comparison
    public int CompareTo(Money other) => Value.CompareTo(other.Value);

    public static bool operator >(Money left, Money right) => left.Value > right.Value;

    public static bool operator <(Money left, Money right) => left.Value < right.Value;

    public static bool operator >=(Money left, Money right) => left.Value >= right.Value;

    public static bool operator <=(Money left, Money right) => left.Value <= right.Value;
    #endregion

    #region Equality overrides
    public bool Equals(Money other) => Value == other.Value;

    public override bool Equals(object? obj) => obj is Money money && Equals(money);

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(Money left, Money right) => left.Equals(right);

    public static bool operator !=(Money left, Money right) => !left.Equals(right);
    #endregion

    #region Conversions (optionnelles)
    // public static implicit operator decimal(Money money) => money.Value;
    // public static explicit operator Money(decimal value) => new(value);
    #endregion

    #region Display
    public override string ToString() => Value.ToString("C", CultureInfo.GetCultureInfo("fr-FR")); // ex. "12,50 €"
    #endregion
}
