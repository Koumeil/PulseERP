using PulseERP.Domain.Enums.Product;
using PulseERP.Domain.Errors;
using PulseERP.Domain.ValueObjects;
using PulseERP.Domain.ValueObjects.Product;

namespace PulseERP.Domain.Entities;

public sealed class Product : BaseEntity
{
    public ProductName Name { get; private set; } = default!;
    public ProductDescription? Description { get; private set; }
    public Brand Brand { get; private set; } = default!;
    public Money Price { get; private set; } = default!;

    // Types valeur
    public int Quantity { get; private set; }
    public bool IsService { get; private set; }
    public bool IsActive { get; private set; }

    // Enum
    public ProductAvailabilityStatus Status { get; private set; }

    // Constructeur vide pour EF Core
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
        if (brand is null)
            throw new DomainException("Brand is required");

        var product = new Product
        {
            Name = new ProductName(name),
            Description = description is null ? null : new ProductDescription(description),
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
        string? name,
        string? description,
        Brand brand,
        decimal price,
        bool isService
    )
    {
        if (!string.IsNullOrWhiteSpace(name))
            Name = new ProductName(name);
        if (description is not null)
            Description = new ProductDescription(description);
        Brand = brand ?? throw new DomainException("Brand is required");
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

    public void Activate()
    {
        IsActive = true;
        UpdateStatus();
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        if (!IsActive)
            Status = ProductAvailabilityStatus.Discontinued;
        else if (Quantity == 0)
            Status = ProductAvailabilityStatus.OutOfStock;
        else if (Quantity <= 5)
            Status = ProductAvailabilityStatus.LowStock;
        else
            Status = ProductAvailabilityStatus.InStock;
    }
}
