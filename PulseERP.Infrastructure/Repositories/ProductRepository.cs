using Microsoft.EntityFrameworkCore;
using PulseERP.Contracts.Services;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Filters.Products;
using PulseERP.Domain.Repositories;
using PulseERP.Infrastructure.Persistence;

namespace PulseERP.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IAppLogger<ProductRepository> _logger;

    public ProductRepository(ApplicationDbContext context, IAppLogger<ProductRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        try
        {
            return await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching product by ID: {id}", ex);
            throw;
        }
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(ProductParams productParams)
    {
        try
        {
            var query = _context.Products.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(productParams.Brand))
                query = query.Where(p => p.Brand.Name == productParams.Brand);

            if (!string.IsNullOrWhiteSpace(productParams.Search))
            {
                var keyword = $"%{productParams.Search}%";
                query = query.Where(p =>
                    EF.Functions.Like(p.Name, keyword) || EF.Functions.Like(p.Description, keyword)
                );
            }

            // Tri
            query = productParams.Sort switch
            {
                "priceAsc" => query.OrderBy(p => p.Price),
                "priceDesc" => query.OrderByDescending(p => p.Price),
                _ => query.OrderBy(p => p.Name),
            };

            // Pagination 
            query = query
                .Skip((productParams.PageCount - 1) * productParams.PageSize)
                .Take(productParams.PageSize);

            return await query.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error fetching all products",ex);
            throw;
        }
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
