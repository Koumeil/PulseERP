using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Interfaces.Repositories;
using PulseERP.Domain.Pagination;
using PulseERP.Domain.Query.Products;
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

    public async Task<PaginationResult<Product>> GetAllAsync(
        PaginationParams pagination,
        ProductParams productParams
    )
    {
        // 1. On construit la requête de base, incluant Brand et en lecture seule
        var query = _ctx.Products.Include(p => p.Brand).AsNoTracking().AsQueryable();

        // 2. Filtre sur le nom de la marque (Contains sur la colonne Brand.Name)
        if (!string.IsNullOrWhiteSpace(productParams.Brand))
        {
            var brandFilter = productParams.Brand.Trim().ToLower();
            query = query.Where(p => p.Brand.Name.ToLower().Contains(brandFilter));
        }

        // 3. Recherche “full‐text” LINQ portable sur Name ou Description
        if (!string.IsNullOrWhiteSpace(productParams.Search))
        {
            var keyword = $"%{productParams.Search.Trim().ToLower()}%";

            query = query.Where(p =>
                EF.Functions.Like(p.Name, keyword)
                || (p.Description != null && EF.Functions.Like(p.Description, keyword))
            );
        }

        // 4. Tri dynamique (OrderBy sur p.Price.Value ou sur p.Name directement)
        query = productParams.Sort switch
        {
            "priceAsc" => query.OrderBy(p => p.Price.Value),
            "priceDesc" => query.OrderByDescending(p => p.Price.Value),
            _ => query.OrderBy(p => p.Name), // Trier sur p.Name (conversion en string)
        };

        // 5. Comptage total avant pagination
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

    public async Task<Product?> GetByIdAsync(Guid id) =>
        await _ctx
            .Products.Include(p => p.Brand)
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.Id == id);

    public Task AddAsync(Product product)
    {
        _ctx.Products.Add(product);
        _logger.LogInformation("Product {ProductId} added to context", product.Id);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Product product)
    {
        _ctx.Products.Update(product);
        _logger.LogInformation("Product {ProductId} updated in context", product.Id);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Product product)
    {
        _ctx.Products.Remove(product);
        _logger.LogInformation("Product {ProductId} removed from context", product.Id);
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync() => _ctx.SaveChangesAsync();
}
