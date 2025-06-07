using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Abstractions.Contracts.Repositories;
using PulseERP.Domain.Entities;
using PulseERP.Infrastructure.Database;

namespace PulseERP.Infrastructure.Repositories;

public class BrandRepository(CoreDbContext ctx) : IBrandRepository
{
    public async Task<PagedResult<Brand>> GetAllAsync(PaginationParams paginationParams)
    {
        var query = ctx.Brands.AsNoTracking().Include(b => b.Products);
        var total = await query.CountAsync();

        var items = await query
            .OrderBy(b => b.Name)
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync();

        return new PagedResult<Brand>
        {
            Items = items,
            PageNumber = paginationParams.PageNumber,
            PageSize = paginationParams.PageSize,
            TotalItems = total,
        };
    }

    public async Task<Brand?> FindByIdAsync(Guid id)
    {
        var brand = await ctx.Brands.SingleOrDefaultAsync(b => b.Id == id);
        return brand;
    }

    public async Task<Brand?> FindByNameAsync(string name)
    {
        var normalized = name.Trim().ToLowerInvariant();
        var brand = await ctx.Brands.SingleOrDefaultAsync(b =>
            b.Name.Trim().Equals(normalized
                , StringComparison.CurrentCultureIgnoreCase)
        );
        return brand;
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null)
    {
        var normalized = name.Trim().ToLowerInvariant();
        return await ctx.Brands.AnyAsync(b =>
            b.Name.Trim().Equals(normalized
                , StringComparison.CurrentCultureIgnoreCase)
            && (!excludeId.HasValue || b.Id != excludeId.Value)
        );
    }

    public Task AddAsync(Brand brand)
    {
        ctx.Brands.Add(brand);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Brand brand)
    {
        ctx.Brands.Update(brand);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Brand brand)
    {
        ctx.Brands.Remove(brand);
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync() => ctx.SaveChangesAsync();
}