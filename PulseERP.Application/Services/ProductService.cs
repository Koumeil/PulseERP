using AutoMapper;
using Microsoft.Extensions.Logging;
using PulseERP.Abstractions.Common.Filters;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Application.Interfaces;
using PulseERP.Application.Products.Commands;
using PulseERP.Application.Products.Models;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Errors;
using PulseERP.Domain.Interfaces;
using PulseERP.Domain.ValueObjects;
using PulseERP.Domain.ValueObjects.Products;

namespace PulseERP.Application.Services;

/// <summary>
/// Service for managing product lifecycle, stock, and brand association.
/// Handles creation, update, activation, deactivation, and logical deletion of products.
/// </summary>
public sealed class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IBrandRepository _brandRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductService> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="ProductService"/>.
    /// </summary>
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

    #region READ

    /// <summary>
    /// Retrieves a paged list of product summaries, optionally filtered.
    /// </summary>
    public async Task<PagedResult<ProductSummary>> GetAllAsync(
        PaginationParams pagination,
        ProductFilter filter
    )
    {
        var paged = await _productRepository.GetAllAsync(pagination, filter);

        _logger.LogInformation(
            "Fetched {Count} products (page {Page})",
            paged.Items.Count,
            paged.PageNumber
        );

        return new PagedResult<ProductSummary>
        {
            Items = _mapper.Map<List<ProductSummary>>(paged.Items),
            TotalItems = paged.TotalItems,
            PageNumber = paged.PageNumber,
            PageSize = paged.PageSize,
        };
    }

    /// <summary>
    /// Retrieves product details by unique identifier. Throws <see cref="NotFoundException"/> if not found.
    /// </summary>
    public async Task<ProductDetails> GetByIdAsync(Guid id)
    {
        var entity =
            await _productRepository.GetByIdAsync(id) ?? throw new NotFoundException("Product", id);
        _logger.LogInformation("Fetched product {ProductId}", id);
        return _mapper.Map<ProductDetails>(entity);
    }

    #endregion

    #region CREATE

    /// <summary>
    /// Creates a new product, associating to existing brand or creating one if not found.
    /// </summary>
    /// <exception cref="ValidationException">If product input is invalid.</exception>
    public async Task<ProductDetails> CreateAsync(CreateProductCommand cmd)
    {
        // --- Handle brand existence or creation ---
        var brand = await _brandRepository.GetByNameAsync(cmd.Brand);
        if (brand is null)
        {
            brand = Brand.Create(cmd.Brand);
            await _brandRepository.AddAsync(brand);
            await _brandRepository.SaveChangesAsync();
            _logger.LogInformation(
                "Created brand '{BrandName}' during product creation",
                cmd.Brand
            );
        }

        // --- Product creation (may throw DomainException) ---
        var product = Product.Create(
            cmd.Name,
            cmd.Description,
            brand,
            cmd.Price,
            cmd.Quantity,
            cmd.IsService
        );

        await _productRepository.AddAsync(product);
        await _productRepository.SaveChangesAsync();

        _logger.LogInformation(
            "Product {ProductId} created (Name: {Name}, Brand: {Brand}, Price: {Price}, Quantity: {Quantity})",
            product.Id,
            product.Name,
            product.Brand.Name,
            product.Price.Value,
            product.Quantity
        );

        return _mapper.Map<ProductDetails>(product);
    }

    #endregion

    #region UPDATE

    /// <summary>
    /// Updates a product by ID. Throws <see cref="NotFoundException"/> if not found,
    /// <see cref="ValidationException"/> if validation fails.
    /// </summary>
    public async Task<ProductDetails> UpdateAsync(Guid id, UpdateProductCommand cmd)
    {
        var errors = new Dictionary<string, string[]>();
        var product =
            await _productRepository.GetByIdAsync(id) ?? throw new NotFoundException("Product", id);

        // --- Brand management ---
        if (!string.IsNullOrWhiteSpace(cmd.BrandName))
        {
            var brandName = cmd.BrandName.Trim();
            var brand = await _brandRepository.GetByNameAsync(brandName);
            if (brand is null)
            {
                try
                {
                    brand = Brand.Create(brandName);
                    await _brandRepository.AddAsync(brand);
                    await _brandRepository.SaveChangesAsync();
                    _logger.LogInformation(
                        "Created new brand '{BrandName}' during product update (ProductId: {ProductId})",
                        brandName,
                        id
                    );
                }
                catch (DomainException ex)
                {
                    errors[nameof(cmd.BrandName)] = new[] { ex.Message };
                }
            }
            if (brand != null && product.Brand.Id != brand.Id)
            {
                try
                {
                    product.SetBrand(brand);
                }
                catch (DomainException ex)
                {
                    errors[nameof(cmd.BrandName)] = new[] { ex.Message };
                }
            }
        }

        // --- Other field validations and updates ---
        if (cmd.Name is not null)
        {
            try
            {
                product.UpdateDetails(name: ProductName.Create(cmd.Name));
            }
            catch (DomainException ex)
            {
                errors[nameof(cmd.Name)] = new[] { ex.Message };
            }
        }
        if (cmd.Description is not null)
        {
            try
            {
                product.UpdateDetails(description: ProductDescription.Create(cmd.Description));
            }
            catch (DomainException ex)
            {
                errors[nameof(cmd.Description)] = new[] { ex.Message };
            }
        }
        if (cmd.Price.HasValue)
        {
            try
            {
                product.UpdateDetails(price: new Money(cmd.Price.Value));
            }
            catch (ArgumentOutOfRangeException ex)
            {
                errors[nameof(cmd.Price)] = new[] { ex.Message };
            }
        }
        if (cmd.IsService.HasValue)
            product.UpdateDetails(isService: cmd.IsService.Value);

        // --- Quantity / stock management ---
        if (cmd.Quantity.HasValue)
        {
            if (cmd.Quantity < 0)
            {
                errors[nameof(cmd.Quantity)] = new[] { "Quantity must be zero or positive." };
            }
            else if (cmd.Quantity != product.Quantity)
            {
                if (cmd.Quantity == 0)
                    product.MarkOutOfStock();
                else if (cmd.Quantity > product.Quantity)
                    product.Restock(cmd.Quantity.Value - product.Quantity);
                else // decrease to non-zero
                {
                    product.MarkOutOfStock();
                    product.Restock(cmd.Quantity.Value);
                }
            }
        }

        // --- Validation aggregation ---
        if (errors.Count > 0)
        {
            _logger.LogWarning(
                "Validation failed during product update (ProductId: {ProductId}): {@Errors}",
                id,
                errors
            );
            throw new ValidationException(errors);
        }

        await _productRepository.UpdateAsync(product);
        await _productRepository.SaveChangesAsync();

        _logger.LogInformation(
            "Product {ProductId} updated (Name: {Name}, Brand: {Brand}, Price: {Price}, Quantity: {Quantity})",
            product.Id,
            product.Name,
            product.Brand.Name,
            product.Price.Value,
            product.Quantity
        );

        return _mapper.Map<ProductDetails>(product);
    }

    #endregion

    #region STATE CHANGES

    /// <summary>
    /// Marks a product as discontinued (permanently removed from catalog, not reversible).
    /// </summary>
    public async Task DiscontinueAsync(Guid id)
    {
        var product =
            await _productRepository.GetByIdAsync(id) ?? throw new NotFoundException("Product", id);

        product.Discontinue();
        await _productRepository.UpdateAsync(product);
        await _productRepository.SaveChangesAsync();

        _logger.LogInformation("Product {ProductId} discontinued", product.Id);
    }

    /// <summary>
    /// Reactivates a product that was previously deactivated or discontinued.
    /// </summary>
    public async Task ActivateAsync(Guid id)
    {
        var product =
            await _productRepository.GetByIdAsync(id) ?? throw new NotFoundException("Product", id);

        product.Activate();
        await _productRepository.UpdateAsync(product);
        await _productRepository.SaveChangesAsync();

        _logger.LogInformation("Product {ProductId} activated", product.Id);
    }

    /// <summary>
    /// Deactivates a product (soft delete, reversible; not discontinued).
    /// </summary>
    public async Task DeactivateAsync(Guid id)
    {
        var product =
            await _productRepository.GetByIdAsync(id) ?? throw new NotFoundException("Product", id);

        product.Deactivate();
        await _productRepository.UpdateAsync(product);
        await _productRepository.SaveChangesAsync();

        _logger.LogInformation("Product {ProductId} deactivated", product.Id);
    }

    /// <summary>
    /// Marks a product as discontinued (alias for <see cref="DiscontinueAsync"/>).
    /// </summary>
    public async Task DeleteAsync(Guid id)
    {
        await DiscontinueAsync(id);
    }

    #endregion
}
