using Microsoft.EntityFrameworkCore;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Interfaces.Repositories;
using PulseERP.Domain.Pagination;
using PulseERP.Infrastructure.Database;

namespace PulseERP.Infrastructure.Repositories;

public class BrandRepository : IBrandRepository
{
    private readonly CoreDbContext _context;

    public BrandRepository(CoreDbContext context)
    {
        _context = context;
    }

    public async Task<Brand?> GetByIdAsync(Guid id)
    {
        return await _context.Brands.FindAsync(id);
    }

    public async Task<Brand?> GetByNameAsync(string name)
    {
        return await _context.Brands.FirstOrDefaultAsync(b => b.Name == name);
    }

    public async Task<PaginationResult<Brand>> GetAllAsync(PaginationParams paginationParams)
    {
        var query = _context.Brands.AsNoTracking();

        var totalItems = await query.CountAsync();

        var items = await query
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync();

        return new PaginationResult<Brand>(
            items,
            totalItems,
            paginationParams.PageNumber,
            paginationParams.PageSize
        );
    }

    public async Task<Brand> AddAsync(Brand brand)
    {
        await _context.Brands.AddAsync(brand);
        await _context.SaveChangesAsync();
        return brand;
    }

    public async Task UpdateAsync(Brand brand)
    {
        _context.Brands.Update(brand);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var brand = await GetByIdAsync(id);
        if (brand != null)
        {
            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Brands.AnyAsync(b => b.Id == id);
    }

    public async Task<bool> NameExistsAsync(string name)
    {
        return await _context.Brands.AnyAsync(b => b.Name == name);
    }
}
