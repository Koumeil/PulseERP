using Microsoft.EntityFrameworkCore;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Interfaces;
using PulseERP.Infrastructure.Database;

namespace PulseERP.Infrastructure.Repositories;

public class BrandRepository : IBrandRepository
{
    private readonly CoreDbContext _ctx;

    public BrandRepository(CoreDbContext ctx) => _ctx = ctx;

    public async Task<PagedResult<Brand>> GetAllAsync(PaginationParams paginationParams)
    {
        var query = _ctx.Brands.AsNoTracking();
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
            TotalItems = total
        };
    }

    public Task<Brand?> GetByIdAsync(Guid id) =>
        _ctx.Brands.AsNoTracking().SingleOrDefaultAsync(b => b.Id == id);

    public async Task<Brand?> GetByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        var normalized = name.Trim().ToLower();

        return await _ctx.Brands.SingleOrDefaultAsync(b => b.Name.Trim().ToLower() == normalized);
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
