using PulseERP.Domain.Errors;

namespace PulseERP.Domain.ValueObjects.Product;

public sealed record ProductDescription
{
    public string? Value { get; }

    public ProductDescription(string? value)
    {
        Value = value?.Trim();
        if (Value?.Length > 1000)
            throw new DomainException("Product description too long (max 1000).");
    }

    public override string ToString() => Value ?? "";
    public static implicit operator string?(ProductDescription desc) => desc.Value;
    public static explicit operator ProductDescription(string? value) => new ProductDescription(value);
}
