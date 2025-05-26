using PulseERP.Domain.ValueObjects;

namespace PulseERP.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public Brand Brand { get; private set; }
    public Money Price { get; private set; }
    public int Quantity { get; private set; }
    public bool IsService { get; private set; } = false;
    public bool IsActive { get; private set; }

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
            throw new ArgumentException("Le nom est obligatoire", nameof(name));

        if (brand == null)
            throw new ArgumentNullException(nameof(brand), "La marque est obligatoire");

        if (price < 0)
            throw new ArgumentOutOfRangeException(
                nameof(price),
                "Le prix ne peut pas être négatif"
            );

        if (quantity < 0)
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                "La quantité ne peut pas être négative"
            );

        return new Product
        {
            Name = name.Trim(),
            Description = description?.Trim(),
            Brand = brand,
            Price = new Money(price),
            Quantity = quantity,
            IsActive = true,
            IsService = isService,
        };
    }

    public void UpdateName(string? name)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            Name = name.Trim();
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void UpdateDescription(string? description)
    {
        if (description != null)
        {
            Description = description.Trim();
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void UpdateBrand(string? brandName)
    {
        if (brandName is not null && brandName != Brand.Name)
        {
            UpdatedAt = DateTime.UtcNow;
            Brand.UpdateName(brandName);
        }
    }

    public void UpdatePrice(decimal? price)
    {
        if (price is not null)
        {
            if (price < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(price),
                    "Le prix ne peut pas être négatif"
                );

            Price = new Money(price.Value);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void UpdateQuantity(int? quantity)
    {
        if (quantity is not null)
        {
            if (quantity < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(quantity),
                    "La quantité ne peut pas être négative"
                );

            Quantity = quantity.Value;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void Deactivate()
    {
        if (IsActive)
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void Reactivate()
    {
        if (!IsActive)
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
