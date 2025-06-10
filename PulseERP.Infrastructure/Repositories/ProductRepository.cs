using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PulseERP.Abstractions.Common.Filters;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Abstractions.Contracts.Repositories;
using PulseERP.Domain.Entities;
using PulseERP.Infrastructure.Database;

namespace PulseERP.Infrastructure.Repositories;

public class ProductRepository(CoreDbContext context) : IProductRepository
{
    public async Task<PagedResult<Product>> GetAllAsync(ProductFilter filter)
    {
        var query = context
            .Products.Include(p => p.Brand)
            .Include(p => p.Inventory)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var keyword = $"%{filter.Search.Trim().ToLowerInvariant()}%";
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
            var status = filter.Status.Trim();
            query = query.Where(p => p.Status.ToString() == status);
        }

        if (filter.IsService.HasValue)
        {
            query = query.Where(p => p.IsService == filter.IsService.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Brand))
        {
            var brand = filter.Brand.Trim().ToLowerInvariant();
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
            "nameasc" => query.OrderBy(p => p.Name),
            "namedesc" => query.OrderByDescending(p => p.Name),
            "stockasc" => query.OrderBy(p => p.Inventory.Quantity),
            "stockdesc" => query.OrderByDescending(p => p.Inventory.Quantity),
            _ => query.OrderBy(p => p.Name),
        };

        var totalItems = await query.CountAsync();

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
        return await context
            .Products.Include(p => p.Inventory)
            .Include(p => p.Brand)
            .AsNoTracking()
            .ToListAsync();
    }

    public Task AddAsync(Product product)
    {
        context.Products.Add(product);
        return Task.CompletedTask;
    }

    public async Task<Product?> FindByIdAsync(Guid id)
    {
        return await context.Products.Include(p => p.Inventory)
            .Include(p => p.Brand)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public Task UpdateAsync(Product product)
    {
        context.Products.Update(product);
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync()
    {
        return context.SaveChangesAsync();
    }


    public Task DeleteAsync(Product product)
    {
        context.Products.Remove(product);
        return Task.CompletedTask;
    }
}