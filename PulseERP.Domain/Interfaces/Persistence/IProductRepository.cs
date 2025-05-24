using PulseERP.Domain.Entities;
using PulseERP.Domain.Filter;

namespace PulseERP.Domain.Interfaces.Persistence;

public interface IProductRepository
{
    Task AddAsync(Product product);
    Task DeleteAsync(Product product);
    Task<bool> ExistsAsync(Guid id);
    Task<IReadOnlyList<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(Guid id);
    Task UpdateAsync(Product product);
    Task<IReadOnlyList<Product>> FilterAsync(ProductFilterRequest filter);
}
