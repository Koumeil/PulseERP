using System.Text.RegularExpressions;
using PulseERP.Domain.Errors;

namespace PulseERP.Domain.ValueObjects;

public sealed record Password
{
    private const int Min = 8,
        Max = 20;
    private static readonly Regex U = new(@"[A-Z]"),
        L = new(@"[a-z]"),
        D = new(@"\d"),
        S = new(@"[^a-zA-Z0-9]");
    public string Value { get; }

    private Password(string v) => Value = v;

    public static Password Create(string plain)
    {
        if (plain is null)
            throw new DomainException("Pwd vide.");
        if (plain.Length is < Min or > Max)
            throw new DomainException($"Pwd {Min}-{Max} car.");
        if (!U.IsMatch(plain) || !L.IsMatch(plain) || !D.IsMatch(plain) || !S.IsMatch(plain))
            throw new DomainException("Pwd doit contenir maj, min, chiffre, spécial.");
        return new Password(plain);
    }

    public override string ToString() => "***";
}
