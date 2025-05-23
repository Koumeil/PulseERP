using PulseERP.Domain.Entities;

namespace PulseERP.Domain.Interfaces.Persistence;

public interface IProductRepository
{
    Task AddAsync(Product product);
    Task DeleteAsync(Product product);
    Task<bool> ExistsAsync(Guid id);
    Task<IReadOnlyList<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(Guid id);
    Task UpdateAsync(Product product);
}
