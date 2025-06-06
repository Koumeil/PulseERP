using PulseERP.Domain.Errors;

namespace PulseERP.Domain.VO;

public sealed class Currency : ValueObject, IEquatable<Currency>
{
    #region Properties

    /// <summary>
    /// The 3-letter ISO 4217 code (e.g., "USD", "EUR"). Always uppercase.
    /// </summary>
    public string Code { get; }

    #endregion

    #region Constructor
    /// <summary>
    /// Creates a new <see cref="Currency"/>.
    /// </summary>
    /// <param name="code">3-letter ISO currency code (e.g., "EUR").</param>
    /// <exception cref="DomainValidationException">Thrown if code is null, whitespace, or not exactly 3 characters.</exception>
    public Currency(string code)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Trim().Length != 3)
            throw new DomainValidationException($"Invalid currency code: {code}");

        Code = code.Trim().ToUpperInvariant();
    }

    #endregion

    #region Equality

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Code;
    }

    public bool Equals(Currency? other) => Equals((object?)other);

    public override string ToString() => Code;

    #endregion
}
