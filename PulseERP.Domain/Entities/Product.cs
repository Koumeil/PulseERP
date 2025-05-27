using PulseERP.Domain.Entities;
using PulseERP.Domain.ValueObjects;

public sealed class Product : BaseEntity
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

    public void UpdateDetails(
        string? name,
        string? description,
        string? brandName,
        decimal? price,
        int? quantity
    )
    {
        bool changed =
            ApplyIfChanged(Name, name?.Trim(), v => Name = v)
            | ApplyIfChanged(Description, description?.Trim(), v => Description = v);

        if (brandName is not null && brandName != Brand.Name)
        {
            Brand.UpdateName(brandName);
            changed = true;
        }

        if (price is not null)
        {
            if (price < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(price),
                    "Le prix ne peut pas être négatif"
                );
            if (Price.Value != price)
            {
                Price = new Money(price.Value);
                changed = true;
            }
        }

        if (quantity is not null)
        {
            if (quantity < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(quantity),
                    "La quantité ne peut pas être négative"
                );
            if (Quantity != quantity)
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

    private static bool ApplyIfChanged<T>(T current, T updated, Action<T> apply)
    {
        if (!EqualityComparer<T>.Default.Equals(current, updated))
        {
            apply(updated);
            return true;
        }
        return false;
    }
}
