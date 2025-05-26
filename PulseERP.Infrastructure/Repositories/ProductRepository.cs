using Microsoft.EntityFrameworkCore;
using PulseERP.Contracts.Interfaces.Services;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Interfaces.Repositories;
using PulseERP.Domain.Pagination;
using PulseERP.Domain.Query.Products;
using PulseERP.Infrastructure.Database;

namespace PulseERP.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly CoreDbContext _context;
    private readonly ISerilogAppLoggerService<ProductRepository> _logger;

    public ProductRepository(
        CoreDbContext context,
        ISerilogAppLoggerService<ProductRepository> logger
    )
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        try
        {
            return await _context
                .Products.Include(p => p.Brand)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching product by ID: {id}", ex);
            throw;
        }
    }

    public async Task<PaginationResult<Product>> GetAllAsync(ProductParams productParams)
    {
        var query = _context.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(productParams.Brand))
            query = query.Where(product => product.Brand.Name == productParams.Brand);

        if (!string.IsNullOrWhiteSpace(productParams.Search))
        {
            var keyword = $"%{productParams.Search}%";
            query = query.Where(p =>
                EF.Functions.Like(p.Name, keyword) || EF.Functions.Like(p.Description, keyword)
            );
        }

        query = productParams.Sort switch
        {
            "priceAsc" => query.OrderBy(product => product.Price),
            "priceDesc" => query.OrderByDescending(product => product.Price),
            _ => query.OrderBy(product => product.Name),
        };

        // Compter le total avant pagination
        var totalItems = await query.CountAsync();

        // Pagination
        var items = await query
            .Skip((productParams.PageNumber - 1) * productParams.PageSize)
            .Take(productParams.PageSize)
            .ToListAsync();

        return new PaginationResult<Product>(
            items,
            totalItems,
            productParams.PageNumber,
            productParams.PageSize
        );
    }

    public async Task AddAsync(Product product)
    {
        try
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            _context.Entry(product).State = EntityState.Detached;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error adding product {product.Id}", ex);
            throw;
        }
    }

    public async Task UpdateAsync(Product product)
    {
        try
        {
            var existing = await _context.Products.FirstOrDefaultAsync(p => p.Id == product.Id);

            if (existing is null)
            {
                _logger.LogWarning($"Product {product.Id} not found for update");
                throw new KeyNotFoundException($"Product {product.Id} not found");
            }

            _context.Entry(existing).CurrentValues.SetValues(product);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Product {product.Id} updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating product {product.Id}", ex);
            throw;
        }
    }

    public async Task DeleteAsync(Product product)
    {
        try
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Product {product.Id} deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting product {product.Id}", ex);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        try
        {
            return await _context.Products.AnyAsync(p => p.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error checking existence for product {id}", ex);
            throw;
        }
    }
}
