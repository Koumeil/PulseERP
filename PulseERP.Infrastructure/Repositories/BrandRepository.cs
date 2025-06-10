using Microsoft.EntityFrameworkCore;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Abstractions.Contracts.Repositories;
using PulseERP.Domain.Entities;
using PulseERP.Infrastructure.Database;

namespace PulseERP.Infrastructure.Repositories;

public class BrandRepository(CoreDbContext context) : IBrandRepository
{
    public async Task<PagedResult<Brand>> GetAllAsync(PaginationParams paginationParams)
    {
        var query = context.Brands.AsNoTracking().Include(b => b.Products);
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
        var brand = await context.Brands.SingleOrDefaultAsync(b => b.Id == id);
        return brand;
    }

    public async Task<Brand?> FindByNameAsync(string name)
    {
        var normalized = name.Trim().ToLowerInvariant();
        var brand = await context.Brands.SingleOrDefaultAsync(b =>
            b.Name == normalized);
        return brand;
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null)
    {
        var normalized = name.Trim().ToLowerInvariant();
        return await context.Brands.AnyAsync(b =>
            b.Name.Trim() == normalized
            && (!excludeId.HasValue || b.Id != excludeId.Value)
        );
    }

    public Task AddAsync(Brand brand)
    {
        context.Brands.Add(brand);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Brand brand)
    {
        context.Brands.Update(brand);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Brand brand)
    {
        context.Brands.Remove(brand);
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync() => context.SaveChangesAsync();

}