using PulseERP.Domain.Entities;
using PulseERP.Domain.Exceptions;
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

    // Constructeur EF Core privé pour ORM
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
            throw new DomainException("Price must be a non-negative value");
        if (quantity < 0)
            throw new DomainException("Quantity cannot be negative");

        return new Product
        {
            Name = name.Trim(),
            Description = description?.Trim(),
            Brand = brand,
            Price = new Money(price),
            Quantity = quantity,
            IsService = isService,
            IsActive = true,
        };
    }

    public void UpdateDetails(
        string? name = null,
        string? description = null,
        string? brandName = null,
        decimal? price = null,
        int? quantity = null
    )
    {
        var changed = false;

        if (!string.IsNullOrWhiteSpace(name) && name.Trim() != Name)
        {
            Name = name.Trim();
            changed = true;
        }

        if (description != null && description.Trim() != Description)
        {
            Description = description.Trim();
            changed = true;
        }

        if (brandName != null && brandName != Brand.Name)
        {
            Brand.UpdateName(brandName);
            changed = true;
        }

        if (price.HasValue)
        {
            if (price.Value < 0)
                throw new DomainException("Price must be a non-negative value");

            if (Price.Value != price.Value)
            {
                Price = new Money(price.Value);
                changed = true;
            }
        }

        if (quantity.HasValue)
        {
            if (quantity.Value < 0)
                throw new DomainException("Quantity cannot be negative");

            if (Quantity != quantity.Value)
            {
                Quantity = quantity.Value;
                changed = true;
            }
        }

        if (changed)
            MarkAsUpdated();
    }

    public void Deactivate() => ChangeStatus(false);

    public void Reactivate() => ChangeStatus(true);

    private void ChangeStatus(bool isActive)
    {
        if (IsActive != isActive)
        {
            IsActive = isActive;
            MarkAsUpdated();
        }
    }
}
