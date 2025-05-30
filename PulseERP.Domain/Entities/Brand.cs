namespace PulseERP.Domain.Entities;

using PulseERP.Domain.Errors;

public sealed class Brand : BaseEntity
{
    public string Name { get; private set; }
    private readonly List<Product> _products = new();
    public IReadOnlyCollection<Product> Products => _products.AsReadOnly();
    public bool IsActive { get; private set; }

    private Brand() { }

    private Brand(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Brand name required");

        Name = name;
        IsActive = true;
    }

    public static Brand Create(string name) => new Brand(name);

    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new DomainException("Brand name required");

        Name = newName;
    }

    public void AddProduct(Product product)
    {
        if (product is null)
            throw new DomainException("Cannot add null product");
        _products.Add(product);
    }

    public void RemoveProduct(Product product)
    {
        if (product is null)
            throw new DomainException("Cannot remove null product");
        _products.Remove(product);
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}
