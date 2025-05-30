using Microsoft.EntityFrameworkCore;
using PulseERP.Domain.Interfaces.Repositories;
using PulseERP.Domain.Pagination;
using PulseERP.Domain.Query.Products;
using PulseERP.Infrastructure.Database;

namespace PulseERP.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly CoreDbContext _ctx;

    public ProductRepository(CoreDbContext ctx) => _ctx = ctx;

    public async Task<PaginationResult<Product>> GetAllAsync(
        PaginationParams pagination,
        ProductParams productParams
    )
    {
        // 1. On commence la query
        var query = _ctx.Products.Include(p => p.Brand).AsNoTracking().AsQueryable();

        // 2. Filtre par nom de marque si fourni
        if (!string.IsNullOrWhiteSpace(productParams.Brand))
            query = query.Where(p => p.Brand.Name.Contains(productParams.Brand));

        // 3. Recherche full-text simple sur nom et description
        if (!string.IsNullOrWhiteSpace(productParams.Search))
        {
            var keyword = $"%{productParams.Search}%";
            query = query.Where(p =>
                EF.Functions.Like(p.Name, keyword) || EF.Functions.Like(p.Description!, keyword)
            );
        }

        // 4. Tri selon productParams.Sort
        query = productParams.Sort switch
        {
            "priceAsc" => query.OrderBy(p => p.Price.Value),
            "priceDesc" => query.OrderByDescending(p => p.Price.Value),
            _ => query.OrderBy(p => p.Name),
        };

        // 5. Comptage avant pagination
        var totalItems = await query.CountAsync();

        // 6. Pagination
        var items = await query
            .Skip((productParams.PageNumber - 1) * productParams.PageSize)
            .Take(productParams.PageSize)
            .ToListAsync();

        return new PaginationResult<Product>(
            items,
            totalItems,
            pagination.PageNumber,
            pagination.PageSize
        );
    }

    public Task<Product?> GetByIdAsync(Guid id) =>
        _ctx.Products.Include(p => p.Brand).AsNoTracking().SingleOrDefaultAsync(p => p.Id == id);

    public Task AddAsync(Product product)
    {
        _ctx.Products.Add(product);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Product product)
    {
        _ctx.Products.Update(product);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Product product)
    {
        _ctx.Products.Remove(product);
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync() => _ctx.SaveChangesAsync();
}
