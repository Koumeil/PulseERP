using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Abstractions.Contracts.Repositories;
using PulseERP.Domain.Entities;
using PulseERP.Infrastructure.Database;

namespace PulseERP.Infrastructure.Repositories;

public class BrandRepository : IBrandRepository
{
    private readonly CoreDbContext _ctx;
    private readonly ILogger<Brand> _logger;

    public BrandRepository(CoreDbContext ctx, ILogger<Brand> logger)
    {
        _ctx = ctx;
        _logger = logger;
    }

    public async Task<PagedResult<Brand>> GetAllAsync(PaginationParams paginationParams)
    {
        var query = _ctx.Brands.AsNoTracking().Include(b => b.Products);
        int total = await query.CountAsync();

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
        Brand? brand = await _ctx.Brands.SingleOrDefaultAsync(b => b.Id == id);
        return brand;
    }

    public async Task<Brand?> FindByNameAsync(string name)
    {
        string normalized = name.Trim().ToLowerInvariant();
        Brand? brand = await _ctx.Brands.SingleOrDefaultAsync(b =>
            b.Name.Trim().ToLower() == normalized
        );
        return brand;
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null)
    {
        string normalized = name.Trim().ToLowerInvariant();
        return await _ctx.Brands.AnyAsync(b =>
            b.Name.Trim().ToLower() == normalized
            && (!excludeId.HasValue || b.Id != excludeId.Value)
        );
    }

    public Task AddAsync(Brand brand)
    {
        _ctx.Brands.Add(brand);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Brand brand)
    {
        _ctx.Brands.Update(brand);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Brand brand)
    {
        _ctx.Brands.Remove(brand);
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync() => _ctx.SaveChangesAsync();
}
