using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Interfaces;
using PulseERP.Infrastructure.Database;

namespace PulseERP.Infrastructure.Repositories;

/// <summary>
/// Repository for <see cref="Brand"/> entities, with Redis caching on read operations.
/// </summary>
public class BrandRepository : IBrandRepository
{
    private readonly CoreDbContext _ctx;
    private readonly IDistributedCache _cache;
    private const string BrandByIdKeyTemplate = "BrandRepository:Id:{0}";
    private const string BrandByNameKeyTemplate = "BrandRepository:Name:{0}";

    /// <summary>
    /// Initializes a new instance of <see cref="BrandRepository"/>.
    /// </summary>
    /// <param name="ctx">EF Core DB context.</param>
    /// <param name="cache">Redis distributed cache.</param>
    public BrandRepository(CoreDbContext ctx, IDistributedCache cache)
    {
        _ctx = ctx;
        _cache = cache;
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public async Task<Brand?> GetByIdAsync(Guid id)
    {
        Brand? brand = await _ctx.Brands.AsNoTracking().SingleOrDefaultAsync(b => b.Id == id);
        return brand;
    }

    /// <inheritdoc/>
    public async Task<Brand?> GetByNameAsync(string name)
    {
        string normalized = name.Trim().ToLowerInvariant();
        Brand? brand = await _ctx
            .Brands.AsNoTracking()
            .SingleOrDefaultAsync(b => b.Name.Trim().ToLower() == normalized);
        return brand;
    }

    /// <inheritdoc/>
    public Task AddAsync(Brand brand)
    {
        _ctx.Brands.Add(brand);
        string idKey = string.Format(BrandByIdKeyTemplate, brand.Id);
        string nameKey = string.Format(
            BrandByNameKeyTemplate,
            brand.Name.Trim().ToLowerInvariant()
        );
        _cache.Remove(idKey);
        _cache.Remove(nameKey);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task UpdateAsync(Brand brand)
    {
        _ctx.Brands.Update(brand);
        string idKey = string.Format(BrandByIdKeyTemplate, brand.Id);
        string nameKey = string.Format(
            BrandByNameKeyTemplate,
            brand.Name.Trim().ToLowerInvariant()
        );
        _cache.Remove(idKey);
        _cache.Remove(nameKey);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task DeleteAsync(Brand brand)
    {
        _ctx.Brands.Remove(brand);
        string idKey = string.Format(BrandByIdKeyTemplate, brand.Id);
        string nameKey = string.Format(
            BrandByNameKeyTemplate,
            brand.Name.Trim().ToLowerInvariant()
        );
        _cache.Remove(idKey);
        _cache.Remove(nameKey);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<int> SaveChangesAsync() => _ctx.SaveChangesAsync();
}
