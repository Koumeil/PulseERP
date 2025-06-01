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

namespace PulseERP.Application.Services;

public sealed class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IBrandRepository _brandRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductService> _logger;

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
    public async Task<PagedResult<ProductSummary>> GetAllAsync(
        PaginationParams pagination,
        ProductFilter filter
    )
    {
        var paged = await _productRepository.GetAllAsync(pagination, filter);

        return new PagedResult<ProductSummary>
        {
            Items = _mapper.Map<List<ProductSummary>>(paged.Items),
            TotalItems = paged.TotalItems,
            PageNumber = paged.PageNumber,
            PageSize = paged.PageSize,
        };
    }

    public async Task<ProductDetails> GetByIdAsync(Guid id) =>
        _mapper.Map<ProductDetails>(
            await _productRepository.GetByIdAsync(id) ?? throw new NotFoundException("Product", id)
        );
    #endregion

    #region CREATE
    public async Task<ProductDetails> CreateAsync(CreateProductCommand cmd)
    {
        var brand =
            await _brandRepository.GetByNameAsync(cmd.Brand) ?? await CreateBrandAsync(cmd.Brand);

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

        _logger.LogInformation("Product {ProductId} created", product.Id);
        return _mapper.Map<ProductDetails>(product);

        async Task<Brand> CreateBrandAsync(string name)
        {
            var b = Brand.Create(name);
            await _brandRepository.AddAsync(b);
            await _brandRepository.SaveChangesAsync();
            return b;
        }
    }
    #endregion

    #region UPDATE
    public async Task<ProductDetails> UpdateAsync(Guid id, UpdateProductCommand cmd)
    {
        var product =
            await _productRepository.GetByIdAsync(id) ?? throw new NotFoundException("Product", id);

        /* ---- Marque ---- */
        if (cmd.BrandId is { } brandId && brandId != product.Brand.Id)
        {
            var newBrand =
                await _brandRepository.GetByIdAsync(brandId)
                ?? throw new NotFoundException("Brand", brandId);
            product.SetBrand(newBrand);
        }

        /* ---- Détails hors stock ---- */
        product.UpdateDetails(
            name: cmd.Name ?? product.Name.Value,
            description: cmd.Description ?? product.Description?.Value,
            price: cmd.Price ?? product.Price.Value,
            isService: cmd.IsService ?? product.IsService
        );

        /* ---- Quantité / Disponibilité ---- */
        if (cmd.Quantity is { } q && q != product.Quantity)
        {
            if (q == 0)
            {
                product.MarkOutOfStock();
            }
            else if (q > product.Quantity)
            {
                product.Restock(q - product.Quantity); // ajoute stock manquant
            }
            else // q < current && q > 0
            {
                // stratégie : repart de zéro puis restock
                product.MarkOutOfStock();
                product.Restock(q);
            }
        }

        await _productRepository.UpdateAsync(product);
        await _productRepository.SaveChangesAsync();

        _logger.LogInformation("Product {ProductId} updated", product.Id);
        return _mapper.Map<ProductDetails>(product);
    }
    #endregion

    #region STATE CHANGES
    /// <summary>Met le produit hors catalogue - status « Discontinued ».</summary>
    public async Task DiscontinueAsync(Guid id)
    {
        var product =
            await _productRepository.GetByIdAsync(id) ?? throw new NotFoundException("Product", id);

        product.Discontinue();

        await _productRepository.UpdateAsync(product);
        await _productRepository.SaveChangesAsync();

        _logger.LogInformation("Product {ProductId} discontinued", product.Id);
    }

    /// <summary>Réactive un produit précédemment désactivé/discontinué.</summary>
    public async Task ActivateAsync(Guid id)
    {
        var product =
            await _productRepository.GetByIdAsync(id) ?? throw new NotFoundException("Product", id);

        product.Activate();

        await _productRepository.UpdateAsync(product);
        await _productRepository.SaveChangesAsync();

        _logger.LogInformation("Product {ProductId} activated", product.Id);
    }

    /// <summary>Désactive un produit sans le marquer « Discontinued » (soft-delete).</summary>
    public async Task DeactivateAsync(Guid id)
    {
        var product =
            await _productRepository.GetByIdAsync(id) ?? throw new NotFoundException("Product", id);

        product.Deactivate();

        await _productRepository.UpdateAsync(product);
        await _productRepository.SaveChangesAsync();

        _logger.LogInformation("Product {ProductId} deactivated", product.Id);
    }

    public async Task DeleteAsync(Guid id)
    {
        var product =
            await _productRepository.GetByIdAsync(id) ?? throw new NotFoundException("Product", id);

        product.Discontinue();

        await _productRepository.UpdateAsync(product);
        await _productRepository.SaveChangesAsync();

        _logger.LogInformation("Product {ProductId} discontinued (deleted)", product.Id);
    }

    #endregion
}
