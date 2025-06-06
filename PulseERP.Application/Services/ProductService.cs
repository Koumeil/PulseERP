using AutoMapper;
using Microsoft.Extensions.Logging;
using PulseERP.Abstractions.Common.DTOs.Inventories.Models;
using PulseERP.Abstractions.Common.DTOs.Products.Commands;
using PulseERP.Abstractions.Common.DTOs.Products.Models;
using PulseERP.Abstractions.Common.Filters;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Abstractions.Contracts.Repositories;
using PulseERP.Application.Helpers;
using PulseERP.Application.Interfaces;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Errors;
using PulseERP.Domain.VO;

namespace PulseERP.Application.Services;

public sealed class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IBrandRepository _brandRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductService> _logger;
    private readonly string _currencyCode = CurrencyHelper.GetCurrencyByCurrentRegion().ToString();

    public ProductService(
        IProductRepository productRepository,
        IBrandRepository brandRepository,
        IMapper mapper,
        ILogger<ProductService> logger
    )
    {
        _productRepository = productRepository;
        _brandRepository = brandRepository;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a paged list of all products, applying brand/status/min/max stock filters.
    /// </summary>
    public async Task<PagedResult<ProductSummary>> GetAllProductsAsync(ProductFilter filter)
    {
        // The repository should translate Brand, Status, MinStockLevel, MaxStockLevel, Search and Sort.
        var result = await _productRepository.GetAllAsync(filter);

        return new PagedResult<ProductSummary>
        {
            Items = _mapper.Map<List<ProductSummary>>(result.Items),
            TotalItems = result.TotalItems,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
        };
    }

    /// <summary>
    /// Retrieves a single product by ID, including Inventory and its Movements (eager loaded).
    /// </summary>
    /// <remarks>
    /// Ensure that FindByIdAsync is implemented to include Inventory & Inventory.Movements via EF Core Include(...).
    /// </remarks>
    public async Task<ProductDetails> GetProductByIdAsync(Guid id)
    {
        var product =
            await _productRepository.FindByIdAsync(id)
            ?? throw new NotFoundException("Product", id);

        return _mapper.Map<ProductDetails>(product);
    }

    /// <summary>
    /// Creates a new product (and brand if needed), then returns its details.
    /// </summary>
    public async Task<ProductDetails> CreateProductAsync(CreateProductCommand cmd)
    {
        // 1. Ensure the brand exists (or create it).
        if (cmd.BrandName == null)
            throw new DomainValidationException("Brand is required.");

        var brand = await GetOrCreateBrandAsync(cmd.BrandName);
        var price = new Money(cmd.Price, new Currency(_currencyCode));

        // 2. Instantiate the Product entity.
        var product = new Product(
            new ProductName(cmd.Name),
            new ProductDescription(cmd.Description),
            brand,
            price,
            cmd.Quantity,
            cmd.IsService
        );

        // 3. Persist and log.
        await _productRepository.AddAsync(product);
        await _productRepository.SaveChangesAsync();

        _logger.LogInformation("Product created: {@Product}", product);

        return _mapper.Map<ProductDetails>(product);
    }

    /// <summary>
    /// Updates product fields: name, description, price, isService and optionally adjusts quantity.
    /// </summary>
    public async Task<ProductDetails> UpdateProductAsync(Guid id, UpdateProductCommand cmd)
    {
        Product? product =
            await _productRepository.FindByIdAsync(id)
            ?? throw new NotFoundException("Product", id);

        await UpdateBrandIfNeededAsync(product, cmd.BrandName);

        try
        {
            if (cmd.Price.HasValue)
            {
                Money newPrice = new(cmd.Price.Value, product.Price.Currency);

                if (!product.Price.Currency.Equals(newPrice.Currency))
                {
                    throw new DomainValidationException(
                        $"Cannot update product {product.Id} with different currency. Existing: {product.Price.Currency}, New: {newPrice.Currency}."
                    );
                }

                if (product.Price.CompareTo(newPrice) != 0)
                {
                    product.SetPrice(newPrice);
                }
            }

            product.UpdateDetails(
                !string.IsNullOrWhiteSpace(cmd.Name) ? new ProductName(cmd.Name.Trim()) : null,
                !string.IsNullOrWhiteSpace(cmd.Description)
                    ? new ProductDescription(cmd.Description.Trim())
                    : null,
                cmd.IsService
            );

            // 3. Adjust quantity if requested (uses SetQuantity internally).
            if (cmd.Quantity.HasValue)
            {
                product.SetQuantity(cmd.Quantity.Value);
            }

            await _productRepository.UpdateAsync(product);
            await _productRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Product updated: {ProductId}, {ProductName}, {ProductPrice}",
                product.Id,
                product.Name,
                product.Price
            );
            return _mapper.Map<ProductDetails>(product);
        }
        catch (DomainValidationException ex)
        {
            _logger.LogWarning("Validation error updating product {Id}: {Message}", id, ex.Message);
            throw; // propagate for controller to turn into HTTP 400
        }
    }

    /// <summary>
    /// Soft-deletes a product (marks it as deleted).
    /// </summary>
    public async Task DeleteProductAsync(Guid id)
    {
        var product =
            await _productRepository.FindByIdAsync(id)
            ?? throw new NotFoundException("Product", id);

        product.MarkAsDeleted();

        await _productRepository.UpdateAsync(product);
        await _productRepository.SaveChangesAsync();

        _logger.LogWarning("Product deleted: {Id}", id);
    }

    /// <summary>
    /// Reactivates a previously deactivated product.
    /// </summary>
    public async Task ActivateProductAsync(Guid id)
    {
        var product =
            await _productRepository.FindByIdAsync(id)
            ?? throw new NotFoundException("Product", id);

        product.MarkAsActivate();

        await _productRepository.UpdateAsync(product);
        await _productRepository.SaveChangesAsync();

        _logger.LogInformation("Product activated: {Id}", id);
    }

    /// <summary>
    /// Deactivates an active product.
    /// </summary>
    public async Task DeactivateProductAsync(Guid id)
    {
        var product =
            await _productRepository.FindByIdAsync(id)
            ?? throw new NotFoundException("Product", id);

        product.MarkAsDeactivate();

        await _productRepository.UpdateAsync(product);
        await _productRepository.SaveChangesAsync();

        _logger.LogInformation("Product deactivated: {Id}", id);
    }

    /// <summary>
    /// Restores (undoes deletion) of a soft-deleted product.
    /// </summary>
    public async Task RestoreProductAsync(Guid id)
    {
        var product =
            await _productRepository.FindByIdAsync(id)
            ?? throw new NotFoundException("Product", id);

        product.MarkAsRestored();

        await _productRepository.UpdateAsync(product);
        await _productRepository.SaveChangesAsync();

        _logger.LogInformation("Product restored: {Id}", id);
    }

    /// <summary>
    /// Increases product inventory by 'quantity'. Throws if quantity ≤ 0.
    /// </summary>
    public async Task RestockProductAsync(Guid id, int quantity)
    {
        if (quantity <= 0)
            throw new DomainValidationException("Quantity must be greater than 0 for restock.");

        var product =
            await _productRepository.FindByIdAsync(id)
            ?? throw new NotFoundException("Product", id);

        try
        {
            product.Restock(quantity);
            await _productRepository.UpdateAsync(product);
            await _productRepository.SaveChangesAsync();

            _logger.LogInformation("Product restocked: {Id} (+{Qty})", id, quantity);
        }
        catch (DomainValidationException ex)
        {
            _logger.LogWarning(
                "Validation error restocking product {Id}: {Message}",
                id,
                ex.Message
            );
            throw;
        }
    }

    /// <summary>
    /// Decreases product inventory by 'quantity' (performs a sale). Throws if cannot be sold.
    /// </summary>
    public async Task SellProductAsync(Guid id, int quantity)
    {
        var product =
            await _productRepository.FindByIdAsync(id)
            ?? throw new NotFoundException("Product", id);

        try
        {
            if (!product.CanBeSold(quantity))
                throw new DomainValidationException(
                    "Product cannot be sold with the requested quantity."
                );

            product.RegisterSale(quantity, DateTime.UtcNow);
            await _productRepository.UpdateAsync(product);
            await _productRepository.SaveChangesAsync();

            _logger.LogInformation("Product sold: {Id}, Qty: {Qty}", id, quantity);
        }
        catch (DomainValidationException ex)
        {
            _logger.LogWarning("Validation error selling product {Id}: {Message}", id, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Changes the product’s price explicitly, generating ProductPriceChangedEvent in the domain.
    /// </summary>
    public async Task<ProductDetails> ChangeProductPriceAsync(Guid id, decimal newPrice)
    {
        var product =
            await _productRepository.FindByIdAsync(id)
            ?? throw new NotFoundException("Product", id);

        try
        {
            product.SetPrice(new Money(newPrice, new Currency(_currencyCode)));
            await _productRepository.UpdateAsync(product);
            await _productRepository.SaveChangesAsync();

            _logger.LogInformation("Product price changed: {Id} -> {Price}", id, newPrice);
            return _mapper.Map<ProductDetails>(product);
        }
        catch (DomainValidationException ex)
        {
            _logger.LogWarning(
                "Validation error changing price for product {Id}: {Message}",
                id,
                ex.Message
            );
            throw;
        }
    }

    /// <summary>
    /// Applies a percentage discount to the product’s current price.
    /// </summary>
    public async Task<ProductDetails> ApplyDiscountAsync(Guid id, decimal percentage)
    {
        var product =
            await _productRepository.FindByIdAsync(id)
            ?? throw new NotFoundException("Product", id);

        try
        {
            product.ApplyDiscount(percentage);
            await _productRepository.UpdateAsync(product);
            await _productRepository.SaveChangesAsync();

            _logger.LogInformation("Applied discount of {Pct}% to product {Id}", percentage, id);
            return _mapper.Map<ProductDetails>(product);
        }
        catch (DomainValidationException ex)
        {
            _logger.LogWarning(
                "Validation error applying discount for product {Id}: {Message}",
                id,
                ex.Message
            );
            throw;
        }
    }

    /// <summary>
    /// Returns true if the product’s inventory ≤ threshold; false otherwise.
    /// </summary>
    public async Task<bool> IsProductLowStockAsync(Guid id, int threshold = 5)
    {
        var product =
            await _productRepository.FindByIdAsync(id)
            ?? throw new NotFoundException("Product", id);

        return product.Inventory.IsLowStock(threshold);
    }

    /// <summary>
    /// Returns true if the product needs restocking (inventory ≤ minThreshold).
    /// </summary>
    public async Task<bool> NeedsRestockingAsync(Guid id, int minThreshold)
    {
        var product =
            await _productRepository.FindByIdAsync(id)
            ?? throw new NotFoundException("Product", id);

        return product.NeedsRestocking(minThreshold);
    }

    /// <summary>
    /// Retrieves all products whose stock is ≤ threshold and that are active.
    /// </summary>
    public async Task<IReadOnlyCollection<ProductSummary>> GetProductsBelowThresholdAsync(
        int threshold = 5
    )
    {
        // For large datasets, consider pushing this filter down into the repository (GetLowStockAsync).
        var all = await _productRepository.GetAllAsync(new ProductFilter());

        var filtered = all
            .Items.Where(p => p.Inventory.Quantity <= threshold && p.IsActive)
            .ToList();

        return _mapper.Map<List<ProductSummary>>(filtered);
    }

    /// <summary>
    /// Returns the full list of InventoryMovements for a given product.
    /// </summary>
    public async Task<IReadOnlyCollection<InventoryMovementModel>> GetInventoryMovementsAsync(
        Guid id
    )
    {
        var product =
            await _productRepository.FindByIdAsync(id)
            ?? throw new NotFoundException("Product", id);

        // Inventory.Movements should already be eager‐loaded by the repository
        var movements = product.Inventory.Movements;
        return _mapper.Map<List<InventoryMovementModel>>(movements);
    }

    /// <summary>
    /// Processes a return by adding 'quantity' back into inventory.
    /// </summary>
    public async Task<ProductDetails> ReturnProductAsync(Guid id, int quantity)
    {
        if (quantity <= 0)
            throw new DomainValidationException("Return amount must be positive");

        var product =
            await _productRepository.FindByIdAsync(id)
            ?? throw new NotFoundException("Product", id);

        try
        {
            // Assumes you have added Inventory.HandleReturn(int) to your Inventory entity.
            product.Inventory.HandleReturn(quantity);
            await _productRepository.UpdateAsync(product);
            await _productRepository.SaveChangesAsync();

            _logger.LogInformation("Return processed for product {Id}: +{Qty}", id, quantity);
            return _mapper.Map<ProductDetails>(product);
        }
        catch (DomainValidationException ex)
        {
            _logger.LogWarning(
                "Validation error on return for product {Id}: {Message}",
                id,
                ex.Message
            );
            throw;
        }
    }

    /// <summary>
    /// Archives any product that has been out of stock for more than 'outOfStockDays' AND is inactive for 'inactivityPeriod'.
    /// </summary>
    public async Task ArchiveStaleProductsAsync(TimeSpan inactivityPeriod, int outOfStockDays)
    {
        // Pull all products (no pagination) so we can check each one.
        var allProducts = await _productRepository.GetAllRawAsync();
        var now = DateTime.UtcNow;

        foreach (var product in allProducts)
        {
            bool hasBeenOutOfStockTooLong =
                product.Inventory.IsOutOfStock()
                && product.LastSoldAt.HasValue
                && now.Subtract(product.LastSoldAt.Value).TotalDays > outOfStockDays;

            if (hasBeenOutOfStockTooLong)
            {
                product.ArchiveIfOutOfStockAndInactive();
                if (product.IsDeleted) // only update if it actually changed
                {
                    await _productRepository.UpdateAsync(product);
                    _logger.LogInformation("Archived stale product {Id}", product.Id);
                }
            }
        }

        await _productRepository.SaveChangesAsync();
    }

    /// <summary>
    /// Returns all products for a given brand (possibly filtered by active status, stock, etc.).
    /// </summary>
    public async Task<PagedResult<ProductSummary>> GetProductsByBrandAsync(
        string brandName,
        ProductFilter filter
    )
    {
        // Override the filter's Brand, then reuse GetAllProductsAsync.
        filter = filter with
        {
            Brand = brandName.Trim(),
        };
        return await GetAllProductsAsync(filter);
    }

    #region Private Helpers

    /// <summary>
    /// Finds an existing Brand by name or creates a new one if it doesn't exist.
    /// </summary>
    private async Task<Brand> GetOrCreateBrandAsync(string brandName)
    {
        brandName = brandName.Trim();

        if (string.IsNullOrWhiteSpace(brandName))
        {
            throw new DomainValidationException("BrandName is required.");
        }

        var brand = await _brandRepository.FindByNameAsync(brandName);

        if (brand is not null)
            return brand;

        brand = new Brand(brandName);
        await _brandRepository.AddAsync(brand);
        await _brandRepository.SaveChangesAsync();
        _logger.LogInformation("Brand created: {Brand}", brandName);

        return brand;
    }

    /// <summary>
    /// Changes the product's brand only if the provided brand name is non‐empty.
    /// </summary>
    private async Task UpdateBrandIfNeededAsync(Product product, string? brandName)
    {
        if (string.IsNullOrWhiteSpace(brandName))
            return;

        var brand = await GetOrCreateBrandAsync(brandName.Trim());
        var oldBrand = product.Brand;
        product.SetBrand(brand);

        // Maintain bidirectional consistency:
        if (oldBrand is not null)
        {
            oldBrand.RemoveProduct(product);
            await _brandRepository.UpdateAsync(oldBrand);
        }
        brand.AddProduct(product);
        await _brandRepository.UpdateAsync(brand);
    }

    #endregion
}
