using Microsoft.EntityFrameworkCore;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Interfaces.Repositories;
using PulseERP.Domain.Pagination;
using PulseERP.Infrastructure.Database;

namespace PulseERP.Infrastructure.Repositories;

public class BrandRepository : IBrandRepository
{
    private readonly CoreDbContext _ctx;

    public BrandRepository(CoreDbContext ctx) => _ctx = ctx;

    public async Task<PaginationResult<Brand>> GetAllAsync(PaginationParams pagination)
    {
        var query = _ctx.Brands.AsNoTracking();
        var total = await query.CountAsync();
        var items = await query
            .OrderBy(b => b.Name)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        return new PaginationResult<Brand>(
            items,
            total,
            pagination.PageNumber,
            pagination.PageSize
        );
    }

    public Task<Brand?> GetByIdAsync(Guid id) =>
        _ctx.Brands.AsNoTracking().SingleOrDefaultAsync(b => b.Id == id);

    public Task<Brand?> GetByNameAsync(string name)
    {
        var trimmed = name.Trim().ToLower();
        return _ctx.Brands.AsNoTracking().SingleOrDefaultAsync(b => b.Name.ToLower() == trimmed);
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
