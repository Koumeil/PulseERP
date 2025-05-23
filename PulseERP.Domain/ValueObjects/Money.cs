namespace PulseERP.Domain.ValueObjects;

public readonly struct Money
{
    public decimal Value { get; }

    public Money(decimal value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(
                nameof(value),
                "Le montant ne peut pas être négatif"
            );

        Value = value;
    }

    public override string ToString() => $"{Value} €";

    public static implicit operator decimal(Money money) => money.Value;

    public static explicit operator Money(decimal value) => new Money(value);
}
