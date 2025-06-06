using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using PulseERP.Abstractions.Common.Filters;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Abstractions.Contracts.Repositories;
using PulseERP.Domain.Entities;
using PulseERP.Infrastructure.Database;

namespace PulseERP.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly CoreDbContext _ctx;
    private readonly ILogger<ProductRepository> _logger;

    public ProductRepository(CoreDbContext ctx, ILogger<ProductRepository> logger)
    {
        _ctx = ctx;
        _logger = logger;
    }

    public async Task<PagedResult<Product>> GetAllAsync(ProductFilter filter)
    {
        var query = _ctx
            .Products.Include(p => p.Brand)
            .Include(p => p.Inventory)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            string keyword = $"%{filter.Search.Trim().ToLowerInvariant()}%";
            query = query.Where(p =>
                EF.Functions.Like(p.Name.Value.ToLower(), keyword)
                || (
                    p.Description != null
                    && EF.Functions.Like(p.Description.Value.ToLower(), keyword)
                )
            );
        }

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            string status = filter.Status.Trim();
            query = query.Where(p => p.Status.ToString() == status);
        }

        if (filter.IsService.HasValue)
        {
            query = query.Where(p => p.IsService == filter.IsService.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Brand))
        {
            string brand = filter.Brand.Trim().ToLowerInvariant();
            query = query.Where(p => p.Brand.Name.ToLower().Contains(brand));
        }

        if (filter.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price.Amount >= filter.MinPrice.Value);
        }

        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price.Amount <= filter.MaxPrice.Value);
        }

        if (filter.MinStockLevel.HasValue)
        {
            query = query.Where(p => p.Inventory.Quantity >= filter.MinStockLevel.Value);
        }

        if (filter.MaxStockLevel.HasValue)
        {
            query = query.Where(p => p.Inventory.Quantity <= filter.MaxStockLevel.Value);
        }

        query = filter.Sort?.Trim().ToLowerInvariant() switch
        {
            "priceasc" => query.OrderBy(p => p.Price.Amount),
            "pricedesc" => query.OrderByDescending(p => p.Price.Amount),
            "nameasc" => query.OrderBy(p => p.Name.Value),
            "namedesc" => query.OrderByDescending(p => p.Name.Value),
            "stockasc" => query.OrderBy(p => p.Inventory.Quantity),
            "stockdesc" => query.OrderByDescending(p => p.Inventory.Quantity),
            _ => query.OrderBy(p => p.Name.Value),
        };

        int totalItems = await query.CountAsync();

        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<Product>
        {
            Items = items,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalItems = totalItems,
        };
    }

    public async Task<IReadOnlyCollection<Product>> GetAllRawAsync()
    {
        return await _ctx
            .Products.Include(p => p.Inventory)
            .ThenInclude(i => i.Movements)
            .Include(p => p.Brand)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Product?> FindByIdAsync(Guid id)
    {
        var product = await _ctx
            .Products.Include(p => p.Brand)
            .Include(p => p.Inventory)
            .SingleOrDefaultAsync(p => p.Id == id);

        return product;
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        Product? product = await _ctx
            .Products.Include(p => p.Brand)
            .SingleOrDefaultAsync(p => p.Id == id);
        return product;
    }

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
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task DeleteAsync(Product product)
    {
        _ctx.Products.Remove(product);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<int> SaveChangesAsync() => _ctx.SaveChangesAsync();
}
