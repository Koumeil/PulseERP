using Microsoft.EntityFrameworkCore;
using PulseERP.Application.Common.Interfaces;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Interfaces.Persistence;
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
            return await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching product by ID: {id}", ex);
            throw;
        }
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync()
    {
        try
        {
            return await _context.Products.AsNoTracking().ToListAsync() as IReadOnlyList<Product>;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error fetching all products", ex);
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
