using PulseERP.Domain.Entities;
using PulseERP.Domain.Filters.Products;

namespace PulseERP.Domain.Interfaces.Repositories;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetAllAsync(ProductParams productParams);
    Task<Product?> GetByIdAsync(Guid id);
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(Product product);
    Task<bool> ExistsAsync(Guid id);
}
