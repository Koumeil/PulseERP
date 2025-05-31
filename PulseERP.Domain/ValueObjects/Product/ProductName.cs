using PulseERP.Domain.Errors;

namespace PulseERP.Domain.ValueObjects.Product;

public sealed record ProductName
{
    public string Value { get; }

    public ProductName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Product name is required.");
        if (value.Length > 200)
            throw new DomainException("Product name too long (max 200).");
        Value = value.Trim();
    }

    public override string ToString() => Value;

    public static implicit operator string(ProductName name) => name.Value;

    public static explicit operator ProductName(string value) => new ProductName(value);
}
