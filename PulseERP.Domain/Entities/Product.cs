using PulseERP.Domain.Enums.Product;
using PulseERP.Domain.Errors;
using PulseERP.Domain.ValueObjects;
using PulseERP.Domain.ValueObjects.Products;

namespace PulseERP.Domain.Entities
{
    /// <summary>
    /// Represents a product in the system. Acts as an aggregate root for product-related operations.
    /// </summary>
    public sealed class Product : BaseEntity
    {
        #region Properties

        /// <summary>
        /// Name of the product.
        /// </summary>
        public ProductName Name { get; private set; } = default!;

        /// <summary>
        /// Description of the product.
        /// </summary>
        public ProductDescription? Description { get; private set; }

        /// <summary>
        /// Brand of the product.
        /// </summary>
        public Brand Brand { get; private set; }

        public Guid BrandId { get; private set; }

        /// <summary>
        /// Price of the product.
        /// </summary>
        public Money Price { get; private set; } = default!;

        /// <summary>
        /// Available quantity in stock.
        /// </summary>
        public int Quantity { get; private set; }

        /// <summary>
        /// Indicates if the item is a service rather than physical good.
        /// </summary>
        public bool IsService { get; private set; }

        /// <summary>
        /// Indicates if the product is active (not discontinued).
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// Availability status of the product (InStock, OutOfStock, LowStock, Discontinued).
        /// </summary>
        public ProductAvailabilityStatus Status { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Private constructor for EF Core.
        /// </summary>
        private Product() { }

        #endregion

        #region Factory

        /// <summary>
        /// Creates a new product. Throws <see cref="DomainException"/> on invalid input.
        /// </summary>
        /// <param name="name">Product name (non-empty).</param>
        /// <param name="description">Product description (nullable).</param>
        /// <param name="brand">Brand instance (non-null).</param>
        /// <param name="price">Product price (positive).</param>
        /// <param name="quantity">Initial quantity (>=0).</param>
        /// <param name="isService">Indicates if it is a service.</param>
        /// <returns>New <see cref="Product"/> instance.</returns>
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
                throw new DomainException("Name is required.");
            if (brand is null)
                throw new DomainException("Brand is required.");
            if (price <= 0)
                throw new DomainException("Price must be positive.");
            if (quantity < 0)
                throw new DomainException("Quantity cannot be negative.");

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

        #region Methods

        /// <summary>
        /// Sets a new brand. Throws <see cref="DomainException"/> if <paramref name="brand"/> is null.
        /// </summary>
        /// <param name="brand">New <see cref="Brand"/> instance.</param>
        public void SetBrand(Brand brand) =>
            Brand = brand ?? throw new DomainException("Brand is required.");

        /// <summary>
        /// Updates product details: name, description, price, or service flag.
        /// </summary>
        /// <param name="name">New name (nullable).</param>
        /// <param name="description">New description (nullable).</param>
        /// <param name="price">New price (nullable).</param>
        /// <param name="isService">New service flag (nullable).</param>
        public void UpdateDetails(
            string? name = null,
            string? description = null,
            decimal? price = null,
            bool? isService = null
        )
        {
            if (!string.IsNullOrWhiteSpace(name))
                Name = new ProductName(name);
            if (description is not null)
                Description = new ProductDescription(description);
            if (price.HasValue)
            {
                if (price.Value <= 0)
                    throw new DomainException("Price must be positive.");
                Price = new Money(price.Value);
            }
            if (isService.HasValue)
                IsService = isService.Value;

            MarkAsUpdated();
        }

        /// <summary>
        /// Increases stock by the specified <paramref name="amount"/>. Throws <see cref="DomainException"/> if amount &lt;= 0.
        /// </summary>
        /// <param name="amount">Amount to add (positive).</param>
        public void Restock(int amount)
        {
            if (amount <= 0)
                throw new DomainException("Amount must be positive.");

            Quantity += amount;
            IsActive = true;
            UpdateStatus();
            MarkAsUpdated();
        }

        /// <summary>
        /// Marks product as out of stock (sets quantity to zero).
        /// </summary>
        public void MarkOutOfStock()
        {
            Quantity = 0;
            IsActive = true;
            UpdateStatus();
            MarkAsUpdated();
        }

        /// <summary>
        /// Discontinues the product (sets IsActive to false).
        /// </summary>
        public void Discontinue()
        {
            IsActive = false;
            UpdateStatus();
            MarkAsUpdated();
        }

        /// <summary>
        /// Activates the product if currently inactive.
        /// </summary>
        public void Activate()
        {
            if (!IsActive)
            {
                IsActive = true;
                UpdateStatus();
                MarkAsUpdated();
            }
        }

        /// <summary>
        /// Deactivates the product if currently active.
        /// </summary>
        public void Deactivate()
        {
            if (IsActive)
            {
                IsActive = false;
                UpdateStatus();
                MarkAsUpdated();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Updates the <see cref="Status"/> based on <see cref="Quantity"/> and <see cref="IsActive"/>.
        /// </summary>
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
}
