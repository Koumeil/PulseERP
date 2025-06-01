using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using PulseERP.Abstractions.Common.Filters;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Interfaces;
using PulseERP.Infrastructure.Database;

namespace PulseERP.Infrastructure.Repositories;

/// <summary>
/// Repository for <see cref="Product"/> entities, with Redis caching on <c>GetByIdAsync</c>.
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly CoreDbContext _ctx;
    private readonly ILogger<ProductRepository> _logger;
    private readonly IDistributedCache _cache;
    private const string ProductByIdKeyTemplate = "ProductRepository:Id:{0}";

    /// <summary>
    /// Initializes a new instance of <see cref="ProductRepository"/>.
    /// </summary>
    /// <param name="ctx">EF Core DB context.</param>
    /// <param name="logger">Logger instance.</param>
    /// <param name="cache">Redis distributed cache.</param>
    public ProductRepository(
        CoreDbContext ctx,
        ILogger<ProductRepository> logger,
        IDistributedCache cache
    )
    {
        _ctx = ctx;
        _logger = logger;
        _cache = cache;
    }

    /// <inheritdoc/>
    public async Task<PagedResult<Product>> GetAllAsync(
        PaginationParams paginationParams,
        ProductFilter productFilter
    )
    {
        var query = _ctx.Products.Include(p => p.Brand).AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(productFilter.Brand))
        {
            string brandFilter = productFilter.Brand.Trim().ToLowerInvariant();
            query = query.Where(p => p.Brand.Name.ToLower().Contains(brandFilter));
        }

        if (!string.IsNullOrWhiteSpace(productFilter.Search))
        {
            string keyword = $"%{productFilter.Search.Trim().ToLower()}%";
            query = query.Where(p =>
                EF.Functions.Like(p.Name, keyword)
                || (p.Description != null && EF.Functions.Like(p.Description, keyword))
            );
        }

        query = productFilter.Sort switch
        {
            "priceAsc" => query.OrderBy(p => p.Price.Value),
            "priceDesc" => query.OrderByDescending(p => p.Price.Value),
            _ => query.OrderBy(p => p.Name),
        };

        int total = await query.CountAsync();
        var items = await query
            .Skip((productFilter.PageNumber - 1) * productFilter.PageSize)
            .Take(productFilter.PageSize)
            .ToListAsync();

        return new PagedResult<Product>
        {
            Items = items,
            PageNumber = productFilter.PageNumber,
            PageSize = productFilter.PageSize,
            TotalItems = total,
        };
    }

    /// <inheritdoc/>
    public async Task<Product?> GetByIdAsync(Guid id)
    {
        Product? product = await _ctx
            .Products.Include(p => p.Brand)
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.Id == id);
        return product;
    }

    /// <inheritdoc/>
    public Task AddAsync(Product product)
    {
        _ctx.Products.Add(product);
        _logger.LogInformation("Product {ProductId} added to context", product.Id);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task UpdateAsync(Product product)
    {
        _ctx.Products.Update(product);
        _logger.LogInformation("Product {ProductId} updated in context", product.Id);
        string cacheKey = string.Format(ProductByIdKeyTemplate, product.Id);
        _cache.Remove(cacheKey);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task DeleteAsync(Product product)
    {
        _ctx.Products.Remove(product);
        _logger.LogInformation("Product {ProductId} removed from context", product.Id);
        string cacheKey = string.Format(ProductByIdKeyTemplate, product.Id);
        _cache.Remove(cacheKey);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<int> SaveChangesAsync() => _ctx.SaveChangesAsync();
}
