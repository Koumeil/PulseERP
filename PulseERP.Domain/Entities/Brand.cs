using PulseERP.Domain.Errors;

namespace PulseERP.Domain.Entities
{
    /// <summary>
    /// Represents a brand of products. Acts as an aggregate root for brand-related operations.
    /// </summary>
    public sealed class Brand : BaseEntity
    {
        #region Properties

        /// <summary>
        /// Name of the brand.
        /// </summary>
        public string Name { get; private set; } = default!;

        private readonly List<Product> _products = new();

        /// <summary>
        /// Read-only collection of associated products.
        /// </summary>
        public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

        /// <summary>
        /// Indicates if the brand is active.
        /// </summary>
        public bool IsActive { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Private constructor for EF Core.
        /// </summary>
        private Brand() { }

        #endregion

        #region Factory

        /// <summary>
        /// Creates a new brand. Throws <see cref="DomainException"/> if <paramref name="name"/> is null or whitespace.
        /// </summary>
        /// <param name="name">Brand name (non-empty).</param>
        /// <returns>New <see cref="Brand"/> instance.</returns>
        public static Brand Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Brand name required.");

            return new Brand { Name = name.Trim(), IsActive = true };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Updates the brand's name. Throws <see cref="DomainException"/> if <paramref name="newName"/> is null or whitespace.
        /// </summary>
        /// <param name="newName">New brand name (non-empty).</param>
        public void UpdateName(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
                throw new DomainException("Brand name required.");

            Name = newName.Trim();
            MarkAsUpdated();
        }

        /// <summary>
        /// Adds a product to the brand’s product collection. Throws <see cref="DomainException"/> if <paramref name="product"/> is null.
        /// </summary>
        /// <param name="product">Product to add (non-null).</param>
        public void AddProduct(Product product)
        {
            if (product is null)
                throw new DomainException("Cannot add null product.");

            _products.Add(product);
            MarkAsUpdated();
        }

        /// <summary>
        /// Removes a product from the brand’s product collection. Throws <see cref="DomainException"/> if <paramref name="product"/> is null.
        /// </summary>
        /// <param name="product">Product to remove (non-null).</param>
        public void RemoveProduct(Product product)
        {
            if (product is null)
                throw new DomainException("Cannot remove null product.");

            _products.Remove(product);
            MarkAsUpdated();
        }

        /// <summary>
        /// Activates the brand if currently inactive.
        /// </summary>
        public void Activate()
        {
            if (!IsActive)
            {
                IsActive = true;
                MarkAsUpdated();
            }
        }

        /// <summary>
        /// Deactivates the brand if currently active.
        /// </summary>
        public void Deactivate()
        {
            if (IsActive)
            {
                IsActive = false;
                MarkAsUpdated();
            }
        }

        #endregion
    }
}
