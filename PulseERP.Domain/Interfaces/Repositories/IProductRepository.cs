using PulseERP.Domain.Entities;
using PulseERP.Domain.Pagination;
using PulseERP.Domain.Query.Products;

namespace PulseERP.Domain.Interfaces.Repositories;

public interface IProductRepository
{
    Task<PaginationResult<Product>> GetAllAsync(ProductParams productParams);
    Task<Product?> GetByIdAsync(Guid id);
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(Product product);
    Task<bool> ExistsAsync(Guid id);
}
