namespace PulseERP.Domain.VO;

using System.Globalization;
using Errors;

/// <summary>
/// Immutable Value Object representing a monetary amount with a specific currency (ISO 4217 code).
/// Invariants:
///   • Amount must be ≥ 0.
///   • Currency must be a non-null, 3-letter ISO 4217 code.
/// </summary>
public sealed class Money : ValueObject, IEquatable<Money>, IComparable<Money>
{
    #region Properties

    /// <summary>
    /// Decimal amount. Guaranteed to be non-negative.
    /// </summary>
    public decimal Amount { get; }

    /// <summary>
    /// Currency represented by an ISO 4217 code.
    /// </summary>
    public Currency Currency { get; }

    #endregion

    #region Constructor

    /// <summary>
    /// Creates a new <see cref="Money"/> instance after validating invariants.
    /// </summary>
    /// <param name="amount">Monetary amount (must be ≥ 0).</param>
    /// <param name="currency">Currency (must be non-null).</param>
    /// <exception cref="DomainValidationException">Thrown if <paramref name="amount"/> is negative.</exception>
    public Money(decimal amount, Currency currency)
    {
        if (amount < 0m)
            throw new DomainValidationException($"Amount must be non-negative; got {amount}.");

        Currency = currency ?? throw new ArgumentNullException(nameof(currency));
        Amount = amount;
    }

    #endregion

    #region Business Operations

    public Money Add(Money other)
    {
        ValidateSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        ValidateSameCurrency(other);
        var result = Amount - other.Amount;
        if (result < 0m)
            throw new DomainValidationException(
                $"Resulting amount cannot be negative (got {result})."
            );

        return new Money(result, Currency);
    }

    public Money Multiply(decimal factor)
    {
        if (factor < 0m)
            throw new DomainValidationException($"Factor must be non-negative; got {factor}.");

        return new Money(Amount * factor, Currency);
    }

    public Money Divide(decimal divisor)
    {
        if (divisor <= 0m)
            throw new DomainValidationException(
                $"Divisor must be greater than zero; got {divisor}."
            );

        return new Money(Amount / divisor, Currency);
    }

    public bool IsZero() => Amount == 0m;

    public bool IsGreaterThan(Money other)
    {
        ValidateSameCurrency(other);
        return Amount > other.Amount;
    }

    public bool IsLessThan(Money other)
    {
        ValidateSameCurrency(other);
        return Amount < other.Amount;
    }

    private void ValidateSameCurrency(Money other)
    {
        if (!Currency.Equals(other.Currency))
            throw new DomainValidationException(
                $"Cannot operate on Money with different currencies: '{Currency}' vs '{other.Currency}'."
            );
    }

    #endregion

    #region Display

    public string ToString(CultureInfo? culture)
    {
        var ci = culture ?? CultureInfo.InvariantCulture;
        return string.Format(ci, "{0:N2} {1}", Amount, Currency);
    }

    public override string ToString() => ToString(CultureInfo.InvariantCulture);

    #endregion

    #region Equality and Comparison

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override bool Equals(object? obj) => base.Equals(obj);

    public bool Equals(Money? other) => Equals((object?)other);

    public override int GetHashCode() => base.GetHashCode();

    public int CompareTo(Money? other)
    {
        if (other is null)
            return 1;

        ValidateSameCurrency(other);
        return Amount.CompareTo(other.Amount);
    }

    public static bool operator ==(Money? left, Money? right) => Equals(left, right);

    public static bool operator !=(Money? left, Money? right) => !Equals(left, right);

    #endregion
}
