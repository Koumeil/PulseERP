using PulseERP.Domain.Errors;

namespace PulseERP.Domain.ValueObjects.Products;

/// <summary>
/// Value Object for product name.
/// </summary>
public sealed record ProductName
{
    public string Value { get; }

    private ProductName(string value)
    {
        Value = value;
    }

    public static ProductName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Product name is required.");
        if (value.Length > 200)
            throw new DomainException("Product name too long (max 200).");
        return new ProductName(value.Trim());
    }

    public override string ToString() => Value;

    public static implicit operator string(ProductName name) => name.Value;

    public static explicit operator ProductName(string value) => Create(value);
}
