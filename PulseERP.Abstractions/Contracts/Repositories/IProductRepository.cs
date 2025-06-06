using PulseERP.Abstractions.Common.Filters;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Domain.Entities;

namespace PulseERP.Abstractions.Contracts.Repositories;

public interface IProductRepository
{
    Task<IReadOnlyCollection<Product>> GetAllRawAsync();
    Task<PagedResult<Product>> GetAllAsync(ProductFilter productFilter);
    Task<Product?> FindByIdAsync(Guid id);
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(Product product);
    Task<int> SaveChangesAsync();
}
