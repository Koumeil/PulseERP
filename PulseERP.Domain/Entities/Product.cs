namespace PulseERP.Domain.Entities;

using System;
using PulseERP.Domain.Common;
using PulseERP.Domain.Enums.Inventory;
using PulseERP.Domain.Enums.Product;
using PulseERP.Domain.Errors;
using PulseERP.Domain.Events.ProductEvents;
using PulseERP.Domain.VO;

/// <summary>
/// Aggregate root representing a product in the catalog, including inventory and lifecycle state.
/// </summary>
public sealed class Product : BaseEntity
{
    #region Properties

    public ProductName Name { get; private set; } = default!;
    public ProductDescription? Description { get; private set; }
    public Brand Brand { get; private set; } = default!;
    public Guid BrandId { get; private set; }

    public Money Price { get; private set; } = default!;
    public bool IsService { get; private set; }
    public ProductAvailabilityStatus Status { get; private set; }
    public DateTime? LastSoldAt { get; private set; }
    public Inventory Inventory { get; private set; } = default!;

    #endregion

    #region Constructor

    private Product() { }

    public Product(
        ProductName name,
        ProductDescription? description,
        Brand brand,
        Money price,
        int quantity,
        bool isService
    )
    {
        if (brand is null)
            throw new DomainValidationException("Brand is required.");
        if (price is null || price.IsZero())
            throw new DomainValidationException("Price must be greater than zero.");
        if (quantity < 0)
            throw new DomainValidationException("Quantity cannot be negative.");

        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description;
        Brand = brand;
        BrandId = brand.Id;
        Price = price;
        IsService = isService;
        Inventory = new Inventory(Id, quantity);

        UpdateStatus();
        AddDomainEvent(new ProductCreatedEvent(Id));
    }

    #endregion

    #region Domain Behaviors

    public void SetPrice(Money newPrice)
    {
        if (newPrice == null || newPrice.IsZero())
            throw new DomainValidationException("Price must be greater than zero.");

        if (!Price.Equals(newPrice))
        {
            Price = newPrice;
            MarkAsUpdated();
            AddDomainEvent(new ProductPriceChangedEvent(Id, newPrice));
        }
    }

    public void ApplyDiscount(decimal percentage)
    {
        if (percentage <= 0 || percentage >= 100)
            throw new DomainValidationException("Discount must be between 0 and 100 (exclusive).");

        var discountedPrice = Price.Multiply(1 - (percentage / 100m));
        SetPrice(discountedPrice);
    }

    public void SetBrand(Brand newBrand)
    {
        Brand = newBrand ?? throw new DomainValidationException("Brand is required.");
        BrandId = newBrand.Id;

        Brand?.RemoveProduct(this);
        newBrand.AddProduct(this);

        MarkAsUpdated();
        AddDomainEvent(new ProductBrandChangedEvent(Id, newBrand));
    }

    public void UpdateDetails(
        ProductName? name = null,
        ProductDescription? description = null,
        bool? isService = null
    )
    {
        var changes = false;

        if (TryUpdateName(name))
            changes = true;
        if (TryUpdateDescription(description))
            changes = true;

        if (TryUpdateServiceFlag(isService))
            changes = true;

        if (changes)
        {
            UpdateStatus();
            MarkAsUpdated();
            AddDomainEvent(new ProductDetailsUpdatedEvent(Id));
        }
    }

    private bool TryUpdateName(ProductName? name)
    {
        if (name is not null && !Name.Equals(name))
        {
            Name = name;
            return true;
        }
        return false;
    }

    private bool TryUpdateDescription(ProductDescription? description)
    {
        if (description is not null && !Equals(Description, description))
        {
            Description = description;
            return true;
        }
        return false;
    }

    private bool TryUpdateServiceFlag(bool? isService)
    {
        if (isService.HasValue && IsService != isService.Value)
        {
            IsService = isService.Value;
            return true;
        }
        return false;
    }

    public void SetQuantity(int newQuantity)
    {
        if (newQuantity < 0)
            throw new DomainValidationException("Quantity cannot be negative.");

        int current = Inventory.Quantity;

        if (newQuantity == current)
            return;

        if (newQuantity == 0)
        {
            MarkOutOfStock();
        }
        else if (newQuantity > current)
        {
            Restock(newQuantity - current);
        }
        else
        {
            MarkOutOfStock();
            Restock(newQuantity);
        }
    }

    public void Restock(int amount)
    {
        Inventory.Restock(amount);
        UpdateStatus();
        MarkAsUpdated();
    }

    public void RegisterSale(int quantitySold, DateTime saleDateUtc)
    {
        Inventory.Decrease(quantitySold);
        LastSoldAt = saleDateUtc;
        UpdateStatus();
        MarkAsUpdated();
        AddDomainEvent(new ProductSoldEvent(Id, quantitySold, saleDateUtc));
    }

    public void MarkOutOfStock()
    {
        Inventory.AdjustTo(0, InventoryMovementType.CorrectionDecrease);
        UpdateStatus();
        MarkAsUpdated();
        AddDomainEvent(new ProductMarkedOutOfStockEvent(Id));
    }

    public void DisableIfObsolete(TimeSpan inactivityPeriod, DateTime nowUtc)
    {
        if (LastSoldAt.HasValue && nowUtc - LastSoldAt.Value > inactivityPeriod)
        {
            MarkAsDeactivate();
        }
    }

    public void ArchiveIfOutOfStockAndInactive()
    {
        if (Status == ProductAvailabilityStatus.OutOfStock && !IsActive)
        {
            MarkAsDeleted();
        }
    }

    public bool CanBeSold(int quantity) =>
        quantity > 0
        && quantity <= Inventory.Quantity
        && IsActive
        && Status == ProductAvailabilityStatus.InStock;

    public bool IsLowStock(int threshold = 5) =>
        Inventory.Quantity > 0 && Inventory.Quantity <= threshold;

    public bool IsDormant(DateTime nowUtc, int months = 6)
    {
        return LastSoldAt.HasValue && nowUtc.Subtract(LastSoldAt.Value).TotalDays > months * 30;
    }

    public bool NeedsRestocking(int minThreshold)
    {
        return Inventory.Quantity < minThreshold && IsActive && !IsService;
    }

    public override void MarkAsDeleted()
    {
        if (!IsDeleted)
        {
            base.MarkAsDeleted();
            AddDomainEvent(new ProductDeletedEvent(Id));
        }
    }

    public override void MarkAsRestored()
    {
        if (IsDeleted)
        {
            base.MarkAsRestored();
            AddDomainEvent(new ProductRestoredEvent(Id));
        }
    }

    public override void MarkAsDeactivate()
    {
        base.MarkAsDeactivate();
        AddDomainEvent(new ProductDeactivatedEvent(Id));
    }

    public override void MarkAsActivate()
    {
        base.MarkAsActivate();
        AddDomainEvent(new ProductReactivatedEvent(Id));
    }

    #endregion

    #region Private Helpers

    private void UpdateStatus()
    {
        Status = !IsActive
            ? ProductAvailabilityStatus.Discontinued
            : Inventory.Quantity switch
            {
                0 => ProductAvailabilityStatus.OutOfStock,
                <= 5 => ProductAvailabilityStatus.LowStock,
                _ => ProductAvailabilityStatus.InStock,
            };
    }

    #endregion
}
