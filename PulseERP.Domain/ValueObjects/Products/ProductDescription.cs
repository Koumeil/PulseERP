using PulseERP.Domain.Errors;

namespace PulseERP.Domain.ValueObjects.Products;

/// <summary>
/// Value Object for product description.
/// </summary>
public sealed record ProductDescription
{
    public string? Value { get; }

    private ProductDescription(string? value)
    {
        Value = value;
    }

    public static ProductDescription Create(string? value)
    {
        string? trimmed = value?.Trim();
        if (trimmed != null && trimmed.Length > 1000)
            throw new DomainException("Product description too long (max 1000).");
        return new ProductDescription(trimmed);
    }

    public override string ToString() => Value ?? "";

    public static implicit operator string?(ProductDescription desc) => desc.Value;

    public static explicit operator ProductDescription(string? value) => Create(value);
}
