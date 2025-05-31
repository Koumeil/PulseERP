using PulseERP.Domain.Enums.Product;
using PulseERP.Domain.Errors;
using PulseERP.Domain.ValueObjects;
using PulseERP.Domain.ValueObjects.Products;

namespace PulseERP.Domain.Entities;

public sealed class Product : BaseEntity
{
    #region Properties
    public ProductName Name { get; private set; } = default!;
    public ProductDescription? Description { get; private set; }
    public Brand Brand { get; private set; } = default!;
    public Money Price { get; private set; } = default!;
    public int Quantity { get; private set; }
    public bool IsService { get; private set; }
    public bool IsActive { get; private set; }
    public ProductAvailabilityStatus Status { get; private set; }
    #endregion

    private Product() { } // EF Core
    #region Factory
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
    #endregion

    #region Updates
    public void SetBrand(Brand brand) =>
        Brand = brand ?? throw new DomainException("Brand is required");

    public void UpdateDetails(
        string? name = null,
        string? description = null,
        decimal? price = null,
        bool? isService = null
    )
    {
        if (name is not null)
            Name = new ProductName(name);
        if (description is not null)
            Description = new ProductDescription(description);
        if (price.HasValue)
            Price = new Money(price.Value);
        if (isService.HasValue)
            IsService = isService.Value;
    }
    #endregion

    #region Stock & Status helpers
    public void Restock(int amount)
    {
        if (amount <= 0)
            throw new DomainException("Amount must be positive");
        Quantity += amount;
        IsActive = true;
        UpdateStatus();
    }

    public void MarkOutOfStock()
    {
        Quantity = 0;
        IsActive = true;
        UpdateStatus();
    }

    public void Discontinue()
    {
        IsActive = false;
        UpdateStatus(); // sets Discontinued
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
    #endregion

    #region Private
    private void UpdateStatus()
    {
        Status = !IsActive
            ? ProductAvailabilityStatus.Discontinued
            : Quantity switch
            {
                0 => ProductAvailabilityStatus.OutOfStock,
                <= 5 => ProductAvailabilityStatus.LowStock,
                _ => ProductAvailabilityStatus.InStock,
            };
    }
    #endregion
}
