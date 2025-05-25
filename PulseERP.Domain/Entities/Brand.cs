namespace PulseERP.Domain.Entities;

public class Brand : BaseEntity
{
    public string Name { get; private set; }

    // Navigation
    private readonly List<Product> _products = new();
    public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

    private Brand() { }

    private Brand(string name)
    {
        Name = name;
    }

    public static Brand Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Brand name cannot be empty.", nameof(name));

        return new Brand(name);
    }

    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Brand name cannot be empty.", nameof(newName));

        Name = newName;
    }
}
