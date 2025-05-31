using System.Text.RegularExpressions;
using PulseERP.Domain.Errors;

namespace PulseERP.Domain.ValueObjects;

public sealed record Phone
{
    private static readonly Regex Rx = new(@"^\+?[0-9]{5,15}$", RegexOptions.Compiled);
    public string Value { get; }

    private Phone(string v) => Value = v;

    public static Phone Create(string number)
    {
        var n = number?.Trim();
        if (string.IsNullOrWhiteSpace(n) || !Rx.IsMatch(n))
            throw new DomainException("Téléphone invalide (5-15 chiffres, option +).");
        return new Phone(n);
    }

    public Phone Update(string newValue) =>
        string.Equals(Value, newValue?.Trim(), StringComparison.OrdinalIgnoreCase)
            ? this
            : Create(newValue!);

    public override string ToString() => Value;

    public static implicit operator Phone(string v) => Create(v);
}
