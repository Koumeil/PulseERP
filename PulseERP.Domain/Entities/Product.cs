using PulseERP.Domain.Entities;
using PulseERP.Domain.Enums.Product;
using PulseERP.Domain.Errors;
using PulseERP.Domain.ValueObjects;

public sealed class Product : BaseEntity
{
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public Brand Brand { get; private set; }
    public Money Price { get; private set; }
    public int Quantity { get; private set; }
    public bool IsService { get; private set; }
    public bool IsActive { get; private set; }
    public ProductAvailabilityStatus Status { get; private set; }

    private Product() { }

    public static Product Create(
        string name,
        string? description,
        Brand brand,
        decimal price,
        int quantity,
        bool isService
    )
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Name is required");
        if (brand is null)
            throw new DomainException("Brand is required");
        if (price < 0)
            throw new DomainException("Price must be non-negative");
        if (quantity < 0)
            throw new DomainException("Quantity must be non-negative");

        var product = new Product
        {
            Name = name,
            Description = description,
            Brand = brand,
            Price = new Money(price),
            Quantity = quantity,
            IsService = isService,
            IsActive = true,
        };

        product.UpdateStatus();
        return product;
    }

    public void UpdateDetails(
        string name,
        string? description,
        Brand brand,
        decimal price,
        bool isService
    )
    {
        Name = name;
        Description = description;
        Brand = brand;
        Price = new Money(price);
        IsService = isService;
    }

    public void IncreaseStock(int amount)
    {
        if (amount <= 0)
            throw new DomainException("Amount must be positive");
        Quantity += amount;
        UpdateStatus();
    }

    public void DecreaseStock(int amount)
    {
        if (amount <= 0 || amount > Quantity)
            throw new DomainException("Invalid stock operation");
        Quantity -= amount;
        UpdateStatus();
    }

    public void Discontinue()
    {
        IsActive = false;
        Status = ProductAvailabilityStatus.Discontinued;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    private void UpdateStatus()
    {
        if (!IsActive)
        {
            Status = ProductAvailabilityStatus.Discontinued;
        }
        else if (Quantity == 0)
        {
            Status = ProductAvailabilityStatus.OutOfStock;
        }
        else if (Quantity <= 5)
        {
            Status = ProductAvailabilityStatus.LowStock;
        }
        else
        {
            Status = ProductAvailabilityStatus.InStock;
        }
    }
}
